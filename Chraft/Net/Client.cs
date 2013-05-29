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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Chraft.Entity;
using Chraft.Net.Packets;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Plugins.Events;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World;
using Chraft.Utils;
using Chraft.PluginSystem;

namespace Chraft.Net
{
    public partial class Client : IClient
    {
        internal const int ProtocolVersion = 47;
        internal const string MinecraftServerVersion = "1.4.2";
        private readonly Socket _socket;
        public volatile bool Running = true;
        internal PacketHandler PacketHandler { get; private set; }
        private Timer _keepAliveTimer;
        private Player _player = null;

        internal static SocketAsyncEventArgsPool SendSocketEventPool = new SocketAsyncEventArgsPool(10);
        internal static SocketAsyncEventArgsPool RecvSocketEventPool = new SocketAsyncEventArgsPool(10);
        internal static BufferPool RecvBufferPool = new BufferPool("Receive", 2048, 2048);

        
        private byte[] _recvBuffer;
        private SocketAsyncEventArgs _sendSocketEvent;
        private SocketAsyncEventArgs _recvSocketEvent;

        private ByteQueue _currentBuffer;
        private ByteQueue _processedBuffer;
        private ByteQueue _fragPackets;

        private bool _sendSystemDisposed;
        private bool _recvSystemDisposed;

        private readonly object _disposeLock = new object();

        private DateTime _nextActivityCheck;

        internal Server Server { get; set; }

        internal int SessionID { get; private set; }

        /// <summary>
        /// The mixed-case, clean username of the client.
        /// </summary>
        public string Username { get; internal set; }

        public string Host { get; set; }

        public Player Owner
        {
            get { return _player; }
        }

        internal ByteQueue FragPackets
        {
            get { return _fragPackets; }
            set { _fragPackets = value; }
        }

        public bool ToDisconnect { get; set; }

        /// <summary>
        /// A reference to the server logger.
        /// </summary>
        internal Logger Logger { get { return Server.Logger; } }

        /// <summary>
        /// A unique Id used as the ServerId within the Authentication process
        /// </summary>
        internal string ConnectionId { get; set; }

        internal byte[] SharedKey { get; set; }
        internal ICryptoTransform Encrypter { get; set; }
        internal ICryptoTransform Decrypter { get; set; }
        public string IpAddress { get; private set; }

        /// <summary>
        /// Instantiates a new Client object.
        /// </summary>
        internal Client(int sessionId, Server server, Socket socket)
        {
            _socket = socket;
            _currentBuffer = new ByteQueue();
            _processedBuffer = new ByteQueue();
            _fragPackets = new ByteQueue();
            _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(10);
            SessionID = sessionId;
            Server = server;
            
            // Generate a unique ServerId for each client
            byte[] bytes = new byte[8];
            Server.Rand.NextBytes(bytes);
            ConnectionId = BitConverter.ToString(bytes).Replace("-", "");

            _chunkSendTimer = new Timer(SendChunks, null, Timeout.Infinite, Timeout.Infinite);
            IpAddress = _socket.RemoteEndPoint != null
                          ? (_socket.RemoteEndPoint as IPEndPoint).Address.ToString()
                          : (_socket.LocalEndPoint as IPEndPoint).Address.ToString();
            //PacketHandler = new PacketHandler(Server, socket);
          
        }

        public IPlayer GetOwner()
        {
            return _player;
        }

        public IServer GetServer()
        {
            return Server;
        }

        public ILogger GetLogger()
        {
            return Logger;
        }

        public bool CheckUsername(string username)
        {
            string usernameToCheck = Regex.Replace(username, Chat.DISALLOWED, "");
            Logger.Log(LogLevel.Debug, "Username: {0}", usernameToCheck);
            return usernameToCheck == Username;
        }

        private void SetGameMode()
        {
            SendPacket(new NewInvalidStatePacket
            {
                GameMode = (byte)_player.GameMode,
                Reason = NewInvalidStatePacket.NewInvalidReason.ChangeGameMode
            });
        }

        internal void Start()
        {
            Running = true;
            _sendSocketEvent = SendSocketEventPool.Pop();
            _recvSocketEvent = RecvSocketEventPool.Pop();
            _recvBuffer = RecvBufferPool.AcquireBuffer();

            _recvSocketEvent.SetBuffer(_recvBuffer, 0, _recvBuffer.Length);
            _recvSocketEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Recv_Completed);

            _sendSocketEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);

            Task.Factory.StartNew(Recv_Start);
        }

        /*internal void AssociateInterface(Interface iface)
        {
            iface.PacketHandler = PacketHandler;
        }*/

        private void CloseInterface()
        {
            if (_player.CurrentInterface == null)
                return;
            SendPacket(new CloseWindowPacket
            {
                WindowId = _player.CurrentInterface.Handle
            });
        }

        public int Ping { get; internal set; }
        public int LastKeepAliveId;
        public DateTime KeepAliveStart;
        public DateTime LastClientResponse = DateTime.Now;

        private void KeepAliveTimer_Callback(object sender)
        {
            if (Running)
            {
                if ((DateTime.Now - LastClientResponse).TotalSeconds > 60)
                {
                    // Client hasn't sent or responded to a keepalive within 60secs
                    this.Stop();
                    return;
                }
                LastKeepAliveId = _player.Server.Rand.Next();
                KeepAliveStart = DateTime.Now;
                SendPacket(new KeepAlivePacket() {KeepAliveID = this.LastKeepAliveId});
            }
        }

        internal void CheckAlive()
        {
            if(DateTime.Now > _nextActivityCheck)
                Stop();
        }

        /// <summary>
        /// Stop reading packets from the client, and kill the keep-alive timer.
        /// </summary>
        internal void Stop()
        {
            MarkToDispose();
            DisposeRecvSystem();
            DisposeSendSystem();
        }

        /// <summary>
        /// Disconnect the client with the given reason.
        /// </summary>
        /// <param name="reason">The reason to be displayed to the player.</param>
        public void Kick(string reason)
        {
            //Event
            ClientKickedEventArgs e = new ClientKickedEventArgs(this, reason);
            Server.PluginManager.CallEvent(Event.PlayerKicked, e);
            if (e.EventCanceled) return;
            reason = e.Message;
            //End Event

            if(_player != null && _player.LoggedIn)
                Save();

            SendPacket(new DisconnectPacket
            {
                Reason = reason
            });
        }

        internal void Disconnected(object sender, SocketAsyncEventArgs e)
        {
            if (_player != null && _player.LoggedIn)
                Save();
            // Just wait a bit since it's possible that we close the socket before the packet reached the client
            Thread.Sleep(200);
            Stop();
        }

        /// <summary>
        /// Disposes associated resources and stops the client.  Also removes the client from the server's client/entity lists.
        /// </summary>
        public void Dispose()
        {
            if (_player != null)
            {
                Server.Logger.Log(LogLevel.Info, "Disposing {0}", _player.DisplayName);
                string disconnectMsg = ChatColor.Yellow + _player.DisplayName + " has left the game.";
                //Event
                ClientLeftEventArgs e = new ClientLeftEventArgs(this);
                Server.PluginManager.CallEvent(Event.PlayerLeft, e);
                //You cant stop the player from leaving so dont try.
                disconnectMsg = e.BrodcastMessage;
                //End Event

                if (_player.LoggedIn)
                {
                    _player.Server.BroadcastSync(disconnectMsg, this);
                    Save();
                }

                Task.Factory.StartNew(() =>
                {
                    foreach (Chunk chunk in _player.LoadedChunks.Values)
                    {
                        if (chunk != null)
                            chunk.RemoveClient(this);
                    }
                });

                Server.RemoveAuthenticatedClient(this);

                Server.Logger.Log(LogLevel.Info, "Clients online: {0}", Server.Clients.Count);
                Server.RemoveEntity(_player, false);

                Client[] nearbyClients = Server.GetNearbyPlayersInternal(_player.World, UniversalCoords.FromAbsWorld(_player.Position)).ToArray();

                foreach (var client in nearbyClients)
                {
                    if (client != this)
                    {
                        DestroyEntityPacket de = new DestroyEntityPacket { EntitiesId = new [] { _player.EntityId } };
                        de.Write();
                        byte[] data = de.GetBuffer();
                        client.Send_Sync(data);
                    }
                }

                _player.LoggedIn = false;
                _player.Ready = false;
                Running = false;

                if (_keepAliveTimer != null)
                {
                    _keepAliveTimer.Dispose();
                    _keepAliveTimer = null;
                }
            }
            else
            {
                Server.Logger.Log(LogLevel.Info, "Disposing {0}", Username);
                Running = false;
                Server.RemoveClient(this);
                Server.Logger.Log(LogLevel.Info, "Clients online: {0}", Server.Clients.Count);
                Server.FreeConnectionSlot();
            }

            _chunkSendTimer.Dispose();
            _chunkSendTimer = null;

            RecvBufferPool.ReleaseBuffer(_recvBuffer);
            SendSocketEventPool.Push(_sendSocketEvent);
            RecvSocketEventPool.Push(_recvSocketEvent);

            if (_socket.Connected)
            {
                try
                {
                    _socket.Shutdown(SocketShutdown.Both);
                }
                catch(SocketException)
                {
                    // Ignore errors in socket shutdown (e.g. if client crashes there is a no connection error when trying to shutdown)
                }
            }
            _socket.Close();
           
            //GC.Collect();
        }

        public void MarkToDispose()
        {
            lock (_disposeLock)
            {
                if (Running)
                {
                    Running = false;
                    StopUpdateChunks();
                }
            }
        }

        internal void DisposeSendSystem()
        {
            lock(_disposeLock)
            {
                if (!_sendSystemDisposed)
                {
                    _sendSystemDisposed = true;
                    if (_recvSystemDisposed)
                    {
                        Server.ClientsToDispose.Enqueue(this);
                        Server.NetworkSignal.Set();
                    }
                }
            }
        }

        internal void DisposeRecvSystem()
        {
            lock (_disposeLock)
            {
                if (!_recvSystemDisposed)
                {
                    _recvSystemDisposed = true;
                    if (_sendSystemDisposed)
                    {
                        Server.ClientsToDispose.Enqueue(this);
                        Server.NetworkSignal.Set();
                    }
                }
            }
        }

        /// <summary>
        /// Sends a message to the player via chat.
        /// </summary>
        /// <param name="message">The message to be displayed in the chat HUD.</param>
        public void SendMessage(string message)
        {
            SendPacket(new ChatMessagePacket
            {
                Message = message
            });
        }

        private void StartKeepAliveTimer()
        {
            _keepAliveTimer = new Timer(KeepAliveTimer_Callback, null, 10000, 10000);
        }
    }
}