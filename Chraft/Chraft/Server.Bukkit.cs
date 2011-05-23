using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using org.bukkit;
using java.net;
using org.bukkit.entity;
using org.bukkit.util;
using org.bukkit.inventory;
using org.bukkit.command;
using org.bukkit.block;
using org.bukkit.material;
using java.util;
using Chraft.World;
using Chraft.Properties;
using org.bukkit.plugin;
using com.avaje.ebean.config;
using java.io;
using org.bukkit.scheduler;

namespace Chraft
{
	public partial class Server : org.bukkit.Server
	{
		private readonly CommandMap Commands;

		public PluginManager BukkitPluginManager { get; private set; }
		public File BukkitPluginDirectory { get { return new File("BukkitPlugins"); } }

		private void InitializeBukkit()
		{
			if (!BukkitPluginDirectory.exists())
				BukkitPluginDirectory.mkdir();
			BukkitPluginManager = new SimplePluginManager(this);
			BukkitPluginManager.loadPlugins(BukkitPluginDirectory);
		}

		public int broadcastMessage(string str)
		{
			return Broadcast(str);
		}

		public void configureDbConfig(ServerConfig sc)
		{
			// TODO: Implement and figure out what the heck it is
			throw new NotImplementedException();
		}

		public org.bukkit.World createWorld(string str, org.bukkit.World.Environment we, long l)
		{
			// HACK: Utilize the seed
			// TODO: Allow Nether
			return CreateWorld(str);
		}

		public org.bukkit.World createWorld(string str, org.bukkit.World.Environment we)
		{
			// TODO: Allow Nether
			return CreateWorld(str);
		}

		public bool dispatchCommand(CommandSender cs, string str)
		{
			return Commands.dispatch(cs, str);
		}

		public string getIp()
		{
			return IpAddress;
		}

		public java.util.logging.Logger getLogger()
		{
			return java.util.logging.Logger.getLogger("Minecraft");
		}

		public int getMaxPlayers()
		{
			return MaxPlayers;
		}

		public string getName()
		{
			return Name;
		}

		public Player[] getOnlinePlayers()
		{
			return GetClients();
		}

		public Player getPlayer(string str)
		{
			return GetClient(str);
		}

		public PluginCommand getPluginCommand(string str)
		{
			Command cmd = Commands.getCommand(str);
			return cmd is PluginCommand ? (PluginCommand)cmd : null;
		}

		public PluginManager getPluginManager()
		{
			return BukkitPluginManager;
		}

		public int getPort()
		{
			return Port;
		}

		public BukkitScheduler getScheduler()
		{
			// TODO: Figure out what this is and whether or not we need it
			throw new NotImplementedException();
		}

		public string getServerId()
		{
			return Name;
		}

		public string getServerName()
		{
			return Name;
		}

		public string getVersion()
		{
			return "C#raft";
		}

		public org.bukkit.World getWorld(string str)
		{
			return GetWorld(str);
		}

		public java.util.List getWorlds()
		{
			WorldManager[] worlds = GetWorlds();
			ArrayList list = new ArrayList(worlds.Length);
			for (int i = 0; i < worlds.Length; i++)
				list.add(worlds[i]);
			return list;
		}

		public java.util.List matchPlayer(string str)
		{
			List<Client> clients = new List<Client>(GetClients(str));
			ArrayList list = new ArrayList(clients.Count);
			foreach (Client c in clients)
				list.add(c);
			return list;
		}

		public void reload()
		{
			Reload();
		}

		public void savePlayers()
		{
			SaveClients();
		}
	}
}
