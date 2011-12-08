#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using Chraft.Interfaces;
using Chraft.Interfaces.Recipes;
using Chraft.Net.Packets;
using Chraft.Utils.Config;
using System.Net;
using System.Threading;
using Chraft.Utils;
using Chraft.Utils.Config;
using Chraft.World;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Plugins;
using Chraft.Irc;
using Chraft.Commands;
using Chraft.Plugins.Events.Args;
using Chraft.Plugins.Events;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Chraft
{
    public class Server
    {
        
        private int NextEntityId;
        private int NextSessionId;
        private bool Running = true;
        private Socket _Listener;
        private SocketAsyncEventArgs _AcceptEventArgs;

        public static ConcurrentQueue<Client> RecvClientQueue = new ConcurrentQueue<Client>();
        public static ConcurrentQueue<Client> SendClientQueue = new ConcurrentQueue<Client>();

        public static ConcurrentQueue<Client> ClientsToDispose = new ConcurrentQueue<Client>();

        private Timer _globalTick;
        private Task _playerSaveTask;
        private CancellationTokenSource _playerSaveToken;
        public bool NeedsFullSave;
        public bool FullSaving;
        public ConcurrentQueue<Client> PlayersToSave;
        public ConcurrentQueue<Client> PlayersToSavePostponed; 

#if PROFILE
        public static PerformanceCounter CpuPerfCounter = new PerformanceCounter("Process", "% Processor Time", Process.GetCurrentProcess().ProcessName);
        public static DateTime ProfileStartTime = DateTime.MinValue;
#endif

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
        /// Gets a thread-safe dictionary of clients.  Use GetClients for an array version.
        /// </summary>
        public ConcurrentDictionary<int, Client> Clients { get; private set; }

        /// <summary>
        /// Gets a thread-safe dictionary of authenticated clients.  Use GetAuthenticatedClients for an array version.
        /// </summary>
        public ConcurrentDictionary<int, Client> AuthClients { get; private set; }

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
        private readonly ConcurrentDictionary<int, EntityBase> _Entities = new ConcurrentDictionary<int, EntityBase>();

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

        public int ClientsConnectionSlots;

        
        public Server()
        {
            ChraftConfig.Load();
            ClientsConnectionSlots = 30;
            Packet.Role = StreamRole.Server;
            Rand = new Random();
            ServerHash = GetRandomServerHash();
            UseOfficalAuthentication = ChraftConfig.UseOfficalAuthentication;
            Clients = new ConcurrentDictionary<int, Client>();
            AuthClients = new ConcurrentDictionary<int, Client>();
            Logger = new Logger(this, ChraftConfig.LogFile);
            PluginManager = new PluginManager(this, ChraftConfig.PluginFolder);
            Items = new ItemDb(ChraftConfig.ItemsFile);
            Recipes = Recipe.FromFile(ChraftConfig.RecipesFile);
            SmeltingRecipes = SmeltingRecipe.FromFile(ChraftConfig.SmeltingRecipesFile);
            ClientCommandHandler = new ClientCommandHandler();
            ServerCommandHandler = new ServerCommandHandler();
            if (ChraftConfig.IrcEnabled)
                InitializeIrc();

            PacketMap.Initialize();

            _AcceptEventArgs = new SocketAsyncEventArgs();
            _AcceptEventArgs.Completed += Accept_Completion;

            _Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            for(int i = 0; i < 10; ++i)
            {
                MapChunkPacket.DeflaterPool.Push(new Deflater(5));
            }

            PlayersToSave = new ConcurrentQueue<Client>();
            PlayersToSavePostponed = new ConcurrentQueue<Client>();
        }

        public string GetRandomServerHash()
        {
            byte[] bytes = new byte[7];
            Rand.NextBytes(bytes);

            return "23" + BitConverter.ToString(bytes).Replace("-", String.Empty);
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
            IPEndPoint ep = new IPEndPoint(Dns.GetHostEntry(ChraftConfig.IrcServer).AddressList[0], ChraftConfig.IrcPort);
            Irc = new IrcClient(ep, ChraftConfig.IrcNickname);
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
                if (!ChraftConfig.AllowedChatChars.Contains(e.Args[1][i]))
                    return;

            Broadcast("§7[IRC] " + e.Prefix.Nickname + ":§f " + e.Args[1], sendToIrc: false);
            e.Handled = true;
        }

        private void OnIrcNotice(object sender, IrcEventArgs e)
        {
            for (int i = 0; i < e.Args[1].Length; i++)
                if (!ChraftConfig.AllowedChatChars.Contains(e.Args[1][i]))
                    return;

            Broadcast("§c[IRC] " + e.Prefix.Nickname + ":§f " + e.Args[1], sendToIrc: false);
            e.Handled = true;
        }

        private void OnIrcWelcome(object sender, IrcEventArgs e)
        {
            Irc.Join(ChraftConfig.IrcChannel);
        }

        public void Run()
        {
            Logger.Log(Logger.LogLevel.Info, "Starting C#raft...");


            Worlds = new List<WorldManager> { new WorldManager(this) };

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

            _globalTick = new Timer(GlobalTickProc, null, 50, 50);

            while (Running)
                RunProc();
        }

        private int _globalTicks;

        private void GlobalTickProc(object state)
        {
            ++_globalTicks;

            foreach (WorldManager worldManager in Worlds)
            {
                worldManager.WorldTick();
                worldManager.StartSaveProc(20 / Worlds.Count);
            }

            if ((_globalTicks % 10) == 0)
                Task.Factory.StartNew(DoPulse);            
            

            if (NeedsFullSave)
            {
                FullSaving = true;
                Task.Factory.StartNew(FullSave);
            }
            else if ((_globalTicks % 200) == 0 && (_playerSaveTask == null || _playerSaveTask.IsCompleted))
            {
                _playerSaveToken = new CancellationTokenSource();
                var token = _playerSaveToken.Token;
                _playerSaveTask = Task.Factory.StartNew(()=> SavePlayers(50, token), token);
            }
        }
        
        private void SavePlayers(int playersToSave, CancellationToken token)
        {
            if (token.IsCancellationRequested)
                return;

            int count = PlayersToSave.Count;

            if (count > playersToSave)
                count = playersToSave;

            playersToSave -= count;
            for (int i = 0; i < count && Running && !token.IsCancellationRequested; ++i)
            {
                Client client;
                PlayersToSave.TryDequeue(out client);

                if (client == null)
                    continue;

                /* Better to "signal" that the chunk can be queued again before saving, 
                 * we don't know which signaled changes will be saved during the save */
                Interlocked.Exchange(ref client.Owner.ChangesToSave, 0);

                client.Save();
            }

            count = PlayersToSavePostponed.Count;

            if (count > playersToSave)
                count = playersToSave;

            for (int i = 0; i < count && Running && !token.IsCancellationRequested; ++i)
            {
                Client client;
                PlayersToSavePostponed.TryDequeue(out client);

                if (client == null)
                    continue;


                if (client.Owner.ChangesToSave > 0 && client.Owner.EnqueuedForSaving > client.Owner.LastSaveTime)
                {
                    if ((DateTime.Now - client.Owner.LastSaveTime) > Chunk.SaveSpan)
                    {
                        PlayersToSavePostponed.Enqueue(client);
                        continue;
                    }
                    /* Better to "signal" that the chunk can be queued again before saving, 
                     * we don't know which signaled changes will be saved during the save */
                    Interlocked.Exchange(ref client.Owner.ChangesToSave, 0);

                    client.Save();
                }
            }
            
        }

        private void FullSave()
        {
            foreach(WorldManager world in Worlds)
            {
                if (world.IsSaving())
                    world.StopSave();


                Chunk[] chunks = world.GetChunks();

                world.ChunksToSave = new ConcurrentQueue<Chunk>();
                world.ChunksToSavePostponed = new ConcurrentQueue<Chunk>();
                foreach (Chunk chunk in chunks)
                {
                    chunk.ChangesToSave = 0;
                    chunk.Save();
                }
            }

            if (!_playerSaveTask.IsCompleted)
                _playerSaveTask.Wait();

            Client[] clients = GetClients();

            foreach(Client client in clients)
            {
                client.Owner.ChangesToSave = 0;
                client.Save();
            }

            NeedsFullSave = false;
            FullSaving = false;
        }

        private void PlayerListProc()
        {
            while (Running)
            {
                foreach (var client in GetAuthenticatedClients())
                {
                    this.BroadcastToAuthenticated(new PlayerListItemPacket() { PlayerName = client.Username, Online = client.Owner.Ready, Ping = (short)client.Ping });
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
            PluginManager.CallEvent(Event.WorldCreate, e);
            if (e.EventCanceled) return null;
            //End Event

            lock (Worlds)
                Worlds.Add(world);

            return world;
        }

        private void RunProc()
        {
            Logger.Log(Logger.LogLevel.Info, "Using IP Address {0}.", ChraftConfig.IPAddress);
            Logger.Log(Logger.LogLevel.Info, "Listening on port {0}.", ChraftConfig.Port);

            IPAddress address = IPAddress.Parse(ChraftConfig.IPAddress);
            IPEndPoint ipEndPoint = new IPEndPoint(address, ChraftConfig.Port);

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

                if (!client.Running)
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

                if (!client.Running)
                    return;

                Interlocked.Exchange(ref client.TimesEnqueuedForRecv, 0);
                ByteQueue bufferToProcess = client.GetBufferToProcess();

                int length = client.FragPackets.Size + bufferToProcess.Size;
                while (length > 0)
                {
                    byte packetType = 0;

                    if (client.FragPackets.Size > 0)
                        packetType = client.FragPackets.GetPacketID();
                    else
                        packetType = bufferToProcess.GetPacketID();

                    //client.Logger.Log(Chraft.Logger.LogLevel.Info, "Reading packet {0}", ((PacketType)packetType).ToString());

                    PacketHandler handler = PacketHandlers.GetHandler((PacketType)packetType);

                    if (handler == null)
                    {
                        byte[] unhandledPacketData = GetBufferToBeRead(bufferToProcess, client, length);

                        // TODO: handle this case, writing on the console a warning and/or writing it plus the bytes on a log
                        client.Logger.Log(Chraft.Logger.LogLevel.Caution, "Unhandled packet arrived, id: {0}", unhandledPacketData[0]);

                        client.Logger.Log(Chraft.Logger.LogLevel.Warning, "Data:\r\n {0}", BitConverter.ToString(unhandledPacketData, 1));
                        length = 0;
                    }
                    else if (handler.Length == 0)
                    {
                        byte[] data = GetBufferToBeRead(bufferToProcess, client, length);

                        if (length >= handler.MinimumLength)
                        {
                            PacketReader reader = new PacketReader(data, length);

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
                        {
                            EnqueueFragment(client, data);
                            length = 0;
                        }

                    }
                    else if (length >= handler.Length)
                    {
                        byte[] data = GetBufferToBeRead(bufferToProcess, client, handler.Length);

                        PacketReader reader = new PacketReader(data, handler.Length);

                        handler.OnReceive(client, reader);

                        // If we failed it's because the packet is wrong
                        if (reader.Failed)
                        {
                            client.MarkToDispose();
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

            processedBuffer.Dequeue(data, fromFrag, fromProcessed);

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
        private int _asyncAccepts = 0;
        private Task _readClientsPackets;
        private Task _sendClientPackets;
        private Task _disposeClients;

        private void RunNetwork()
        {
            while (NetworkSignal.WaitOne())
            {
                if (TryTakeConnectionSlot())
                    _Listener.AcceptAsync(_AcceptEventArgs);               

                if (!RecvClientQueue.IsEmpty && (_readClientsPackets == null || _readClientsPackets.IsCompleted))
                {
                    _readClientsPackets = Task.Factory.StartNew(ProcessReadQueue);
                }

                if (!ClientsToDispose.IsEmpty && (_disposeClients == null || _disposeClients.IsCompleted))
                {
                    _disposeClients = Task.Factory.StartNew(DisposeClients);
                }

                if (!SendClientQueue.IsEmpty && (_sendClientPackets == null || _sendClientPackets.IsCompleted))
                {
                    _sendClientPackets = Task.Factory.StartNew(ProcessSendQueue);
                }
            }
        }

        private void DisposeClients()
        {
            int count = ClientsToDispose.Count;
            while (!ClientsToDispose.IsEmpty)
            {
                Client client;
                if (!ClientsToDispose.TryDequeue(out client))
                    continue;

                client.Dispose();
            }
        }

        public void FreeConnectionSlot()
        {
            //Logger.Log(Logger.LogLevel.Info, "FreeingSlot ");
            Interlocked.Increment(ref ClientsConnectionSlots);
            NetworkSignal.Set();
        }

        public bool TryTakeConnectionSlot()
        {            
            int accepts = Interlocked.Exchange(ref _asyncAccepts, 1);           
            if (accepts == 0)
            {
                int count = Interlocked.Decrement(ref ClientsConnectionSlots);

                if (count >= 0)
                    return true;

                _asyncAccepts = 0;

                Interlocked.Increment(ref ClientsConnectionSlots);
            }          
           
            return false;
            
        }

        private void Accept_Process(SocketAsyncEventArgs e)
        {
            if (OnBeforeAccept(e.AcceptSocket))
            {
                Interlocked.Increment(ref NextSessionId);
                Client c = new Client(NextSessionId, this, e.AcceptSocket);
                //Event
                ClientAcceptedEventArgs args = new ClientAcceptedEventArgs(this, c);
                PluginManager.CallEvent(Event.ServerAccept, args);
                //Do not check for EventCanceled because that could make this unstable.
                //End Event

                c.Start();
                
                AddClient(c);
                Logger.Log(Chraft.Logger.LogLevel.Info, "Clients online: {0}", Clients.Count);
                                
                Logger.Log(Chraft.Logger.LogLevel.Info, "Starting client");
                OnJoined(c);
            }
            else
            {
                if (e.AcceptSocket.Connected)
                    e.AcceptSocket.Shutdown(SocketShutdown.Both);
                e.AcceptSocket.Close();
            }
            _AcceptEventArgs.AcceptSocket = null;
            Interlocked.Exchange(ref _asyncAccepts, 0);
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
            return Interlocked.Increment(ref NextSessionId);
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
            PluginManager.CallEvent(Event.ServerBroadcast, e);
            if (e.EventCanceled) return;
            message = e.Message;
            excludeClient = e.ExcludeClient;
            //End Event

            foreach (Client c in GetAuthenticatedClients())
            {
                if (c != excludeClient)
                    c.SendMessage(message);
            }

            if (sendToIrc && Irc != null)
            {
                Irc.WriteLine("PRIVMSG {0} :{1}", ChraftConfig.IrcChannel, message.Replace('§', '\x3'));
            }
        }

        /// <summary>
        /// Broadcasts a message to all clients, optionally excluding the sender.
        /// </summary>
        /// <param name="message">The message to be broadcasted.</param>
        /// <param name="excludeClient">The client to excluded; usually the sender.</param>
        public void BroadcastSync(string message, Client excludeClient = null, bool sendToIrc = true)
        {
            //Event
            ServerBroadcastEventArgs e = new ServerBroadcastEventArgs(this, message, excludeClient);
            PluginManager.CallEvent(Event.ServerBroadcast, e);
            if (e.EventCanceled) return;
            message = e.Message;
            excludeClient = e.ExcludeClient;
            //End Event

            foreach (Client c in GetAuthenticatedClients())
            {
                if (c != excludeClient)
                {
                    ChatMessagePacket cm = new ChatMessagePacket { Message = message };
                    c.Send_Sync_Packet(cm);
                }
            }

            if (sendToIrc && Irc != null)
            {
                Irc.WriteLine("PRIVMSG {0} :{1}", ChraftConfig.IrcChannel, message.Replace('§', '\x3'));
            }
        }

        /// <summary>
        /// Broadcasts a packet to all clients, optionally excluding the sender.
        /// </summary>
        /// <param name="packet">The packet to be broadcasted.</param>
        /// <param name="excludeClient">The client to excluded; usually the sender.</param>
        public void Broadcast(Packet packet, Client excludeClient = null)
        {
            Client[] clients = GetClients();

            if (clients.Length == 0)
                return;

            packet.SetShared(Logger, clients.Length);
            foreach (Client c in clients)
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
            Client[] authClients = GetAuthenticatedClients();

            if (authClients.Length == 0)
                return;

            packet.SetShared(Logger, authClients.Length);
            System.Threading.Tasks.Parallel.ForEach(authClients, (client) => client.SendPacket(packet));
        }

        private Client[] _clientsCache;
        private int _clientDictChanges;

        /// <summary>
        /// Gets a thread-safe array of connected clients.
        /// </summary>
        /// <returns>A thread-safe array of connected clients.</returns>
        public Client[] GetClients()
        {
            int changes = Interlocked.Exchange(ref _clientDictChanges, 0);
            if (_clientsCache == null || changes > 0)
                _clientsCache = Clients.Values.ToArray();

            return _clientsCache;
        }

        private Client[] _authClientsCache;
        private int _authClientDictChanges;

        public Client[] GetAuthenticatedClients()
        {
            int changes = Interlocked.Exchange(ref _authClientDictChanges, 0);
            if (_authClientsCache == null || changes > 0)
                _authClientsCache = AuthClients.Values.ToArray();

            return _authClientsCache;
        }


        public IEnumerable<Client> GetClients(string name)
        {
            return from c in GetAuthenticatedClients()
                   where c.Username.ToLower().Contains(name.ToLower()) || c.Owner.DisplayName.ToLower().Contains(name.ToLower())
                   select c;
        }

        private EntityBase[] _entityCache;
        private int _entityDictChanges;

        /// <summary>
        /// Gets a thread-safe array of all entities, including clients.
        /// </summary>
        /// <returns>A thread-safe array of all active entities.</returns>
        public EntityBase[] GetEntities()
        {
            int changes = Interlocked.Exchange(ref _entityDictChanges, 0);

            if (_entityCache == null || changes > 0)
                _entityCache = _Entities.Values.ToArray();

            return _entityCache;
        }

        /// <summary>
        /// Thread-friendly way of removing server entities
        /// </summary>
        public void RemoveEntity(EntityBase e, bool notifyNearbyClients = true)
        {
            if (notifyNearbyClients)
            {
                this.SendRemoveEntityToNearbyPlayers(e.World, e);
            }
            _Entities.TryRemove(e.EntityId, out e);
            Interlocked.Increment(ref _entityDictChanges);
        }

        /// <summary>
        /// Thread-friendly way of adding server entities
        /// </summary>
        public void AddEntity(EntityBase e, bool notifyNearbyClients = true)
        {
            _Entities.TryAdd(e.EntityId, e);
            Interlocked.Increment(ref _entityDictChanges);
            if (notifyNearbyClients)
            {
                this.SendEntityToNearbyPlayers(e.World, e);
            }
        }

        public void AddClient(Client client)
        {
            Clients.TryAdd(client.SessionID, client);
            Interlocked.Increment(ref _clientDictChanges);
        }

        public void RemoveClient(Client client)
        {
            Clients.TryRemove(client.SessionID, out client);
            Interlocked.Increment(ref _clientDictChanges);
        }

        public void AddAuthenticatedClient(Client client)
        {
            AuthClients.TryAdd(client.SessionID, client);
            Interlocked.Increment(ref _authClientDictChanges);
        }

        public void RemoveAuthenticatedClient(Client client)
        {
            Client removed;
            AuthClients.TryRemove(client.SessionID, out removed);
            Interlocked.Increment(ref _authClientDictChanges);

            RemoveClient(client);
        }

        public static Packet GetSpawnPacket(Server server, EntityBase entity)
        {
            Packet packet = null;
            if (entity is Player)
            {
                Player p = ((Player)entity);

                packet = new NamedEntitySpawnPacket
                {
                    EntityId = p.EntityId,
                    X = p.Position.X,
                    Y = p.Position.Y,
                    Z = p.Position.Z,
                    Yaw = p.PackedYaw,
                    Pitch = p.PackedPitch,
                    PlayerName = p.Client.Username + p.EntityId,
                    CurrentItem = 0
                };
            }
            else if (entity is ItemEntity)
            {
                ItemEntity item = (ItemEntity)entity;
                packet = new SpawnItemPacket
                {
                    X = item.Position.X,
                    Y = item.Position.Y,
                    Z = item.Position.Z,
                    Yaw = item.PackedYaw,
                    Pitch = item.PackedPitch,
                    EntityId = item.EntityId,
                    ItemId = item.ItemId,
                    Count = item.Count,
                    Durability = item.Durability,
                    Roll = 0
                };
            }
            else if (entity is Mob)
            {
                Mob mob = (Mob)entity;
                server.Logger.Log(Logger.LogLevel.Debug, ("ClientSpawn: Sending Mob " + mob.Type + " (" + mob.Position.X + ", " + mob.Position.Y + ", " + mob.Position.Z + ")"));
                packet = new MobSpawnPacket
                {
                    X = mob.Position.X,
                    Y = mob.Position.Y,
                    Z = mob.Position.Z,
                    Yaw = mob.PackedYaw,
                    Pitch = mob.PackedPitch,
                    EntityId = mob.EntityId,
                    Type = mob.Type,
                    Data = mob.Data
                };
            }

            return packet;
        }

        // TODO: This should be removed in favor of the one below
        /// <summary>
        /// Sends a packet in parallel to each nearby player.
        /// </summary>
        /// <param name="world">The world containing the coordinates.</param>
        /// <param name="absCoords>The center coordinates.</param>
        /// <param name="packet">The packet to send</param>
        public void SendPacketToNearbyPlayers(WorldManager world, AbsWorldCoords absCoords, Packet packet)
        {
            Client[] nearbyClients = GetNearbyPlayers(world, absCoords).ToArray();

            if (nearbyClients.Length == 0)
                return;

            packet.SetShared(Logger, nearbyClients.Length);
            Parallel.ForEach(nearbyClients, (client) =>
            {
                client.SendPacket(packet);
            });
        }

        /// <summary>
        /// Sends a packet in parallel to each nearby player.
        /// </summary>
        /// <param name="world">The world containing the coordinates.</param>
        /// <param name="coords">The center coordinates.</param>
        /// <param name="packet">The packet to send</param>
        public void SendPacketToNearbyPlayers(WorldManager world, UniversalCoords coords, Packet packet, Client excludedClient = null)
        {
            Client[] nearbyClients = GetNearbyPlayers(world, coords).ToArray();

            if (nearbyClients.Length == 0)
                return;

            packet.SetShared(Logger, nearbyClients.Length);

            Parallel.ForEach(nearbyClients, (client) =>
            {
                if (excludedClient != client)
                    client.SendPacket(packet);
                else
                    packet.Release();
            });
        }

        /// <summary>
        /// Sends packets in parallel to each nearby player.
        /// </summary>
        /// <param name="world">The world containing the coordinates.</param>
        /// <param name="coords">The center coordinates.</param>
        /// <param name="packets">The list of packets to send</param>
        public void SendPacketsToNearbyPlayers(WorldManager world, UniversalCoords coords, List<Packet> packets, Client excludedClient = null)
        {
            Client[] nearbyClients = GetNearbyPlayers(world, coords).ToArray();

            if (nearbyClients.Length == 0)
                return;

            foreach (Packet packet in packets)
                packet.SetShared(Logger, nearbyClients.Length);

            Parallel.ForEach(nearbyClients, (client) =>
            {
                if (excludedClient != client)
                {
                    foreach (Packet packet in packets)
                        client.SendPacket(packet);
                }
                else
                {
                    foreach (Packet packet in packets)
                        packet.Release();
                }
            });
        }

        public void SendEntityToNearbyPlayers(WorldManager world, EntityBase entity)
        {
            Packet packet;
            if ((packet = GetSpawnPacket(this, entity)) != null)
            {
                if (packet is NamedEntitySpawnPacket)
                {
                    List<Packet> packets = new List<Packet> { packet };
                    for (short i = 0; i < 5; i++)
                    {
                        packets.Add(new EntityEquipmentPacket
                        {
                            EntityId = entity.EntityId,
                            Slot = i,
                            ItemId = -1,
                            Durability = 0
                        });
                    }

                    SendPacketsToNearbyPlayers(world, UniversalCoords.FromAbsWorld(entity.Position), packets,
                                          entity is Player ? ((Player)entity).Client : null);
                }
                else
                    SendPacketToNearbyPlayers(world, UniversalCoords.FromAbsWorld(entity.Position), packet,
                                          entity is Player ? ((Player)entity).Client : null);
            }

            else if (entity is TileEntity)
            {

            }
            else
            {
                List<Packet> packets = new List<Packet> { new CreateEntityPacket { EntityId = entity.EntityId }, new EntityTeleportPacket { EntityId = entity.EntityId, Pitch = entity.PackedPitch, Yaw = entity.PackedYaw, X = entity.Position.X, Y = entity.Position.Y, Z = entity.Position.Z } };
                SendPacketsToNearbyPlayers(world, UniversalCoords.FromAbsWorld(entity.Position), packets);
            }

        }

        public void SendRemoveEntityToNearbyPlayers(WorldManager world, EntityBase entity)
        {
            SendPacketToNearbyPlayers(world, UniversalCoords.FromAbsWorld(entity.Position), new DestroyEntityPacket { EntityId = entity.EntityId }, entity is Player ? ((Player)entity).Client : null);
        }

        // TODO: This should be removed in favor of the one below
        /// <summary>
        /// Yields an enumerable of nearby players, thread-safe.
        /// </summary>
        /// <param name="world">The world containing the coordinates.</param>
        /// <param name="absCoords">The center coordinates.</param>
        /// <returns>A lazy enumerable of nearby players.</returns>
        public IEnumerable<Client> GetNearbyPlayers(WorldManager world, AbsWorldCoords absCoords)
        {
            int radius = ChraftConfig.SightRadius << 4;
            foreach (Client c in GetAuthenticatedClients())
            {
                if (c.Owner.World == world && Math.Abs(absCoords.X - c.Owner.Position.X) <= radius && Math.Abs(absCoords.Z - c.Owner.Position.Z) <= radius)
                    yield return c;
            }
        }

        /// <summary>
        /// Yields an enumerable of nearby players, thread-safe.
        /// </summary>
        /// <param name="world">The world containing the coordinates.</param>
        /// <param name="absCoords">The center coordinates.</param>
        /// <returns>A lazy enumerable of nearby players.</returns>
        public IEnumerable<Client> GetNearbyPlayers(WorldManager world, UniversalCoords coords)
        {
            int radius = ChraftConfig.SightRadius;
            foreach (Client c in GetAuthenticatedClients())
            {
                int playerChunkX = (int)Math.Floor(c.Owner.Position.X) >> 4;
                int playerChunkZ = (int)Math.Floor(c.Owner.Position.Z) >> 4;
                if (c.Owner.World == world && Math.Abs(coords.ChunkX - playerChunkX) <= radius && Math.Abs(coords.ChunkZ - playerChunkZ) <= radius)
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
            int radius = ChraftConfig.SightRadius << 4;
            foreach (EntityBase e in GetEntities())
            {
                if (e.World == world && Math.Abs(coords.X - e.Position.X) <= radius && Math.Abs(coords.Y - e.Position.Y) <= radius && Math.Abs(coords.Z - e.Position.Z) <= radius)
                    yield return e;
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
        public IEnumerable<EntityBase> GetNearbyEntities(WorldManager world, UniversalCoords coords)
        {
            int radius = ChraftConfig.SightRadius;

            foreach (EntityBase e in GetEntities())
            {
                int entityChunkX = (int)Math.Floor(e.Position.X) >> 4;
                int entityChunkZ = (int)Math.Floor(e.Position.Z) >> 4;

                if (e.World == world && Math.Abs(coords.ChunkX - entityChunkX) <= radius && Math.Abs(coords.ChunkZ - entityChunkZ) <= radius)
                    yield return e;
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
        public Dictionary<int, EntityBase> GetNearbyEntitiesDict(WorldManager world, UniversalCoords coords)
        {
            int radius = ChraftConfig.SightRadius;

            Dictionary<int, EntityBase> dict = new Dictionary<int, EntityBase>();

            foreach (EntityBase e in GetEntities())
            {
                int entityChunkX = (int)Math.Floor(e.Position.X) >> 4;
                int entityChunkZ = (int)Math.Floor(e.Position.Z) >> 4;

                if (e.World == world && Math.Abs(coords.ChunkX - entityChunkX) <= radius && Math.Abs(coords.ChunkZ - entityChunkZ) <= radius)
                {
                    dict.Add(e.EntityId, e);
                }
            }

            return dict;
        }

        public EntityBase GetEntityById(int id)
        {
            EntityBase entity;
            _Entities.TryGetValue(id, out entity);

            return entity;
        }

        public IEnumerable<EntityBase> GetNearbyLivings(WorldManager world, AbsWorldCoords coords)
        {
            int radius = ChraftConfig.SightRadius << 4;
            foreach (EntityBase entity in GetEntities())
            {
                if (!(entity is LivingEntity))
                    continue;

                if (entity.World == world && Math.Abs(coords.X - entity.Position.X) <= radius && Math.Abs(coords.Y - entity.Position.Y) <= radius && Math.Abs(coords.Z - entity.Position.Z) <= radius)
                    yield return (entity as LivingEntity);
            }
        }

        public IEnumerable<LivingEntity> GetNearbyLivings(WorldManager world, UniversalCoords coords)
        {
            int radius = ChraftConfig.SightRadius;
            foreach (EntityBase entity in GetEntities())
            {
                if (!(entity is LivingEntity))
                    continue;
                int entityChunkX = (int)Math.Floor(entity.Position.X) >> 4;
                int entityChunkZ = (int)Math.Floor(entity.Position.Z) >> 4;

                if (entity.World == world && Math.Abs(coords.ChunkX - entityChunkX) <= radius && Math.Abs(coords.ChunkZ - entityChunkZ) <= radius)
                    yield return (entity as LivingEntity);
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
            return DropItem(client.Owner.World, UniversalCoords.FromAbsWorld(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z), stack);
        }

        /// <summary>
        /// Drops an item based on the given player's position and rotation.
        /// </summary>
        /// <param name="player">The player to be used for position calculations.</param>
        /// <param name="stack">The stack to be dropped.</param>
        /// <returns>The entity ID of the item drop.</returns>
        public int DropItem(Player player, ItemStack stack)
        {
            //todo - proper drop
            return DropItem(player.World, UniversalCoords.FromAbsWorld(player.Position.X + 4, player.Position.Y, player.Position.Z), stack);
        }

        /// <summary>
        /// Drops an item at the given location.
        /// </summary>
        /// <param name="world">The world in which the coordinates reside.</param>
        /// <param name="coords">The target coordinate</param>
        /// <param name="stack">The stack to be dropped</param>
        /// <param name="velocity">An optional velocity (the velocity will be clamped to -0.4 and 0.4 on each axis)</param>
        /// <returns>The entity ID of the item drop.</returns>
        public int DropItem(WorldManager world, UniversalCoords coords, ItemStack stack, Vector3 velocity = new Vector3())
        {
            int entityId = AllocateEntity();

            bool sendVelocity = false;
            if (velocity != Vector3.Origin)
            {
                velocity = new Vector3(velocity.X.Clamp(-0.4, 0.4), velocity.Y.Clamp(-0.4, 0.4), velocity.Z.Clamp(-0.4, 0.4));
                sendVelocity = true;
            }

            AddEntity(new ItemEntity(this, entityId)
            {
                World = world,
                Position = new AbsWorldCoords(new Vector3(coords.WorldX + 0.5, coords.WorldY, coords.WorldZ + 0.5)), // Put in the middle of the block (ignoring Y)
                ItemId = stack.Type,
                Count = stack.Count,
                Velocity = velocity,
                Durability = stack.Durability
            });

            if (sendVelocity)
                SendPacketToNearbyPlayers(world, coords, new EntityVelocityPacket { EntityId = entityId, VelocityX = (short)(velocity.X * 8000), VelocityY = (short)(velocity.Y * 8000), VelocityZ = (short)(velocity.Z * 8000) });

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
        /// Pulses separated by almost exactly half a second.  Should be run in a standalone thread, as it could
        /// take some time to process.  Thread-safe, locking.
        /// </summary>
        internal void DoPulse()
        {
            foreach (Client c in GetAuthenticatedClients())
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
            Logger.Log(Logger.LogLevel.Info, "Shutting down server");
            foreach (Client c in GetClients())
                c.Kick("Server is shutting down.");
            foreach (WorldManager w in GetWorlds())
                w.Dispose();
            Running = false;

            if (Irc != null)
                Irc.Stop();

            Logger.Log(Logger.LogLevel.Info, "Server stopped, press enter to exit");
            Console.ReadLine();
            Environment.Exit(0);
        }

        internal void OnCommand(Client client, IClientCommand cmd, string[] tokens)
        {
            if (Command != null)
                Command.Invoke(client, new CommandEventArgs(client, cmd, tokens));
        }
    }
}
