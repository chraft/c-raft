using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using Chraft.Commands;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net.Packets;
using Chraft.Plugins.Events;
using Chraft.Plugins.Events.Args;
using Chraft.Utils;
using Chraft.World;
using Chraft.Net;

namespace Chraft
{
    //Cheat enum since enums can't be strings.
    public static class ChatColor
    {
        public static string Black = "§0",
        DarkBlue = "§1",
        DarkGreen = "§2",
        DarkTeal = "§3",
        DarkRed = "§4",
        Purple = "§5",
        Gold = "§6",
        Gray = "§7",
        DarkGray = "§8",
        Blue = "§9",
        BrightGreen = "§a",
        Teal = "§b",
        Red = "§c",
        Pink = "§d",
        Yellow = "§e",
        White = "§f";
    }

    public class Player : EntityBase
    {
        public ConcurrentDictionary<PointI, Chunk> LoadedChunks = new ConcurrentDictionary<PointI, Chunk>();
        private List<EntityBase> LoadedEntities = new List<EntityBase>();
        public volatile bool LoggedIn = false;
        public PermissionHandler PermHandler;
        public ClientPermission Permissions;
        public Interface CurrentInterface = null;

        internal int SessionID { get; private set; }
        private Client _Client;

        public Client Client
        {
            get { return _Client; }
            set { _Client = value; }
        }

        /// <summary>
        /// The mixed-case, clean username of the client.
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// The name that we display of the client
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The current inventory.
        /// </summary>
        public Inventory Inventory { get; set; }

        /// <summary>
        /// Is the client muted from chat
        /// </summary>
        public bool IsMuted { get; set; }

        public const double EyeGroundOffset = 1.6200000047683716;

        public bool Ready { get; set; }

        public byte GameMode { get; set; }

        public Player(Server server, int sessionId) : base(server, sessionId)
        {
            EnsureServer(server);
            Inventory = null;
            DisplayName = Username;
            SessionID = sessionId;
            InitializePosition();
            PermHandler = new PermissionHandler(server);
        }

        public void InitializePosition()
        {
            World = Server.GetDefaultWorld();
            Position = new Location(
                World.Spawn.X,
                World.Spawn.Y + EyeGroundOffset,
                World.Spawn.Z);
        }
        public void InitializeInventory()
        {
            if (Inventory == null)
            {
                Inventory = new Inventory(this);

                for (short i = 0; i < Inventory.SlotCount; i++) // Void inventory slots (for Holding)
                {
                    Inventory[i] = ItemStack.Void;
                }

                Inventory[Inventory.ActiveSlot] = new ItemStack(278, 1, 0);
            }

            Inventory.UpdateClient();
        }

        public float FoodSaturation { get; set; }
        public short Food { get; set; }

        public void InitializeHealth()
        {
            if (Health <= 0)
            {
                Health = 20;
            }

            if (Food <= 0)
            {
                Food = 20;
            }
            FoodSaturation = 5.0f;

            _Client.SendPacket(new UpdateHealthPacket
            {
                Health = this.Health,
                Food = this.Food,
                FoodSaturation = this.FoodSaturation,
            });
        }

        /// <summary>
        /// Move less than four blocks to the given destination.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        public override void MoveTo(double x, double y, double z)
        {
            base.MoveTo(x, y, z);
            UpdateEntities();
        }

        /// <summary>
        /// Move less than four blocks to the given destination and rotate.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        /// <param name="yaw">The absolute yaw to which client should change.</param>
        /// <param name="pitch">The absolute pitch to which client should change.</param>
        public override void MoveTo(double x, double y, double z, float yaw, float pitch)
        {
            base.MoveTo(x, y, z, yaw, pitch);
            UpdateEntities();
        }

        public override void OnMoveTo(sbyte x, sbyte y, sbyte z)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if(!c.Owner.Equals(this))
                    c.SendMoveBy(this, x, y, z);
            }
        }

        public override void OnTeleportTo(double x, double y, double z)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!Equals(this))
                    c.SendTeleportTo(this);
                else
                {
                    c.SendPacket(new PlayerPositionRotationPacket
                    {
                        X = x,
                        Y = y + Player.EyeGroundOffset,
                        Z = z,
                        Yaw = (float)Position.Yaw,
                        Pitch = (float)Position.Pitch,
                        Stance = c.Stance,
                        OnGround = false
                    }
                    );
                }
            }
        }

        public override void OnRotateTo()
        {
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Owner.Equals(this))
                    c.SendRotateBy(this, PackedYaw, PackedPitch);
            }
        }

        public override void OnMoveRotateTo(sbyte x, sbyte y, sbyte z)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Owner.Equals(this))
                    c.SendMoveRotateBy(this, x, y, z, PackedYaw, PackedPitch);
            }
        }

        public void UpdateEntities()
        {
            IEnumerable<EntityBase> nearbyEntities = Server.GetNearbyEntities(World, Position.X, Position.Y, Position.Z);

            foreach (EntityBase e in nearbyEntities)
            {
                if (e.Equals(this))
                    continue;
                if (!LoadedEntities.Contains(e))
                    _Client.SendCreateEntity(e);
                if (this.Health > 0 && e is ItemEntity && Math.Abs(e.Position.X - Position.X) < 1 && Math.Abs(e.Position.Y - Position.Y) < 1 && Math.Abs(e.Position.Z - Position.Z) < 1)
                    PickupItem((ItemEntity)e);
            }

            foreach (EntityBase e in LoadedEntities)
            {
                if (nearbyEntities.Contains(e))
                    continue;
                _Client.SendDestroyEntity(e);
            }

            LoadedEntities = new List<EntityBase>(nearbyEntities);
        }

        /// <summary>
        /// Handles the death of the Client.
        /// </summary>
        /// <param name="hitBy">Who killed the current Client.</param>
        public void HandleDeath(EntityBase hitBy = null, string deathBy = "")
        {
            // TODO: Add config option for none/global/local death messages
            // ...Or maybe make messages a plugin?
            string deathMessage;

            if (hitBy == null && deathBy == "") // Generic message
            {
                deathBy = "mysteriously!";
            }
            else if (hitBy is Player)
            {
                Player p = (Player)hitBy;
                deathBy = "by " + DisplayName.ToString() + " using" + Server.Items.ItemName(Inventory.Slots[Inventory.ActiveSlot].Type);
            }
            else if (hitBy is Mob)
            {
                Mob m = (Mob)hitBy;
                deathBy = "by " + m.Type;
            }

            deathMessage = this.DisplayName.ToString() + " was killed " + deathBy;

            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                c.SendMessage(deathMessage);

                if (c == _Client)
                    continue;

                c.SendPacket(new EntityStatusPacket // Death Action
                {
                    EntityId = this.EntityId,
                    EntityStatus = 3
                });
            }

            Inventory.DropAll((int)Position.X, (int)Position.Y, (int)Position.Z);
        }

        /// <summary>
        /// Handles the respawning of the Client, called from respawn packet.
        /// </summary>
        public void HandleRespawn()
        {
            // This can no doubt be improved as waiting on the updatechunk thread is quite slow.
            Server.RemoveEntity(this);

            Position.X = World.Spawn.X;
            Position.Y = World.Spawn.Y + EyeGroundOffset;
            Position.Z = World.Spawn.Z;

            _Client.StopUpdateChunks();
            UpdateChunks(1, CancellationToken.None, false);
            _Client.SendPacket(new RespawnPacket { });
            UpdateEntities();
            //SendSpawnPosition();
            _Client.SendInitialPosition();
            _Client.SendInitialTime();
            InitializeInventory();
            InitializeHealth();
            _Client.ScheduleUpdateChunks();


            Server.AddEntity(this);
        }

        private void PickupItem(ItemEntity item)
        {
            if (!Server.GetEntities().Contains(item))
                return;
            Server.RemoveEntity(item);

            foreach (Client c in Server.GetNearbyPlayers(item.World, item.Position.X, item.Position.Y, item.Position.Z))
            {
                c.SendPacket(new CollectItemPacket
                {
                    EntityId = item.EntityId,
                    PlayerId = EntityId
                });
            }

            Inventory.AddItem(item.ItemId, (sbyte)item.Count, item.Durability);
        }

        public void SynchronizeEntities()
        { 
            foreach (EntityBase e in Server.GetNearbyEntities(World, Position.X, Position.Y, Position.Z))
            {
                if (e.Equals(this))
                    continue;

                _Client.SendPacket(new EntityTeleportPacket
                {
                    EntityId = e.EntityId,
                    X = e.Position.X,
                    Y = e.Position.Y,
                    Z = e.Position.Z,
                    Yaw = e.PackedYaw,
                    Pitch = e.PackedPitch
                });
            }
        }

        public void UpdateChunks(int radius, CancellationToken token)
        {
            UpdateChunks(radius, token, false, true);
        }

        public void UpdateChunks(int radius, bool sync, CancellationToken token)
        {
            UpdateChunks(radius, token, sync, true);
        }

        public void UpdateChunks(int radius, CancellationToken token, bool remove)
        {
            UpdateChunks(radius, token, false, remove);
        }

        public void UpdateChunks(int radius, CancellationToken token, bool sync, bool remove)
        {
            List<PointI> nearbyChunks = new List<PointI>();
            List<PointI> toUpdate = new List<PointI>();
            int chunkX = (int)Position.X >> 4;
            int chunkZ = (int)Position.Z >> 4;

            for (int x = chunkX - radius; x <= chunkX + radius; ++x)
            {
                for (int z = chunkZ - radius; z <= chunkZ + radius; ++z)
                {
                    if (token.IsCancellationRequested)
                        return;

                    nearbyChunks.Add(new PointI(x, z));

                    if (!LoadedChunks.ContainsKey(new PointI(x, z)))
                    {
                        toUpdate.Add(new PointI(x, z));
                        _Client.SendPreChunk(x, z, true, sync);
                    }
                }
            }

            foreach (PointI c in toUpdate)
            {
                if (token.IsCancellationRequested)
                    return;

                Chunk chunk = World[c.X, c.Z, true, true];
                chunk.AddClient(_Client);
                LoadedChunks.TryAdd(c, chunk);
                _Client.SendChunk(chunk, sync);
            }

            if (remove)
            {
                foreach (PointI c in LoadedChunks.Keys.Where<PointI>(c => !nearbyChunks.Contains(c)))
                {
                    if (token.IsCancellationRequested)
                        return;

                    _Client.SendPreChunk(c.X, c.Z, false, sync);
                    Chunk chunk;
                    LoadedChunks.TryRemove(c, out chunk);
                    chunk.RemoveClient(_Client);
                }
            }

        }

        public bool CheckUsername(string username)
        {
            string usernameToCheck = Regex.Replace(username, Chat.DISALLOWED, "");
            _Client.Logger.Log(Logger.LogLevel.Debug, "Username: {0}", usernameToCheck);
            return usernameToCheck == Username;
        }

        public void OnJoined()
        {
            LoggedIn = true;
            string DisplayMessage = DisplayName + " has logged in";
            //Event
            ClientJoinedEventArgs e = new ClientJoinedEventArgs(_Client);
            Server.PluginManager.CallEvent(Event.PLAYER_JOINED, e);
            //We kick the player because it would not work to use return.
            if (e.EventCanceled) _Client.Kick("");
            DisplayMessage = e.BrodcastMessage;
            //End Event
            Server.Broadcast(DisplayMessage);
        }


        public void SetHealth(short health)
        {
            if (health > 20)
            {
                health = 20;
            }
            this.Health = health;
            _Client.SendPacket(new UpdateHealthPacket
            {
                Health = this.Health,
                Food = this.Food,
                FoodSaturation = this.FoodSaturation,
            });
        }


        #region Permission related commands
        //Check if the player has permissions to use the command from a command object
        public bool CanUseCommand(Command command)
        {
            return PermHandler.HasPermission(this, command);
        }

        //
        public bool CanUseCommand(string command)
        {
            return PermHandler.HasPermission(Username, command);
        }

        //Returns the players prefix
        public string GetPlayerPrefix()
        {
            return PermHandler.GetPlayerPrefix(this);
        }
        //returns the players suffix
        public string GetPlayerSuffix()
        {
            return PermHandler.GetPlayerSuffix(this);
        }
        #endregion
    }
}
