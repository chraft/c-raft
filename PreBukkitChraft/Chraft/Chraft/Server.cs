using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mime;
using System.Text;
using System.Net.Sockets;
using Chraft.Interfaces;
using Chraft.Interfaces.Recipes;
using Chraft.Net.Packets;
using Chraft.Properties;
using System.Net;
using System.Threading;
using Chraft.Utils;
using Chraft.World;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Plugins;
using System.Diagnostics;
using Chraft.Resources;
using Chraft.Irc;

namespace Chraft
{
	public class Server
	{
		private readonly PermissionConfiguration Permissions = null;
		private volatile int NextEntityId = 0;
		private bool Running = true;
		private TcpListener Tcp = null;

		/// <summary>
		/// Invoked when a client is accepted and started.
		/// </summary>
		public event EventHandler<ClientEventArgs> Joined;

		/// <summary>
		/// Invoked whenever a user sends a command.
		/// </summary>
		public event EventHandler<CommandEventArgs> Command;

		/// <summary>
		/// Triggered every ten seconds in a dedicated thread.
		/// </summary>
		public event EventHandler Pulse;

		/// <summary>
		/// Invoked prior to a client being accepted, before any data is transcieved.
		/// </summary>
		public event EventHandler<TcpEventArgs> BeforeAccept;

		/// <summary>
		/// Gets the PluginManager for the server.
		/// </summary>
		public PluginManager PluginManager { get; private set; }

		/// <summary>
		/// Gets a thread-unsafe list of clients.  Use GetClients for a thread-safe version.
		/// </summary>
		public Dictionary<int, Client> Clients { get; private set; }

		/// <summary>
		/// Gets a thread-unsafe list of worlds.  Use GetWorlds for a thread-safe version.
		/// </summary>
		public List<WorldManager> Worlds { get; private set; }

		/// <summary>
		/// Gets a random number generator for the server.
		/// </summary>
		public Random Rand { get; private set; }

		/// <summary>
		/// Gets a thread-unsafe list of entities on the server.  Use GetEntities for a thread-safe version.
		/// </summary>
		public List<EntityBase> Entities { get { return _Entities; } }
		private readonly List<EntityBase> _Entities = new List<EntityBase>();

		/// <summary>
		/// Gets the server Logger instance for console and file logging.
		/// </summary>
		public Logger Logger { get; private set; }

		/// <summary>
		/// Gets user-chosen item names, numerics, and durabilities.
		/// </summary>
		public ItemDb Items { get; private set; }

		/// <summary>
		/// Gets a list of user-defined recipes known to the server.
		/// </summary>
		public Recipe[] Recipes { get; private set; }

		/// <summary>
		/// Gets the IRC client, if it has been initialized.
		/// </summary>
		public IrcClient Irc { get; private set; }

		internal Server()
		{
			Clients = new Dictionary<int, Client>();
			Rand = new Random();
			Logger = new Logger(Settings.Default.LogFile);
			Permissions = new PermissionConfiguration(this);
			PluginManager = new PluginManager(Settings.Default.PluginFolder);
			Items = new ItemDb(Settings.Default.ItemsFile);
			Recipes = Recipe.FromFile(Settings.Default.RecipesFile);
			if (Settings.Default.IrcEnabled)
				InitializeIrc();
		}

		private void InitializeIrc()
		{
			IPEndPoint ep = new IPEndPoint(Dns.GetHostEntry(Settings.Default.IrcServer).AddressList[0], Settings.Default.IrcPort);
			Irc = new IrcClient(ep, Settings.Default.IrcNickname);
			Irc.Received += new IrcEventHandler(Irc_Received);
		}

		private void Irc_Received(object sender, IrcEventArgs e)
		{
			if (e.Handled)
				return;

			switch (e.Command)
			{
			case "PRIVMSG": OnIrcPrivMsg(sender, e); break;
			case "NOTICE": OnIrcNotice(sender, e); break;
			case "001": OnIrcWelcome(sender, e); break;
			}
		}

		private void OnIrcPrivMsg(object sender, IrcEventArgs e)
		{
			for (int i = 0; i < e.Args[1].Length; i++)
				if (!Settings.Default.AllowedChatChars.Contains(e.Args[1][i]))
					return;

			Broadcast("§7[IRC] " + e.Prefix.Nickname + ":§f " + e.Args[1], sendToIrc: false);
			e.Handled = true;
		}

		private void OnIrcNotice(object sender, IrcEventArgs e)
		{
			for (int i = 0; i < e.Args[1].Length; i++)
				if (!Settings.Default.AllowedChatChars.Contains(e.Args[1][i]))
					return;

			Broadcast("§c[IRC] " + e.Prefix.Nickname + ":§f " + e.Args[1], sendToIrc: false);
			e.Handled = true;
		}

		private void OnIrcWelcome(object sender, IrcEventArgs e)
		{
			Irc.Join(Settings.Default.IrcChannel);
		}

		internal void Run()
		{
			Logger.Log(Logger.LogLevel.Info, "Starting C#raft...");
		

			Worlds = new List<WorldManager>();
			Worlds.Add(new WorldManager(this));

			PluginManager.LoadDefaultAssemblies();

			while (Running)
				RunProc();
		}

		/// <summary>
		/// Creates a new world.
		/// </summary>
		/// <param name="name">The name of the folder to contain and identify the world.</param>
		/// <returns>The newly created world.</returns>
		public WorldManager CreateWorld(string name)
		{
			WorldManager world = new WorldManager(this);
			lock (Worlds)
				Worlds.Add(world);
			return world;
		}

		private void RunProc()
		{
			Logger.Log(Logger.LogLevel.Info, "Using IP Addresss {0}.", Settings.Default.IPAddress);
			Logger.Log(Logger.LogLevel.Info, "Listening on port {0}.", Settings.Default.Port);
			RunListener();

			if (Running)
			{
				Logger.Log(Logger.LogLevel.Info, "Waiting one second before restarting listener.");
				Thread.Sleep(1000);
			}
		}

		private bool StartTcp()
		{
			try
			{
				Tcp = new TcpListener(IPAddress.Parse(Settings.Default.IPAddress), Settings.Default.Port);
				Tcp.Start(5);
				return true;
			}
			catch (Exception ex)
			{
				Logger.Log(Logger.LogLevel.Error, "Could not listen: {0}", ex.Message);
				Logger.Log(Logger.LogLevel.Info, "Waiting one second before retrying.");
				return false;
			}
		}

		private void AcceptClient()
		{
			TcpClient tcp = Tcp.AcceptTcpClient();
			if (OnBeforeAccept(tcp))
			{
				Client c = new Client(this, AllocateEntity(), tcp);

				lock (Clients)
					Clients.Add(c.EntityId, c);
				lock (Entities)
					Entities.Add(c);
				c.Start();
				OnJoined(c);
			}
			else
			{
				tcp.Close();
			}
		}

		private void OnJoined(Client c)
		{
			if (Joined != null)
				Joined.Invoke(this, new ClientEventArgs(c));
		}

		private bool OnBeforeAccept(TcpClient tcp)
		{
			if (BeforeAccept != null)
			{
				TcpEventArgs e = new TcpEventArgs(tcp);
				BeforeAccept.Invoke(this, e);
				return !e.Cancelled;
			}
			return true;
		}

		private void StopTcp()
		{
			try
			{
				Logger.Log(Logger.LogLevel.Info, "Stopping listener...");
				Tcp.Stop();
				Logger.Log(Logger.LogLevel.Info, "Listener stopped.");
			}
			catch
			{
				Logger.Log(Logger.LogLevel.Info, "Listener already stopped.");
			}
		}

		private void RunListener()
		{
			if (!StartTcp())
				return;
			Logger.Log(Logger.LogLevel.Info, "Ready to accept clients.");

			try
			{
				while (Tcp.Server.IsBound)
					AcceptOrWait();
			}
			catch (Exception ex)
			{
				Logger.Log(ex);
			}
			finally
			{
				StopTcp();
			}
		}

		private void AcceptOrWait()
		{
			if (Tcp.Pending())
			{
				AcceptClient();
				Thread.Sleep(300);
			}
			else
			{
				Thread.Sleep(10);
			}
		}

		/// <summary>
		/// Allocate a new entity ID.
		/// </summary>
		/// <returns>A new entity ID reserved for a new entity.</returns>
		public int AllocateEntity()
		{
			return NextEntityId++;
		}

		/// <summary>
		/// Broadcasts a message to all clients, optionally excluding the sender.
		/// </summary>
		/// <param name="message">The message to be broadcasted.</param>
		/// <param name="excludeClient">The client to excluded; usually the sender.</param>
		public void Broadcast(string message, Client excludeClient = null, bool sendToIrc = true)
		{
			foreach (Client c in GetClients())
			{
				if (c != excludeClient)
					c.SendMessage(message);
			}

			if (sendToIrc && Irc != null)
			{
				Irc.WriteLine("PRIVMSG {0} :{1}", Settings.Default.IrcChannel, message.Replace('§', '\x3'));
			}
		}

		/// <summary>
		/// Broadcasts a packet to all clients, optionally excluding the sender.
		/// </summary>
		/// <param name="packet">The packet to be broadcasted.</param>
		/// <param name="excludeClient">The client to excluded; usually the sender.</param>
		public void Broadcast(Packet packet, Client excludeClient = null)
		{
			foreach (Client c in GetClients())
			{
				if (excludeClient == null || c.EntityId != excludeClient.EntityId)
					c.SendPacket(packet);
			}
		}

		/// <summary>
		/// Gets a thread-safe array of connected clients.
		/// </summary>
		/// <returns>A thread-safe array of connected clients.</returns>
		public Client[] GetClients()
		{
			return Clients.Values.ToArray();
		}

		public IEnumerable<Client> GetClients(string name)
		{
			return from c in GetClients()
				   where c.Username.ToLower().Contains(name.ToLower()) || c.DisplayName.ToLower().Contains(name.ToLower())
				   select c;
		}

		/// <summary>
		/// Gets a thread-safe array of all entities, including clients.
		/// </summary>
		/// <returns>A thread-safe array of all active entities.</returns>
		public EntityBase[] GetEntities()
		{
			return Entities.ToArray();
		}

		/// <summary>
		/// Yields an enumerable of nearby players, thread-safe.
		/// </summary>
		/// <param name="world">The world containing the coordinates.</param>
		/// <param name="x">The center X coordinate.</param>
		/// <param name="y">The center Y coordinate.</param>
		/// <param name="z">The center Z coordinate.</param>
		/// <returns>A lazy enumerable of nearby players.</returns>
		public IEnumerable<Client> GetNearbyPlayers(WorldManager world, double x, double y, double z)
		{
			int radius = Settings.Default.SightRadius << 4;
			foreach (Client c in GetClients())
			{
				if (c.World == world && Math.Abs(x - c.X) <= radius && Math.Abs(z - c.Z) <= radius)
					yield return c;
			}
		}

		/// <summary>
		/// Yields an enumerable of nearby entities, including players.  Thread-safe.
		/// </summary>
		/// <param name="world">The world containing the coordinates.</param>
		/// <param name="x">The center X coordinate.</param>
		/// <param name="y">The center Y coordinate.</param>
		/// <param name="z">The center Z coordinate.</param>
		/// <returns>A lazy enumerable of nearby entities.</returns>
		public IEnumerable<EntityBase> GetNearbyEntities(WorldManager world, double x, double y, double z)
		{
			int radius = Settings.Default.SightRadius << 4;
			foreach (EntityBase e in GetEntities())
			{
				if (e.World == world && Math.Abs(x - e.X) <= radius && Math.Abs(z - e.Z) <= radius)
					yield return e;
			}
		}

		/// <summary>
		/// Drops an item based on the given player's position and rotation.
		/// </summary>
		/// <param name="client">The client to be used for position calculations.</param>
		/// <param name="stack">The stack to be dropped.</param>
		/// <returns>The entity ID of the item drop.</returns>
		public int DropItem(Client client, ItemStack stack)
		{
			return DropItem(client.World, (int)client.X, (int)client.Y, (int)client.Z, stack);
		}

		/// <summary>
		/// Drops an item at the given location.
		/// </summary>
		/// <param name="world">The world in which the coordinates reside.</param>
		/// <param name="x">The target X coordinate.</param>
		/// <param name="y">The target Y coordinate.</param>
		/// <param name="z">The target Z coordinate.</param>
		/// <param name="stack">The stack to be dropped</param>
		/// <returns>The entity ID of the item drop.</returns>
		public int DropItem(WorldManager world, int x, int y, int z, ItemStack stack)
		{
			int entityId = AllocateEntity();
			Entities.Add(new ItemEntity(this, entityId)
			{
				World = world,
				X = x + 0.5,
				Y = y,
				Z = z + 0.5,
				ItemId = stack.Type,
				Count = stack.Count,
				Durability = stack.Durability
			});
			return entityId;
		}

		/// <summary>
		/// Gets a thread-safe array of worlds.
		/// </summary>
		/// <returns>A thread-safe array of WorldManager objects.</returns>
		public WorldManager[] GetWorlds()
		{
			return Worlds.ToArray();
		}

		/// <summary>
		/// Pulses separated by almost exactly ten seconds.  Should be run in a standalone thread, as it could
		/// take some time to process.  Thread-safe, locking.
		/// </summary>
		internal void DoPulse()
		{
			foreach (Client c in GetClients())
				c.SendPulse();
			OnPulse();
		}

		private void OnPulse()
		{
			if (Pulse != null)
				Pulse.Invoke(this, new EventArgs());
		}

		/// <summary>
		/// Gets the default/main world of the server in a thread-safe fashion.
		/// </summary>
		/// <returns>The default world of the server.</returns>
		public WorldManager GetDefaultWorld()
		{
			return Worlds[0];
		}

		/// <summary>
		/// Saves the current state and stops the server.
		/// </summary>
		public void Stop()
		{
			foreach (Client c in GetClients())
				c.Kick("Server is shutting down.");
			foreach (WorldManager w in GetWorlds())
				w.Dispose();
			Running = false;
			if (Tcp != null && Tcp.Server.IsBound)
				Tcp.Stop();
			if (Irc != null)
				Irc.Stop();
		}

		internal void OnCommand(Client client, string[] tokens)
		{
			if (Command != null)
				Command.Invoke(client, new CommandEventArgs(client, tokens));
		}
	}
}
