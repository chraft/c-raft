using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Chraft.Commands;
using Chraft.Entity;
using Chraft.Entity.Mobs;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Plugins.Events;
using Chraft.World;
using Chraft.Utils;
using Chraft.Properties;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using System.Collections.Concurrent;

namespace Chraft.Net
{
    public partial class Client
    {
        internal const int ProtocolVersion = 17;
        private Socket _Socket;
        public volatile bool Running = true;
        public PacketHandler PacketHandler { get; private set; }
        private Timer KeepAliveTimer;
        private Player _Player = null;

        public static SocketAsyncEventArgsPool SendSocketEventPool = new SocketAsyncEventArgsPool(10);
        public static SocketAsyncEventArgsPool RecvSocketEventPool = new SocketAsyncEventArgsPool(10);
        public static BufferPool RecvBufferPool = new BufferPool("Receive", 2048, 2048);


        private byte[] _RecvBuffer;
        private SocketAsyncEventArgs _SendSocketEvent;
        private SocketAsyncEventArgs _RecvSocketEvent;

        private ByteQueue _CurrentBuffer;
        private ByteQueue _ProcessedBuffer;
        private ByteQueue _FragPackets;

        private bool _SendSystemDisposed;
        private bool _RecvSystemDisposed;

        private object _DisposeLock = new object();

        public Player Owner
        {
            get { return _Player; }
        }

        public ByteQueue FragPackets
        {
            get { return _FragPackets; }
            set { _FragPackets = value; }
        }

        /// <summary>
        /// A reference to the server logger.
        /// </summary>
        public Logger Logger { get { return _Player.Server.Logger; } }

        /// <summary>
        /// Instantiates a new Client object.
        /// </summary>
        /// <param name="server">The Server to associate with the entity.</param>
        /// <param name="sessionId">The entity ID for the client.</param>
        /// <param name="tcp">The TCP client to be used for communication.</param>
        internal Client(Socket socket, Player player)
        {
            _Socket = socket;
            _Player = player;
            _Player.Client = this;
            _CurrentBuffer = new ByteQueue();
            _ProcessedBuffer = new ByteQueue();
            _FragPackets = new ByteQueue();
            //PacketHandler = new PacketHandler(Server, socket);
        }

        private void SetGameMode()
        {
            SendPacket(new NewInvalidStatePacket
            {
                GameMode = _Player.GameMode,
                Reason = NewInvalidStatePacket.NewInvalidReason.ChangeGameMode
            });
        }

        public void Start()
        {
            Running = true;
            _SendSocketEvent = SendSocketEventPool.Pop();
            _RecvSocketEvent = RecvSocketEventPool.Pop();
            _RecvBuffer = RecvBufferPool.AcquireBuffer();

            _RecvSocketEvent.SetBuffer(_RecvBuffer, 0, _RecvBuffer.Length);
            _RecvSocketEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Recv_Completed);

            _SendSocketEvent.Completed += new EventHandler<SocketAsyncEventArgs>(Send_Completed);

            new Task(Recv_Start).Start();
        }

        /*internal void AssociateInterface(Interface iface)
        {
            iface.PacketHandler = PacketHandler;
        }*/

        private void CloseInterface()
        {
            if (_Player.CurrentInterface == null)
                return;
            SendPacket(new CloseWindowPacket
            {
                WindowId = _Player.CurrentInterface.Handle
            });
        }

        public int Ping { get; set; }
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
                LastKeepAliveId = _Player.Server.Rand.Next();
                KeepAliveStart = DateTime.Now;
                SendPacket(new KeepAlivePacket() {KeepAliveID = this.LastKeepAliveId});
            }
        }

        /// <summary>
        /// Stop reading packets from the client, and kill the keep-alive timer.
        /// </summary>
        public void Stop()
        {
            _Player.Ready = false;
            MarkToDispose();
            DisposeRecvSystem();
            DisposeSendSystem();
            if (KeepAliveTimer != null)
            {
                KeepAliveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                KeepAliveTimer = null;
            }
        }

        /// <summary>
        /// Disconnect the client with the given reason.
        /// </summary>
        /// <param name="reason">The reason to be displayed to the player.</param>
        public void Kick(string reason)
        {
            //Event
            ClientKickedEventArgs e = new ClientKickedEventArgs(this, reason);
            _Player.Server.PluginManager.CallEvent(Event.PLAYER_KICKED, e);
            if (e.EventCanceled) return;
            reason = e.Message;
            //End Event
            SendPacket(new DisconnectPacket
            {
                Reason = reason
            });
        }

        public void Disconnected(object sender, SocketAsyncEventArgs e)
        {
            // Just wait a bit since it's possible that we close the socket before the packet reached the client
            Thread.Sleep(200);
            Stop();
        }

        /// <summary>
        /// Disposes associated resources and stops the client.  Also removes the client from the server's client/entity lists.
        /// </summary>
        public void Dispose()
        {
            _Player.Server.Logger.Log(Chraft.Logger.LogLevel.Info, "Disposing {0}", _Player.DisplayName);
            string disconnectMsg = ChatColor.Yellow + _Player.DisplayName + " has left the game.";
            //Event
            ClientLeftEventArgs e = new ClientLeftEventArgs(this);
            _Player.Server.PluginManager.CallEvent(Plugins.Events.Event.PLAYER_LEFT, e);
            //You cant stop the player from leaving so dont try.
            disconnectMsg = e.BrodcastMessage;
            //End Event
            _Player.Server.Broadcast(disconnectMsg);

            if(_Player.LoggedIn)
                Save();
            _Player.LoggedIn = false;

            _Player.Server.RemoveClient(this);
            _Player.Server.Logger.Log(Chraft.Logger.LogLevel.Info, "Clients online: {0}", _Player.Server.Clients.Count);
            _Player.Server.RemoveEntity(_Player);
            foreach (int packedCoords in _Player.LoadedChunks.Keys)
            {
                Chunk chunk = _Player.World.GetChunk(UniversalCoords.FromPackedChunk(packedCoords), false, false);
                if (chunk != null)
                    chunk.RemoveClient(this);
            }

            RecvBufferPool.ReleaseBuffer(_RecvBuffer);
            SendSocketEventPool.Push(_SendSocketEvent);
            RecvSocketEventPool.Push(_RecvSocketEvent);

            if (_Socket.Connected)
                _Socket.Close();

            GC.Collect();
        }

        public void MarkToDispose()
        {
            lock (_DisposeLock)
            {
                if (Running)
                {
                    Running = false;
                    StopUpdateChunks();
                }
            }
        }

        public void DisposeSendSystem()
        {
            lock(_DisposeLock)
            {
                if (!_SendSystemDisposed)
                {
                    _SendSystemDisposed = true;
                    if (_RecvSystemDisposed)
                    {
                        Server.ClientsToDispose.Enqueue(this);
                        _Player.Server.NetworkSignal.Set();
                    }
                }
            }
        }

        public void DisposeRecvSystem()
        {
            lock (_DisposeLock)
            {
                if (!_RecvSystemDisposed)
                {
                    _RecvSystemDisposed = true;
                    if (_SendSystemDisposed)
                    {
                        Server.ClientsToDispose.Enqueue(this);
                        _Player.Server.NetworkSignal.Set();
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
            KeepAliveTimer = new Timer(KeepAliveTimer_Callback, null, 10000, 10000);
        }

        /// <summary>
        /// Updates nearby players when Client is hurt.
        /// </summary>
        /// <param name="cause"></param>
        /// <param name="hitBy">The Client hurting the current Client.</param>
        /// <param name="args">First argument should always be the damage amount.</param>
        public void DamageClient(DamageCause cause, EntityBase hitBy = null, params object[] args)
        {

            //event start
            EntityDamageEventArgs entevent = new EntityDamageEventArgs(_Player, Convert.ToInt16(args[0]), null, cause);
            _Player.Server.PluginManager.CallEvent(Event.ENTITY_DAMAGE, entevent);
            if (_Player.GameMode == 1) { entevent.EventCanceled = true; }
            if (entevent.EventCanceled) return;
            //event end

            switch (cause)
            {
                case DamageCause.BlockExplosion:
                    break;
                case DamageCause.Contact:
                    break;
                case DamageCause.Drowning:
                    break;
                case DamageCause.EntityAttack:
                    if (hitBy != null)
                    {

                    }
                    break;
                case DamageCause.EntityExplosion:
                    break;
                case DamageCause.Fall:
                    if (args.Length > 0)
                    {
                        _Player.Health -= Convert.ToInt16(args[0]);
                    }
                    break;
                case DamageCause.Fire:
                    break;
                case DamageCause.FireBurn:
                    break;
                case DamageCause.Lava:
                    break;
                case DamageCause.Lightning:
                    break;
                case DamageCause.Projectile:
                    break;
                case DamageCause.Suffocation:
                    break;
                case DamageCause.Void:
                    break;
                default:
                    _Player.Health -= 1;
                    break;

            }

            SendPacket(new UpdateHealthPacket
            {
                Health = _Player.Health,
                Food = Owner.Food,
                FoodSaturation = Owner.FoodSaturation,
            });

            foreach (Client c in _Player.Server.GetNearbyPlayers(_Player.World, new AbsWorldCoords(_Player.Position.X, _Player.Position.Y, _Player.Position.Z)))
            {
                if (c == this)
                    continue;

                c.SendPacket(new AnimationPacket // Hurt Animation
                {
                    Animation = 2,
                    PlayerId = _Player.EntityId
                });

                c.SendPacket(new EntityStatusPacket // Hurt Action
                {
                    EntityId = _Player.EntityId,
                    EntityStatus = 2
                });
            }

            if (_Player.Health == 0)
                _Player.HandleDeath(hitBy);
        }

        public string FacingDirection(byte points)
        {

            byte rotation = (byte)(_Player.Position.Yaw * 256 / 360); // Gives rotation as 0 - 255, 0 being due E.

            if (points == 8)
            {
                if (rotation < 17 || rotation > 240)
                    return "E";
                if (rotation < 49)
                    return "SE";
                if (rotation < 81)
                    return "S";
                if (rotation < 113)
                    return "SW";
                if (rotation > 208)
                    return "NE";
                if (rotation > 176)
                    return "N";
                if (rotation > 144)
                    return "NW";
                return "W";
            }
            if (rotation < 32 || rotation > 224)
                return "E";
            if (rotation < 76)
                return "S";
            if (rotation > 140)
                return "N";
            return "W";
        }
    }
}