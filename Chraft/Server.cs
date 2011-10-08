using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
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
using Chraft.Irc;
using Chraft.Commands;
using Chraft.Plugins.Events.Args;
using Chraft.Plugins.Events;

namespace Chraft
{
    public class Server
    {

        private volatile int NextEntityId = 0;
        private bool Running = true;
        private Socket _Listener;
        private SocketAsyncEventArgs _AcceptEventArgs;

        public static ConcurrentQueue<Client> RecvClientQueue = new ConcurrentQueue<Client>();
        public static ConcurrentQueue<Client> SendClientQueue = new ConcurrentQueue<Client>();

        public static ConcurrentQueue<Client> ClientsToDispose = new ConcurrentQueue<Client>();

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
        /// Gets the ClientCommandHandler for the server.
        /// </summary>
        public ClientCommandHandler ClientCommandHandler { get; private set; }

        /// <summary>
        /// Gets the ServerCommandHandler for the server.
        /// </summary>
        public ServerCommandHandler ServerCommandHandler { get; private set; }

        /// <summary>
        /// Gets a thread-safe list of clients.  Use GetClients for a thread-safe version.
        /// </summary>
        public ConcurrentDictionary<int, Client> Clients { get; private set; }

        /// <summary>
        /// Gets a thread-unsafe list of worlds.  Use GetWorlds for a thread-safe version.
        /// </summary>
        public List<WorldManager> Worlds { get; private set; }

        /// <summary>
        /// Gets a random number generator for the server.
        /// </summary>
        public Random Rand { get; private set; }

        /// <summary>
        /// Thread safe list of all entities on the server.
        /// </summary>
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
        public static Recipe[] Recipes { get; private set; }

        /// <summary>
        /// Gets a list of user-defined smelting recipes known to the server.
        /// </summary>
        public static SmeltingRecipe[] SmeltingRecipes { get; private set; }

        /// <summary>
        /// Gets the IRC client, if it has been initialized.
        /// </summary>
        public IrcClient Irc { get; private set; }

        /// <summary>
        /// Gets the server hash. Used for authentication, guaranteed to be unique between instances
        /// </summary>
        public string ServerHash { get; private set; }

        /// <summary>
        /// Determines whether to use the official minecraft authentication or none
        /// </summary>
        public bool UseOfficalAuthentication { get; private set; }

        public Server()
        {
            ServerHash = Hash.MD5(Guid.NewGuid().ToByteArray());
            UseOfficalAuthentication = Settings.Default.UseOfficalAuthentication;
            Clients = new ConcurrentDictionary<int, Client>();
            Rand = new Random();
            Logger = new Logger(this, Settings.Default.LogFile);
            PluginManager = new PluginManager(this, Settings.Default.PluginFolder);
            Items = new ItemDb(Settings.Default.ItemsFile);
            Recipes = Recipe.FromFile(Settings.Default.RecipesFile);
            SmeltingRecipes = SmeltingRecipe.FromFile(Settings.Default.SmeltingRecipesFile);
            ClientCommandHandler = new ClientCommandHandler();
            ServerCommandHandler = new ServerCommandHandler();
            if (Settings.Default.IrcEnabled)
                InitializeIrc();

            _AcceptEventArgs = new SocketAsyncEventArgs();
            _AcceptEventArgs.Completed += Accept_Completion;

            _Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        }

        public static Recipe[] GetRecipes()
        {
            lock (Recipes)
                return Recipes;
        }

        public static SmeltingRecipe[] GetSmeltingRecipes()
        {
            lock (SmeltingRecipes)
                return SmeltingRecipes;
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

        public void Run()
        {
            Logger.Log(Logger.LogLevel.Info, "Starting C#raft...");


            Worlds = new List<WorldManager>();
            Worlds.Add(new WorldManager(this));

            PluginManager.LoadDefaultAssemblies();

            // Player list update thread
            Thread t = new Thread(PlayerListProc);
            t.IsBackground = true;
            t.Start();

            for (int i = 0; i < 10; ++i)
            {
                Client.SendSocketEventPool.Push(new SocketAsyncEventArgs());
                Client.RecvSocketEventPool.Push(new SocketAsyncEventArgs());
            }

            while (Running)
                RunProc();
        }

        private void PlayerListProc()
        {
            while (Running)
            {
                foreach (var client in this.GetAuthenticatedClients())
                {
                    this.BroadcastToAuthenticated(new PlayerListItemPacket() { PlayerName = client.Owner.Username, Online = client.Owner.Ready, Ping = (short)client.Ping });
                    Thread.Sleep(50);
                }
                Thread.Sleep(1000);
            }
        }

        /// <summary>
        /// Creates a new world.
        /// </summary>
        /// <param name="name">The name of the folder to contain and identify the world.</param>
        /// <returns>The newly created world.</returns>
        public WorldManager CreateWorld(string name)
        {

            WorldManager world = new WorldManager(this);

            //Event
            WorldCreatedEventArgs e = new WorldCreatedEventArgs(world);
            PluginManager.CallEvent(Event.WORLD_CREATE, e);
            if (e.EventCanceled) return null;
            //End Event

            lock (Worlds)
                Worlds.Add(world);

            return world;
        }

        private void RunProc()
        {
            Logger.Log(Logger.LogLevel.Info, "Using IP Addresss {0}.", Settings.Default.IPAddress);
            Logger.Log(Logger.LogLevel.Info, "Listening on port {0}.", Settings.Default.Port);

            IPAddress address = IPAddress.Parse(Settings.Default.IPAddress);
            IPEndPoint ipEndPoint = new IPEndPoint(address, Settings.Default.Port);

            _Listener.Bind(ipEndPoint);
            _Listener.Listen(5);

            RunNetwork();

            if (Running)
            {
                Logger.Log(Logger.LogLevel.Info, "Waiting one second before restarting network.");
                Thread.Sleep(1000);
            }
        }

        public static void ProcessSendQueue()
        {
            int count = SendClientQueue.Count;

            Parallel.For(0, count, i =>
            {
                Client client;
                if (!SendClientQueue.TryDequeue(out client))
                    return;

                if(!client.Running)
                {
                    client.DisposeSendSystem();
                    return;
                }

                client.Send_Start();
            });
        }

        public static void ProcessReadQueue()
        {
            int count = RecvClientQueue.Count;

            Parallel.For(0, count, i =>
            {
                Client client;
                if (!RecvClientQueue.TryDequeue(out client))
                    return;

                if(!client.Running)
                    return;
                
                Interlocked.Exchange(ref client.TimesEnqueuedForRecv, 0);
                ByteQueue bufferToProcess = client.GetBufferToProcess();

                int length = client.FragPackets.Size + bufferToProcess.Size;
                while(length > 0)
                {
                    byte packetType = 0;

                    if (client.FragPackets.Size > 0)
                        packetType = client.FragPackets.GetPacketID();
                    else
                        packetType = bufferToProcess.GetPacketID();

                    //client.Logger.Log(Chraft.Logger.LogLevel.Info, "Reading packet {0}", ((PacketType)packetType).ToString());

                    PacketHandler handler = PacketHandlers.GetHandler((PacketType)packetType);

                    if(handler == null)
                    {
                        byte[] unhandledPacketData = GetBufferToBeRead(bufferToProcess, client, length);
                        
                        // TODO: handle this case, writing on the console a warning and/or writing it plus the bytes on a log
                        client.Logger.Log(Chraft.Logger.LogLevel.Caution, "Unhandled packet arrived, id: {0}", unhandledPacketData[0]);

                        client.Logger.Log(Chraft.Logger.LogLevel.Warning, "Data:\r\n {0}", BitConverter.ToString(unhandledPacketData, 1));
                        length = 0;
                    }
                    else if(handler.Length == 0)
                    {
                        byte[] data = GetBufferToBeRead(bufferToProcess, client, length);

                        if (length >= handler.MinimumLength)
                        {
                            PacketReader reader = new PacketReader(data, length, StreamRole.Server);

                            handler.OnReceive(client, reader);

                            // If we failed it's because the packet isn't complete
                            if (reader.Failed)
                            {
                                EnqueueFragment(client, data);
                                length = 0;
                            }
                            else
                            {
                                bufferToProcess.Enqueue(data, reader.Index, data.Length - reader.Index);
                                length = bufferToProcess.Length;
                            }
                        }
                        else
                            EnqueueFragment(client, data);
                        
                    }
                    else if (length >= handler.Length)
                    {
                        byte[] data = GetBufferToBeRead(bufferToProcess, client, handler.Length);

                        PacketReader reader = new PacketReader(data, handler.Length, StreamRole.Server);

                        handler.OnReceive(client, reader);

                        // If we failed it's because the packet isn't complete
                        if (reader.Failed)
                        {
                            EnqueueFragment(client, data);
                            length = 0;
                        }
                        else
                            length = bufferToProcess.Length;
                    }
                    else
                    {
                        byte[] data = GetBufferToBeRead(bufferToProcess, client, length);
                        EnqueueFragment(client, data);
                        length = 0;
                    }
                }
            });
        }

        private static void EnqueueFragment(Client client, byte[] data)
        {
            int fragPacketWaiting = client.FragPackets.Length;
            // We are waiting for more data than an uncompressed chunk, it's not possible
            if (fragPacketWaiting > 81920)
                client.Kick("Too much pending data to be read");
            else
                client.FragPackets.Enqueue(data, 0, data.Length);
        }

        private static byte[] GetBufferToBeRead(ByteQueue processedBuffer, Client client, int length)
        {
            int availableData = client.FragPackets.Size + processedBuffer.Size;

            if (length > availableData)
                return null;

            int fromFrag;

            byte[] data = new byte[length];

            if (length >= client.FragPackets.Size)
                fromFrag = client.FragPackets.Size;
            else
                fromFrag = length;

            client.FragPackets.Dequeue(data, 0, fromFrag);

            int fromProcessed = length - fromFrag; 
            
            processedBuffer.Dequeue(data, 0, fromProcessed);

            return data;
        }

        private void OnJoined(Client c)
        {
            if (Joined != null)
                Joined.Invoke(this, new ClientEventArgs(c));
        }

        private bool OnBeforeAccept(Socket socket)
        {
            if (BeforeAccept != null)
            {
                TcpEventArgs e = new TcpEventArgs(socket);
                BeforeAccept.Invoke(this, e);
                return !e.Cancelled;
            }
            return true;
        }

        public AutoResetEvent NetworkSignal = new AutoResetEvent(true);
        private int _AsyncAccepts = 0;
        private Task _ReadClientsPackets;
        private Task _SendClientPackets;
        private Task _DisposeClients;

        private void RunNetwork()
        {
            while (NetworkSignal.WaitOne())
            {
                int accepts = Interlocked.CompareExchange(ref _AsyncAccepts, 1, 0);

                if (accepts == 0)
                {
                    //Logger.Log(Chraft.Logger.LogLevel.Info, "Starting async accept");
                    _AcceptEventArgs.AcceptSocket = null;
                    _Listener.AcceptAsync(_AcceptEventArgs);
                }

                if (RecvClientQueue.Count > 0 && (_ReadClientsPackets == null || _ReadClientsPackets.IsCompleted))
                {
                    //Logger.Log(Chraft.Logger.LogLevel.Info, "Starting ProcessReadQueue");
                    _ReadClientsPackets = new Task(ProcessReadQueue);
                    _ReadClientsPackets.Start();
                }

                if(ClientsToDispose.Count > 0 && (_DisposeClients == null || _DisposeClients.IsCompleted))
                {
                    _DisposeClients = new Task(DisposeClients);
                    _DisposeClients.Start();
                }

                if(SendClientQueue.Count > 0 && (_SendClientPackets == null || _SendClientPackets.IsCompleted))
                {
                    _SendClientPackets = new Task(ProcessSendQueue);
                    _SendClientPackets.Start();
                }
            }
        }

        private void DisposeClients()
        {
            int count = ClientsToDispose.Count;
            while (ClientsToDispose.Count > 0)
            {
                Client client;
                if (!ClientsToDispose.TryDequeue(out client))
                    continue;

                client.Dispose();
            }
        }

        private void Accept_Process(SocketAsyncEventArgs e)
        {
            if (OnBeforeAccept(e.AcceptSocket))
            {
                Client c = new Client(e.AcceptSocket, new Player(this, AllocateEntity()));
                //Event
                ClientAcceptedEventArgs args = new ClientAcceptedEventArgs(this, c);
                PluginManager.CallEvent(Event.SERVER_ACCEPT, args);
                //Do not check for EventCanceled because that could make this unstable.
                //End Event

                lock (Clients)
                {
                    AddClient(c);
                    Logger.Log(Chraft.Logger.LogLevel.Info, "Clients online: {0}", Clients.Count);
                }
                AddEntity(c.Owner);
                c.Start();
                Logger.Log(Chraft.Logger.LogLevel.Info, "Starting client");
                OnJoined(c);
            }
            else
            {
                e.AcceptSocket.Close();
            }

            Interlocked.Exchange(ref _AsyncAccepts, 0);
            NetworkSignal.Set();
        }

        private void Accept_Completion(object sender, SocketAsyncEventArgs e)
        {
            Accept_Process(e); 
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
            //Event
            ServerBroadcastEventArgs e = new ServerBroadcastEventArgs(this, message, excludeClient);
            PluginManager.CallEvent(Event.SERVER_BROADCAST, e);
            if (e.EventCanceled) return;
            message = e.Message;
            excludeClient = e.ExcludeClient;
            //End Event

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
                if (excludeClient == null || c.Owner.EntityId != excludeClient.Owner.EntityId)
                    c.SendPacket(packet);
            }
        }

        /// <summary>
        /// Broadcasts a packet to all authenticated clients
        /// </summary>
        /// <param name="packet"></param>
        public void BroadcastToAuthenticated(Packet packet)
        {
            System.Threading.Tasks.Parallel.ForEach(this.GetAuthenticatedClients(), (client) => client.SendPacket(packet));
        }

        private Client[] _ClientsCache;
        private int _ClientListChanges;

        /// <summary>
        /// Gets a thread-safe array of connected clients.
        /// </summary>
        /// <returns>A thread-safe array of connected clients.</returns>
        public Client[] GetClients()
        {
            int changes = Interlocked.Exchange(ref _ClientListChanges, 0);
            if (_ClientsCache == null || changes > 0)
             _ClientsCache =  Clients.Values.ToArray();

            return _ClientsCache;
        }

        // TODO: cache this thing?
        public IEnumerable<Client> GetAuthenticatedClients()
        {
            return GetClients().Where((client) => !String.IsNullOrEmpty(client.Owner.Username) && client.Owner.Ready);
        }


        public IEnumerable<Client> GetClients(string name)
        {
            return from c in GetAuthenticatedClients()
                   where c.Owner.Username.ToLower().Contains(name.ToLower()) || c.Owner.DisplayName.ToLower().Contains(name.ToLower())
                   select c;
        }

        /// <summary>
        /// Gets a thread-safe array of all entities, including clients.
        /// </summary>
        /// <returns>A thread-safe array of all active entities.</returns>
        public EntityBase[] GetEntities()
        {
            lock (_Entities)
            {
                return _Entities.ToArray();
            }
        }

        /// <summary>
        /// Thread-friendly way of removing server entities
        /// </summary>
        public void RemoveEntity(EntityBase e)
        {
            lock (_Entities)
            {
                _Entities.Remove(e);
            }
        }

        /// <summary>
        /// Thread-friendly way of adding server entities
        /// </summary>
        public void AddEntity(EntityBase e)
        {
            lock (_Entities)
            {
                _Entities.Add(e);
            }
        }

        public void AddClient(Client client)
        {
            Clients.TryAdd(client.Owner.SessionID, client);
            Interlocked.Increment(ref _ClientListChanges);
        }

        public void RemoveClient(Client client)
        {
            Clients.TryRemove(client.Owner.SessionID, out client);
            Interlocked.Increment(ref _ClientListChanges);
        }

        /// <summary>
        /// Sends a packet in parallel to each nearby player.
        /// </summary>
        /// <param name="world">The world containing the coordinates.</param>
        /// <param name="absCoords>The center coordinates.</param>
        /// <param name="packet">The packet to send</param>
        public void SendPacketToNearbyPlayers(WorldManager world, AbsWorldCoords absCoords, Packet packet)
        {
            Parallel.ForEach(this.GetNearbyPlayers(world, absCoords), (client) =>
            {
                client.SendPacket(packet);
            });
        }

        /// <summary>
        /// Yields an enumerable of nearby players, thread-safe.
        /// </summary>
        /// <param name="world">The world containing the coordinates.</param>
        /// <param name="absCoords">The center coordinates.</param>
        /// <returns>A lazy enumerable of nearby players.</returns>
        public IEnumerable<Client> GetNearbyPlayers(WorldManager world, AbsWorldCoords absCoords)
        {
            int radius = Settings.Default.SightRadius << 4;
            foreach (Client c in GetAuthenticatedClients())
            {
                if (c.Owner.World == world && Math.Abs(absCoords.X - c.Owner.Position.X) <= radius && Math.Abs(absCoords.Y - c.Owner.Position.Y) <= radius && Math.Abs(absCoords.Z - c.Owner.Position.Z) <= radius)
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
        public IEnumerable<EntityBase> GetNearbyEntities(WorldManager world, AbsWorldCoords coords)
        {
            int radius = Settings.Default.SightRadius << 4;
            foreach (EntityBase e in GetEntities())
            {
                if (e.World == world && Math.Abs(coords.X - e.Position.X) <= radius && Math.Abs(coords.Y - e.Position.Y) <= radius && Math.Abs(coords.Z - e.Position.Z) <= radius)
                    yield return e;
            }
        }
        
        /// <summary>
        /// Yields an enumerable of entities where their BoundingBox intersects with <paramref name="boundingBox"/>
        /// </summary>
        /// <returns>
        /// The entities within bounding box.
        /// </returns>
        /// <param name='boundingBox'>
        /// Bounding box.
        /// </param>
        public IEnumerable<EntityBase> GetEntitiesWithinBoundingBox(BoundingBox boundingBox)
        {
            return from e in GetEntities()
                   where e.BoundingBox.IntersectsWith(boundingBox)
                   select e;
        }

        /// <summary>
        /// Drops an item based on the given player's position and rotation.
        /// </summary>
        /// <param name="client">The client to be used for position calculations.</param>
        /// <param name="stack">The stack to be dropped.</param>
        /// <returns>The entity ID of the item drop.</returns>
        public int DropItem(Client client, ItemStack stack)
        {
            return DropItem(client.Owner.World, UniversalCoords.FromWorld(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z), stack);
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
        public int DropItem(WorldManager world, UniversalCoords coords, ItemStack stack)
        {
            int entityId = AllocateEntity();
            AddEntity(new ItemEntity(this, entityId)
            {
                World = world,
                Position = new Location(new Vector3(coords.WorldX + 0.5, coords.WorldY, coords.WorldZ + 0.5)), // Put in the middle of the block (ignoring Y)
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
           
            if (Irc != null)
                Irc.Stop();
        }

        internal void OnCommand(Client client, ClientCommand cmd, string[] tokens)
        {
            if (Command != null)
                Command.Invoke(client, new CommandEventArgs(client, cmd, tokens));
        }
    }
}
