using System;
using System.Collections.Concurrent;
using System.Net.Sockets;
using Chraft.Entity;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Properties;
using System.Threading;
using Chraft.World.Blocks;
using Chraft.World.Weather;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Chraft.Net
{
    public partial class Client 
    {
        public ConcurrentQueue<Packet> PacketsToBeSent = new ConcurrentQueue<Packet>();

        private int _TimesEnqueuedForSend;
        public void SendPacket(Packet packet)
        {
            if (!Running)
                return;

            if (packet.Logger == null)
                packet.Logger = _player.Server.Logger;

            PacketsToBeSent.Enqueue(packet);

            int newValue = Interlocked.Increment(ref _TimesEnqueuedForSend);

            if ((newValue - 1) == 0)
                Server.SendClientQueue.Enqueue(this);

            //Logger.Log(Chraft.Logger.LogLevel.Info, "Sending packet: {0}", packet.GetPacketType().ToString());

            _player.Server.NetworkSignal.Set();
        }

        public void Send_Async(byte[] data)
        {
            if (!Running || !_socket.Connected)
            {
                DisposeSendSystem();
                return;
            }

            if (data[0] == (byte)PacketType.Disconnect)
                _sendSocketEvent.Completed += Disconnected;

            _sendSocketEvent.SetBuffer(data, 0, data.Length);

            bool pending = _socket.SendAsync(_sendSocketEvent);

            _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(2.5);

            if (!pending)
                Send_Completed(null, _sendSocketEvent);
        }

        public void Send_Sync(byte[] data)
        {
            if (!Running || !_socket.Connected)
            {
                DisposeSendSystem();
                return;
            }
            try
            {
                _socket.Send(data, data.Length, 0);
                _nextActivityCheck = DateTime.Now + TimeSpan.FromSeconds(2.5);
            }
            catch (Exception)
            {
                Stop();

            }
            
        }

        public void Send_Start(Packet packet = null)
        {
            if (!Running || !_socket.Connected)
            {
                DisposeSendSystem();

                if(packet != null)
                    packet.Release();
                return;
            }

            try
            {
                byte[] data;
                if (packet == null)
                {
                    if (PacketsToBeSent.Count > 0)
                    {
                        if (!PacketsToBeSent.TryDequeue(out packet))
                        {
                            Interlocked.Exchange(ref _TimesEnqueuedForSend, 0);
                            return;
                        }

                        if (!packet.Shared)
                            packet.Write();
                                                   
                        data = packet.GetBuffer();
                        
                        if (packet.Async)
                        {
                            Send_Async(data);
                            packet.Release();
                        }
                        else
                        {
                            Send_Sync(data);
                            packet.Release();
                            packet = null;
                            while (Running && PacketsToBeSent.Count > 0)
                            {
                                if (!PacketsToBeSent.TryDequeue(out packet))
                                {
                                    Interlocked.Exchange(ref _TimesEnqueuedForSend, 0);
                                    return;
                                }

                                if (packet.Async)
                                    break;

                                if (!packet.Shared)
                                    packet.Write();

                                data = packet.GetBuffer();

                                Send_Sync(data);
                                packet.Release();
                                packet = null;
                            }

                            if (packet != null)
                                Send_Start(packet);
                            else
                                Interlocked.Exchange(ref _TimesEnqueuedForSend, 0);
                        }
                    }
                    else
                        Interlocked.Exchange(ref _TimesEnqueuedForSend, 0);
                }
                else
                {
                    if (!packet.Shared)
                        packet.Write();
                    
                    data = packet.GetBuffer();
                    
                    Send_Async(data);
                    packet.Release();
                }
            }
            catch (Exception e)
            {
                MarkToDispose();
                DisposeSendSystem();
                if(packet != null)
                    Logger.Log(Logger.LogLevel.Error, "Sending packet: {0}", packet.ToString());
                Logger.Log(Logger.LogLevel.Error, e.ToString());

                // TODO: log something?
            }
            
        }

        private void Send_Completed(object sender, SocketAsyncEventArgs e)
        {
            if (e.Buffer[0] == (byte)PacketType.Disconnect)
                e.Completed -= Disconnected;
            if (!Running)
                DisposeSendSystem();
            else if(e.SocketError != SocketError.Success)
            {
                MarkToDispose();
                DisposeSendSystem();
                _nextActivityCheck = DateTime.MinValue;
            }
            else
                Send_Start();
        }

        internal void SendPulse()
        {
            if(_player.LoggedIn)
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

        /// <summary>
        /// Updates a region of blocks
        /// </summary>
        /// <param name="x">Start coordinate, X component</param>
        /// <param name="y">Start coordinate, Y component</param>
        /// <param name="z">Start coordinate, Z component</param>
        /// <param name="l">Length (X magnitude)</param>
        /// <param name="w">Height (Y magnitude)</param>
        /// <param name="h">Width (Z magnitude)</param>
        public void SendBlockRegion(int x, int y, int z, int l, int h, int w)
        {
            for (int dx = 0; dx < l; dx++)
            {
                for (int dy = 0; dy < h; dy++)
                {
                    for (int dz = 0; dz < w; dz++)
                    {
                        byte? type = _player.World.GetBlockOrNull(x + dx, y + dy, z + dz);
                        if (type != null)
                            SendBlock(x + dx, y + dy, z + dz, type.Value, _player.World.GetBlockData(x + dx, y + dy, z + dz));
                    }
                }
            }
        }

        private void SendMotd()
        {
            string MOTD = Settings.Default.MOTD.Replace("%u", _player.DisplayName);
            SendMessage(MOTD);
        }


        #region Login

        public void SendLoginRequest()
        {
            SendPacket(new LoginRequestPacket
            {
                ProtocolOrEntityId = _player.EntityId,
                Dimension = _player.World.Dimension,
                Username = "",
                MapSeed = _player.World.Seed,
                WorldHeight = 128,
                MaxPlayers = 50,
                Unknown = 2
            });
        }

        public void SendInitialTime()
        {
            SendPacket(new TimeUpdatePacket
            {
                Time = _player.World.Time
            });
        }

        public void SendInitialPosition()
        {
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
        }

        public void SendSpawnPosition()
        {
            SendPacket(new SpawnPositionPacket
            {
                X = _player.World.Spawn.WorldX,
                Y = _player.World.Spawn.WorldY,
                Z = _player.World.Spawn.WorldZ
            });
        }

        public void SendHandshake()
        {
            SendPacket(new HandshakePacket
            {

                UsernameOrHash = (_player.Server.UseOfficalAuthentication ? _player.Server.ServerHash : "-")
                //UsernameOrHash = "-" // No authentication
                //UsernameOrHash = this.Server.ServerHash // Official Minecraft server authentication
            });
        }

        public void SendLoginSequence()
        {
            _player.Server.AddAuthenticatedClient(this);
            _player.Permissions = _player.PermHandler.LoadClientPermission(this);
            Load();
            StartKeepAliveTimer();
            SendLoginRequest();
            SendSpawnPosition();
            SendInitialTime();
            // This must be sent sync otherwise we will fall through them
            _player.UpdateChunks(2, true, CancellationToken.None);
            SendInitialPosition();
            SendInitialTime();
            SetGameMode();
            _player.InitializeInventory();
            _player.InitializeHealth();
            _player.OnJoined();
            SendMotd();
        }

        #endregion

        #region Chunks

        public void SendPreChunk(int x, int z, bool load, bool sync)
        {
            PreChunkPacket prepacket = new PreChunkPacket
            {
                Load = load,
                X = x,
                Z = z,
                Async = !sync
            };
            SendPacket(prepacket);
        }

        internal void SendChunk(Chunk chunk, bool sync)
        {
            MapChunkPacket packet = new MapChunkPacket
            {
                Chunk = chunk,
                Async = !sync
            };
            SendPacket(packet);
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

        public void SendCreateEntity(EntityBase entity)
        {
            if (entity is Player)
            {
                Player p = ((Player) entity);
                Client c = p.Client;

                SendPacket(new NamedEntitySpawnPacket
                {
                    EntityId = p.EntityId,
                    X = p.Position.X,
                    Y = p.Position.Y,
                    Z = p.Position.Z,
                    Yaw = p.PackedYaw,
                    Pitch = p.PackedPitch,
                    PlayerName = p.Username + p.EntityId,
                    CurrentItem = 0
                });
                for (short i = 0; i < 5; i++)
                {
                    SendPacket(new EntityEquipmentPacket
                    {
                        EntityId = p.EntityId,
                        Slot = i,
                        ItemId = -1,
                        Durability = 0
                    });
                }
            }
            else if (entity is ItemEntity)
            {
                ItemEntity item = (ItemEntity)entity;
                SendPacket(new SpawnItemPacket
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
                });
            }
            else if (entity is Mob)
            {
                
                Mob mob = (Mob)entity;
                Logger.Log(Logger.LogLevel.Debug, ("ClientSpawn: Sending Mob " + mob.Type + " (" + mob.Position.X + ", " + mob.Position.Y + ", " + mob.Position.Z + ")"));
                SendPacket(new MobSpawnPacket
                {
                    X = mob.Position.X,
                    Y = mob.Position.Y,
                    Z = mob.Position.Z,
                    Yaw = mob.PackedYaw,
                    Pitch = mob.PackedPitch,
                    EntityId = mob.EntityId,
                    Type = mob.Type,
                    Data = mob.Data
                });
            }
            else
                if (entity is TileEntity)
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

        public void SendDestroyEntity(EntityBase entity)
        {
            SendPacket(new DestroyEntityPacket
            {
                EntityId = entity.EntityId
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

        public void SendHoldingEquipment(Client c) // Updates entity holding via 0x05
        {
            SendPacket(new EntityEquipmentPacket
            {
                EntityId = c.Owner.EntityId,
                Slot = 0,
                ItemId = c.Owner.Inventory.ActiveItem.Type,
                Durability = c.Owner.Inventory.ActiveItem.Durability
            });
        }

        public void SendEntityEquipment(Client c, short slot) // Updates entity equipment via 0x05
        {
            SendPacket(new EntityEquipmentPacket
            {
                EntityId = c.Owner.EntityId,
                Slot = slot,
                ItemId = c.Owner.Inventory.Slots[slot].Type,
                Durability = 0
            });
        }

        #endregion

        public void SendWeather(WeatherState weather, UniversalCoords coords)
        {

            //throw new NotImplementedException();
        }
    }
}
