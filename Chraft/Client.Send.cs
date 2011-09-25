using System;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Properties;
using System.Threading;
using Chraft.World.Weather;
using System.Threading.Tasks;

namespace Chraft
{
    public partial class Client 
    {
        internal void SendPulse()
        {
            if (LoggedIn)
            {
                SynchronizeEntities();
                PacketHandler.SendPacket(new TimeUpdatePacket
                {
                    Time = World.Time
                });
            }
        }

        internal void SendBlock(int x, int y, int z, byte type, byte data)
        {
            if (LoggedIn)
            {
                PacketHandler.SendPacket(new BlockChangePacket
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
                        byte? type = World.GetBlockOrNull(x + dx, y + dy, z + dz);
                        if (type != null)
                            SendBlock(x + dx, y + dy, z + dz, type.Value, World.GetBlockData(x + dx, y + dy, z + dz));
                    }
                }
            }
        }

        private void SendMotd()
        {
            string MOTD = Settings.Default.MOTD.Replace("%u", DisplayName);
            SendMessage(MOTD);
        }

        public void SendPacket(Packet packet)
        {
            this.PacketHandler.SendPacket(packet);
        }


        #region Login

        private void SendLoginRequest()
        {
            PacketHandler.SendPacket(new LoginRequestPacket
            {
                ProtocolOrEntityId = SessionID,
                Dimension = World.Dimension,
                Username = "",
                MapSeed = World.Seed,
                WorldHeight = 128,
                MaxPlayers = 50,
                Unknown = 2
            });
        }

        private void SendInitialTime()
        {
            PacketHandler.SendPacket(new TimeUpdatePacket
            {
                Time = World.Time
            });
        }

        private void SendInitialPosition()
        {
            PacketHandler.SendPacket(new PlayerPositionRotationPacket
            {
                X = Position.X,
                Y = Position.Y + EyeGroundOffset,
                Z = Position.Z,
                Yaw = (float)Position.Yaw,
                Pitch = (float)Position.Pitch,
                Stance = Stance,
                OnGround = false
            });
        }

        private void SendSpawnPosition()
        {
            PacketHandler.SendPacket(new SpawnPositionPacket
            {
                X = World.Spawn.X,
                Y = World.Spawn.Y,
                Z = World.Spawn.Z
            });
        }

        private void SendHandshake()
        {
            PacketHandler.SendPacket(new HandshakePacket
            {
                
                UsernameOrHash = (Server.UseOfficalAuthentication ? Server.ServerHash : "-")
                //UsernameOrHash = "-" // No authentication
                //UsernameOrHash = this.Server.ServerHash // Official Minecraft server authentication
            });
        }

        private void SendLoginSequence()
        {
            Permissions = PermHandler.LoadClientPermission(this);
            SendMessage("§cLoading, please wait...");
            Load();
            StartKeepAliveTimer();
            SendLoginRequest();
            SendSpawnPosition();
            SendInitialTime();
            UpdateChunks(2, CancellationToken.None);
            SendInitialPosition();
            SendInitialTime();
            InitializeInventory();
            InitializeHealth();
            OnJoined();
            SendMotd();
            SendMessage("§cLoading complete.");
            _UpdateChunks = new Task(() => { UpdateChunks(Settings.Default.SightRadius, CancellationToken.None); });
            _UpdateChunks.Start();
        }

        #endregion


        #region Chunks

        private void SendPreChunk(int x, int z, bool load)
        {
            PreChunkPacket prepacket = new PreChunkPacket
            {
                Load = load,
                X = x,
                Z = z
            };
            PacketHandler.SendPacket(prepacket);
        }

        internal void SendChunk(Chunk chunk)
        {
            MapChunkPacket packet = new MapChunkPacket
            {
                Chunk = chunk
            };
            PacketHandler.SendPacket(packet);
        }

        #endregion


        #region Entities

        private void SendCreateEntity(EntityBase entity)
        {
            if (entity is Client)
            {
                Client c = (Client)entity;
                PacketHandler.SendPacket(new NamedEntitySpawnPacket
                {
                    EntityId = c.EntityId,
                    X = c.Position.X,
                    Y = c.Position.Y,
                    Z = c.Position.Z,
                    Yaw = c.PackedYaw,
                    Pitch = c.PackedPitch,
                    PlayerName = c.Username + c.EntityId,
                    CurrentItem = 0
                });
                for (short i = 0; i < 5; i++)
                {
                    PacketHandler.SendPacket(new EntityEquipmentPacket
                    {
                        EntityId = c.EntityId,
                        Slot = i,
                        ItemId = -1,
                        Durability = 0
                    });
                }
            }
            else if (entity is ItemEntity)
            {
                ItemEntity item = (ItemEntity)entity;
                PacketHandler.SendPacket(new SpawnItemPacket
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
                PacketHandler.SendPacket(new MobSpawnPacket
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
            PacketHandler.SendPacket(new CreateEntityPacket
            {
                EntityId = entity.EntityId
            });
        }

        private void SendDestroyEntity(EntityBase entity)
        {
            PacketHandler.SendPacket(new DestroyEntityPacket
            {
                EntityId = entity.EntityId
            });
        }

        internal void SendTeleportTo(EntityBase entity)
        {
            PacketHandler.SendPacket(new EntityTeleportPacket
            {
                EntityId = entity.EntityId,
                X = entity.Position.X,
                Y = entity.Position.Y,
                Z = entity.Position.Z,
                Yaw = entity.PackedYaw,
                Pitch = entity.PackedPitch
            });
            SendMoveBy(entity, (sbyte)((Position.X - (int)entity.Position.X) * 32), (sbyte)((Position.Y - (int)entity.Position.Y) * 32), (sbyte)((Position.Z - (int)entity.Position.Z) * 32));
        }

        internal void SendRotateBy(EntityBase entity, sbyte dyaw, sbyte dpitch)
        {
            PacketHandler.SendPacket(new EntityLookPacket
            {
                EntityId = entity.EntityId,
                Yaw = dyaw,
                Pitch = dpitch
            });
        }

        internal void SendMoveBy(EntityBase entity, sbyte dx, sbyte dy, sbyte dz)
        {
            PacketHandler.SendPacket(new EntityRelativeMovePacket
            {
                EntityId = entity.EntityId,
                DeltaX = dx,
                DeltaY = dy,
                DeltaZ = dz
            });
        }

        internal void SendMoveRotateBy(EntityBase entity, sbyte dx, sbyte dy, sbyte dz, sbyte dyaw, sbyte dpitch)
        {
            PacketHandler.SendPacket(new EntityLookAndRelativeMovePacket
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
            PacketHandler.SendPacket(new AttachEntityPacket
            {
                EntityId = entity.EntityId,
                VehicleId = attachTo.EntityId
            });
        }

        #endregion


        #region Clients

        private void SendHoldingEquipment(Client c) // Updates entity holding via 0x05
        {
            PacketHandler.SendPacket(new EntityEquipmentPacket
            {
                EntityId = c.EntityId,
                Slot = 0,
                ItemId = c.Inventory.ActiveItem.Type,
                Durability = c.Inventory.ActiveItem.Durability
            });
        }

        private void SendEntityEquipment(Client c, short slot) // Updates entity equipment via 0x05
        {
            PacketHandler.SendPacket(new EntityEquipmentPacket
            {
                EntityId = c.EntityId,
                Slot = slot,
                ItemId = c.Inventory.Slots[slot].Type,
                Durability = 0
            });
        }

        #endregion

        public void SendWeather(WeatherState weather, int i, int i1)
        {

            //throw new NotImplementedException();
        }
    }
}
