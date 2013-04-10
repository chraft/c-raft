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
using System.Collections;
using System.Collections.Concurrent;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Chraft.Entity;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Interfaces;
using Chraft.Net.Packets;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;
using Chraft.Utilities.Config;
using System.Threading;
using Chraft.World.Blocks;
using Chraft.World.Weather;
using Chraft.PluginSystem;

namespace Chraft.Net
{
    public partial class Client : IClient
    {
        public ConcurrentQueue<Packet> PacketsToBeSent = new ConcurrentQueue<Packet>();
        public ConcurrentQueue<Chunk> ChunksToBeSent = new ConcurrentQueue<Chunk>();

        private int _chunkTimerRunning;

        private int _TimesEnqueuedForSend;
        private Timer _chunkSendTimer;

        private DateTime _lastChunkTimerStart = DateTime.MinValue;
        private int _startDelay;

        public byte[] Token { get; set; }

        internal void SendPacket(IPacket iPacket)
        {
            Packet packet = iPacket as Packet;
            if (!Running || ToDisconnect)
                return;

            if (packet.Logger == null)
                packet.Logger = Server.Logger;


            PacketsToBeSent.Enqueue(packet);

            int newValue = Interlocked.Increment(ref _TimesEnqueuedForSend);

            if (newValue == 1)
            {
                Server.SendClientQueue.Enqueue(this);

            }

            Server.NetworkSignal.Set();

            //Logger.Log(Chraft.LogLevel.Info, "Sending packet: {0}", packet.GetPacketType().ToString());           
        }

        private void Send_Async(byte[] data)
        {
            if (!Running || !_socket.Connected)
            {
                DisposeSendSystem();
                return;
            }

            _sendSocketEvent.SetBuffer(data, 0, data.Length);
            bool pending = _socket.SendAsync(_sendSocketEvent);
            if (!pending)
                Send_Completed(null, _sendSocketEvent);
        }

        private void Send_Sync(byte[] data)
        {
            if (!Running || !_socket.Connected)
            {
                DisposeSendSystem();
                return;
            }
            try
            {
                if (Encrypter != null)
                {
                    byte[] toDecrypt = data;
                    data = new byte[toDecrypt.Length];
                    Encrypter.TransformBlock(toDecrypt, 0, toDecrypt.Length, data, 0);
                }
                _socket.Send(data, 0, data.Length, 0);

                if (DateTime.Now + TimeSpan.FromSeconds(5) > _nextActivityCheck)
                    _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(5);
            }
            catch (Exception e)
            {
                Logger.Log(LogLevel.Error, "Exception on Send_Sync: {0}", e.ToString());
                Stop();
            }
        }

        internal void Send_Sync_Packet(Packet packet)
        {
            packet.Write();
            Send_Sync(packet.GetBuffer());
            packet.Release();
        }

        internal void Send_Start()
        {
            if (!Running || !_socket.Connected)
            {
                DisposeSendSystem();
                return;
            }

            Packet packet = null;
            try
            {
                ByteQueue byteQueue = new ByteQueue();
                int length = 0;
                while (!PacketsToBeSent.IsEmpty && length <= 1024)
                {
                    if (!PacketsToBeSent.TryDequeue(out packet))
                    {
                        Interlocked.Exchange(ref _TimesEnqueuedForSend, 0);
                        return;
                    }

                    if (!packet.Shared)
                        packet.Write();

                    byte[] packetBuffer = packet.GetBuffer();
                    length += packetBuffer.Length;

                    byteQueue.Enqueue(packetBuffer, 0, packetBuffer.Length);
                    packet.Release();

                    if (packet is DisconnectPacket)
                    {
                        ToDisconnect = true;
                        _sendSocketEvent.Completed += Disconnected;
                        break;
                    }

                }

                if (byteQueue.Length > 0)
                {
                    byte[] data = new byte[length];
                    byteQueue.Dequeue(data, 0, data.Length);

                    if (Encrypter != null)
                    {
                        byte[] toEncrypt = data;
                        data = new byte[length];
                        Encrypter.TransformBlock(toEncrypt, 0, length, data, 0);
                    }
                    Send_Async(data);
                }
                else
                {
                    Interlocked.Exchange(ref _TimesEnqueuedForSend, 0);

                    if (!PacketsToBeSent.IsEmpty)
                    {
                        int newValue = Interlocked.Increment(ref _TimesEnqueuedForSend);

                        if (newValue == 1)
                        {
                            Server.SendClientQueue.Enqueue(this);
                            Server.NetworkSignal.Set();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                MarkToDispose();
                DisposeSendSystem();
                if (packet != null)
                    Logger.Log(LogLevel.Error, "Sending packet: {0}", packet.ToString());
                Logger.Log(LogLevel.Error, e.ToString());

                // TODO: log something?
            }

        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (!Running)
                DisposeSendSystem();
            else if (e.SocketError != SocketError.Success)
            {
                MarkToDispose();
                DisposeSendSystem();
                _nextActivityCheck = DateTime.MinValue;
            }
            else
            {
                if (DateTime.Now + TimeSpan.FromSeconds(5) > _nextActivityCheck)
                    _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(5);
                Send_Start();
            }
        }

        internal void SendPulse()
        {
            if (_player != null && _player.LoggedIn)
            {
                SendPacket(new TimeUpdatePacket
                {
                    Time = _player.World.Time
                });
                //_player.SynchronizeEntities();
            }
        }

        internal void SendBlock(int x, int y, int z, byte type, byte data)
        {
            if (_player.LoggedIn)
            {
                SendPacket(new BlockChangePacket
                {
                    Data = data,
                    Type = type,
                    X = x,
                    Y = (sbyte)y,
                    Z = z
                });
            }
        }

        private void SendMotd()
        {
            string MOTD = ChraftConfig.MOTD.Replace("%u", _player.DisplayName);
            SendMessage(MOTD);
        }


        #region Login

        internal void SendEncryptionRequest()
        {
            //Console.WriteLine("Authentication hash: {0}", Server.ServerHash);

            byte[] publicKey = PacketCryptography.PublicKeyToAsn1(Server.ServerKey);
            //byte[] publicKey = AsnKeyBuilder.PublicKeyToX509(Server.ServerKey).GetBytes();
            //byte[] publicKey = PacketCryptography.PublicKeyToAsn1((RsaKeyParameters) Server.ServerKey.Public);
            short keyLength = (short)publicKey.Length;
            byte[] token = PacketCryptography.GetRandomToken();
            short tokenLength = (short)token.Length;

            Console.WriteLine("Public Key Length: {0}", keyLength);
            Console.WriteLine("Public Key: {0}", BitConverter.ToString(publicKey));
            Console.WriteLine("Token Length: {0}", tokenLength);
            Console.WriteLine("Token: {0}", BitConverter.ToString(token));
            Console.WriteLine("");
            Send_Sync_Packet(new EncryptionKeyRequest
                                 {
                                     ServerId = ConnectionId,
                                     PublicKey = publicKey,
                                     PublicKeyLength = keyLength,
                                     VerifyToken = token,
                                     VerifyTokenLength = tokenLength
                                 });
            Token = token;
        }
        internal void SendLoginRequest()
        {
            Send_Sync_Packet(new LoginRequestPacket
            {
                ProtocolOrEntityId = _player.EntityId,
                Dimension = _player.World.Dimension,
                MaxPlayers = (byte)ChraftConfig.MaxPlayers,
                Difficulty = 2,
                ServerMode = 0,
                LevelType = ChraftConfig.LevelType
            });
        }

        public void SendInitialTime(bool async = true)
        {
            Packet packet = new TimeUpdatePacket
            {
                Time = _player.World.Time
            };

            if (async)
                SendPacket(packet);
            else
                Send_Sync_Packet(packet);
        }

        internal void SendInitialPosition(bool async = true)
        {
            if (async)
                SendPacket(new PlayerPositionRotationPacket
                {
                    X = _player.Position.X,
                    Y = _player.Position.Y + this._player.EyeHeight,
                    Z = _player.Position.Z,
                    Yaw = (float)_player.Yaw,
                    Pitch = (float)_player.Pitch,
                    Stance = Stance,
                    OnGround = false
                });
            else
                Send_Sync_Packet(new PlayerPositionRotationPacket
                {
                    X = _player.Position.X,
                    Y = _player.Position.Y + this._player.EyeHeight,
                    Z = _player.Position.Z,
                    Yaw = (float)_player.Yaw,
                    Pitch = (float)_player.Pitch,
                    Stance = Stance,
                    OnGround = false
                });
        }

        internal void SendSpawnPosition(bool async = true)
        {
            Packet packet = new SpawnPositionPacket
            {
                X = _player.World.Spawn.WorldX,
                Y = _player.World.Spawn.WorldY,
                Z = _player.World.Spawn.WorldZ
            };

            if (async)
                SendPacket(packet);
            else
                Send_Sync_Packet(packet);
        }

        public bool WaitForInitialPosAck;

        internal void SendLoginSequence()
        {



            foreach (Client client in Server.GetAuthenticatedClients())
            {
                if (client.Username == Username)
                    client.Stop();
            }
            _player = new Player(Server, Server.AllocateEntity(), this);
            IBanSystem banSystem = _player.GetServer().GetBanSystem();
            if (ChraftConfig.WhiteList)
            {
                //todo has permission instead of can use command
                if (!_player.CanUseCommand("chraft.whitelist.exempt") && !banSystem.IsOnWhiteList(_player.Name))
                {
                    _player.GetClient().Kick(ChraftConfig.WhiteListMesasge);
                    return;
                }
            }

            IIpBans ipBan = banSystem.GetIpBan(_player.GetClient().IpAddress);
            IBans ban = _player.GetBan();

            if (ipBan != null)
            {
                if (DateTime.Compare(ipBan.Duration, new DateTime(1900, 01, 01, 00, 00, 00)) == 0 || ipBan.Duration >= DateTime.Now)
                {
                    _player.GetClient().Kick("Banned: " + ipBan.Reason);
                    return;
                }
                banSystem.RemoveFromIpBanList(_player.GetClient().IpAddress);
            }

            if (ban != null)
            {
                if (DateTime.Compare(ban.Duration, new DateTime(1900, 01, 01, 00, 00, 00)) == 0 || ban.Duration >= DateTime.Now)
                {
                    _player.GetClient().Kick("Banned: " + ban.Reason);
                    return;
                }
                banSystem.RemoveFromBanList(_player.Name);
            }



            _player.Permissions = _player.PermHandler.LoadClientPermission(this);
            Load();

            if (!_player.World.Running)
            {
                Stop();
                return;
            }

            SendLoginRequest();

            SendInitialTime(false);
            _player.UpdateChunks(4, CancellationToken.None, true, false);
            SendSpawnPosition(false);
            SendInitialPosition(false);
            SendInitialTime(false);
            SetGameMode();
            _player.InitializeInventory();
            _player.InitializeHealth();
            _player.SendUpdateExperience();
            _player.OnJoined();
            Server.AddEntity(_player, false);
            Server.AddAuthenticatedClient(this);
            SendMotd();
            StartKeepAliveTimer();
            _player.UpdateEntities();
            Server.SendEntityToNearbyPlayers(_player.World, _player);
            Server.FreeConnectionSlot();
        }

        internal void SendSecondLoginSequence()
        {




        }

        #endregion

        #region Chunks

        internal void SendChunk(Chunk chunk)
        {
            ChunksToBeSent.Enqueue(chunk);
            int newValue = Interlocked.Increment(ref _chunkTimerRunning);

            if (newValue == 1)
            {
                if (_lastChunkTimerStart != DateTime.MinValue)
                    _startDelay = 1000 - (int)(DateTime.Now - _lastChunkTimerStart).TotalMilliseconds;

                if (_startDelay < 0)
                    _startDelay = 0;

                if (_chunkSendTimer != null)
                    _chunkSendTimer.Change(_startDelay, 1000);
            }
        }

        internal void SendChunks(object state)
        {
            MapChunkBulkPacket packet = new MapChunkBulkPacket();
            for (int i = 0; i < 20 && !ChunksToBeSent.IsEmpty; ++i)
            {
                Chunk chunk;
                ChunksToBeSent.TryDequeue(out chunk);

                packet.ChunksToSend.Add(chunk);


            }

            SendPacket(packet);

            if (ChunksToBeSent.IsEmpty)
            {
                _chunkSendTimer.Change(Timeout.Infinite, Timeout.Infinite);
                Interlocked.Exchange(ref _chunkTimerRunning, 0);
            }

            if (!ChunksToBeSent.IsEmpty)
            {
                int running = Interlocked.Exchange(ref _chunkTimerRunning, 1);

                if (running == 0)
                {
                    if (_lastChunkTimerStart != DateTime.MinValue)
                        _startDelay = 1000 - (int)(DateTime.Now - _lastChunkTimerStart).TotalMilliseconds;

                    if (_startDelay < 0)
                        _startDelay = 0;
                    _chunkSendTimer.Change(_startDelay, 1000);
                }
            }
            _lastChunkTimerStart = DateTime.Now;
        }

        internal void SendSignTexts(Chunk chunk)
        {
            foreach (var signKVP in chunk.SignsText)
            {
                int blockX = signKVP.Key >> 11;
                int blockY = (signKVP.Key & 0xFF) % 128;
                int blockZ = (signKVP.Key >> 7) & 0xF;

                UniversalCoords coords = UniversalCoords.FromBlock(chunk.Coords.ChunkX, chunk.Coords.ChunkZ, blockX, blockY, blockZ);

                string[] lines = new string[4];

                int length = signKVP.Value.Length;

                for (int i = 0; i < 4; ++i, length -= 15)
                {
                    int currentLength = length;
                    if (currentLength > 15)
                        currentLength = 15;

                    if (length > 0)
                        lines[i] = signKVP.Value.Substring(i * 15, currentLength);
                    else
                        lines[i] = "";
                }

                SendPacket(new UpdateSignPacket { X = coords.WorldX, Y = coords.WorldY, Z = coords.WorldZ, Lines = lines });
            }
        }

        #endregion


        #region Entities

        internal void SendCreateEntity(EntityBase entity)
        {
            Packet packet;
            if ((packet = (Server.GetSpawnPacket(entity) as Packet)) != null)
            {
                if (packet is NamedEntitySpawnPacket)
                {
                    SendPacket(packet);
                    for (short i = 0; i < 5; i++)
                    {
                        SendPacket(new EntityEquipmentPacket
                        {
                            EntityId = entity.EntityId,
                            Slot = i,
                            Item = ItemHelper.Void
                        });
                    }
                }
            }
            else if (entity is TileEntity)
            {

            }
            else
            {
                SendEntity(entity);
                SendTeleportTo(entity);
            }
        }

        internal void SendEntity(EntityBase entity)
        {
            SendPacket(new CreateEntityPacket
            {
                EntityId = entity.EntityId
            });
        }

        internal void SendEntityMetadata(LivingEntity entity)
        {
            SendPacket(new EntityMetadataPacket
            {
                EntityId = entity.EntityId,
                Data = entity.Data
            });
        }

        internal void SendDestroyEntity(EntityBase entity)
        {
            SendPacket(new DestroyEntityPacket
            {
                EntitiesCount = 1,
                EntitiesId = new[] { entity.EntityId }
            });
        }

        internal void SendDestroyEntities(int[] entities)
        {
            SendPacket(new DestroyEntityPacket
            {
                EntitiesCount = 1,
                EntitiesId = entities
            });
        }

        internal void SendTeleportTo(EntityBase entity)
        {
            SendPacket(new EntityTeleportPacket
            {
                EntityId = entity.EntityId,
                X = entity.Position.X,
                Y = entity.Position.Y,
                Z = entity.Position.Z,
                Yaw = entity.PackedYaw,
                Pitch = entity.PackedPitch
            });

            //SendMoveBy(entity, (sbyte)((_Player.Position.X - (int)entity.Position.X) * 32), (sbyte)((_Player.Position.Y - (int)entity.Position.Y) * 32), (sbyte)((_Player.Position.Z - (int)entity.Position.Z) * 32));
        }

        internal void SendRotateBy(EntityBase entity, sbyte dyaw, sbyte dpitch)
        {
            SendPacket(new EntityLookPacket
            {
                EntityId = entity.EntityId,
                Yaw = dyaw,
                Pitch = dpitch
            });
        }

        internal void SendMoveBy(EntityBase entity, sbyte dx, sbyte dy, sbyte dz)
        {
            SendPacket(new EntityRelativeMovePacket
            {
                EntityId = entity.EntityId,
                DeltaX = dx,
                DeltaY = dy,
                DeltaZ = dz
            });
        }

        internal void SendMoveRotateBy(EntityBase entity, sbyte dx, sbyte dy, sbyte dz, sbyte dyaw, sbyte dpitch)
        {
            SendPacket(new EntityLookAndRelativeMovePacket
            {
                EntityId = entity.EntityId,
                DeltaX = dx,
                DeltaY = dy,
                DeltaZ = dz,
                Yaw = dyaw,
                Pitch = dpitch
            });
        }

        internal void SendAttachEntity(EntityBase entity, EntityBase attachTo)
        {
            SendPacket(new AttachEntityPacket
            {
                EntityId = entity.EntityId,
                VehicleId = attachTo.EntityId
            });
        }

        #endregion


        #region Clients

        internal void SendHoldingEquipment(Client c) // Updates entity holding via 0x05
        {
            SendPacket(new EntityEquipmentPacket
            {
                EntityId = c.Owner.EntityId,
                Slot = 0,
                Item = c.Owner.Inventory.ActiveItem as ItemInventory
            });
        }

        internal void SendEntityEquipment(Client c, short slot) // Updates entity equipment via 0x05
        {
            SendPacket(new EntityEquipmentPacket
            {
                EntityId = c.Owner.EntityId,
                Slot = slot,
                Item = c.Owner.Inventory[slot]
            });
        }

        #endregion

        internal void SendWeather(WeatherState weather, UniversalCoords coords)
        {

            //throw new NotImplementedException();
        }
    }
}
