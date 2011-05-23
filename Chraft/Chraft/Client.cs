using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading;
using Chraft.Entity;
using Chraft.Net;
using Chraft.World;
using Chraft.Utils;
using Chraft.Properties;
using Chraft.Interfaces;
using org.bukkit.@event.player;

namespace Chraft
{
    public partial class Client
    {
        internal const int ProtocolVersion = 9;
        private volatile TcpClient Tcp;
        private Thread RxThread;
        private volatile bool Running = true;
        public PacketHandler PacketHandler { get; private set; }
        private Timer KeepAliveTimer;
        private List<PointI> LoadedChunks = new List<PointI>();
        private List<EntityBase> LoadedEntities = new List<EntityBase>();
        private volatile bool LoggedIn = false;
        private Interface CurrentInterface = null;
        private PermissionHandler Permissions;
		private Location LeftGround = null;

        internal int SessionID { get; private set; }

        /// <summary>
        /// The mixed-case, clean username of the client.
        /// </summary>
        public string Username { get; private set; }

        /// <summary>
        /// The name that we display of the client
        /// </summary>
        public string DisplayName { get; set; }

        /// <summary>
        /// The current inventory.
        /// </summary>
        public Inventory Inventory { get; private set; }

        /// <summary>
        /// Is the client muted from chat
        /// </summary>
        public bool IsMuted { get; set; }

        /// <summary>
        /// A reference to the server logger.
        /// </summary>
        public Logger Logger { get { return Server.Logger; } }

		/// <summary>
		/// Gets or sets whether the client is sneaking.
		/// </summary>
		public bool IsSneaking { get; set; }

		/// <summary>
		/// Gets or sets the number of sleep ticks. >=0 indicates sleeping.
		/// </summary>
		public int SleepTicks { get; set; }

        /// <summary>
        /// Instantiates a new Client object.
        /// </summary>
        /// <param name="server">The Server to associate with the entity.</param>
        /// <param name="sessionId">The entity ID for the client.</param>
        /// <param name="tcp">The TCP client to be used for communication.</param>
        internal Client(Server server, int sessionId, TcpClient tcp)
            : base(server, sessionId)
        {
            EnsureServer(server);
            SessionID = sessionId;
            Tcp = tcp;
            PacketHandler = new PacketHandler(Server, tcp);
            Inventory = null;
            Permissions = new PermissionHandler(Server);
            DisplayName = Username;
			Health = 20;
			MaxHealth = 20;
            InitializePosition();
            InitializeRecv();
        }

        private void InitializePosition()
        {
            World = Server.GetDefaultWorld();
            X = World.Spawn.X;
            Y = World.Spawn.Y + 1;
            Z = World.Spawn.Z;
        }

        private void InitializeInventory()
        {
            if (Inventory == null)
            {
                Inventory = new Inventory(this);

                for (int i = 0; i < Inventory.SlotCount; i++) // Void inventory slots (for Holding)
                {
                    Inventory.Slots[i] = ItemStackChraft.Void;
                }

                Inventory[Inventory.ActiveSlot] = new ItemStackChraft(278, 1, 0);
            }

            Inventory.UpdateClient();
        }

        private void InitializeHealth()
        {
            if (Health == 0)
            {
                Health = 20;
            }

            PacketHandler.SendPacket(new UpdateHealthPacket
            {
                Health = this.Health
            });
        }

		private void UpdateOnGround(bool onGround)
		{
			if (OnGround)
				LeftGround = new Location(X, Y, Z, Yaw, Pitch);
			OnGround = onGround;
		}

        internal void AssociateInterface(Interface iface)
        {
            iface.PacketHandler = PacketHandler;
        }

        private void CloseInterface()
        {
            if (CurrentInterface == null)
                return;
            PacketHandler.SendPacket(new CloseWindowPacket
            {
                WindowId = CurrentInterface.Handle
            });
        }

        private void KeepAliverTimer_Callback(object sender)
        {
            if (Running)
                PacketHandler.SendPacket(new KeepAlivePacket());
        }

        /// <summary>
        /// Move less than four blocks to the given destination.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        public override void MoveTo(double x, double y, double z)
        {
			MoveTo(x, y, z, Yaw, Pitch);
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
			PlayerMoveEvent e = new PlayerMoveEvent(this, getLocation(), new org.bukkit.Location(World, x, y, z));
			Server.BukkitPluginManager.callEvent(e);
			if (e.isCancelled())
				return;
			org.bukkit.Location loc = e.getTo();
            base.MoveTo(loc.getX(), loc.getY(), loc.getZ(), loc.getYaw(), loc.getPitch());
            UpdateEntities();
        }

		public override bool TeleportTo(double x, double y, double z)
		{
			PlayerTeleportEvent e = new PlayerTeleportEvent(this, new org.bukkit.Location(World, X, Y, Z), new org.bukkit.Location(World, x, y, z));
			Server.BukkitPluginManager.callEvent(e);
			if (e.isCancelled())
				return false;
			org.bukkit.Location loc = e.getTo();
			return base.TeleportTo(loc.getX(), loc.getY(), loc.getZ());
		}

        private void UpdateEntities()
        {
            IEnumerable<EntityBase> nearbyEntities = Server.GetNearbyEntities(World, X, Y, Z);

            foreach (EntityBase e in nearbyEntities)
            {
                if (e.Equals(this))
                    continue;
                if (!LoadedEntities.Contains(e))
                    SendCreateEntity(e);
                if (e is ItemEntity && Math.Abs(e.X - X) < 1 && Math.Abs(e.Y - Y) < 1 && Math.Abs(e.Z - Z) < 1)
                    PickupItem((ItemEntity)e);
            }

            foreach (EntityBase e in LoadedEntities)
            {
                if (nearbyEntities.Contains(e))
                    continue;
                SendDestroyEntity(e);
            }

            LoadedEntities = new List<EntityBase>(nearbyEntities);
        }

        /// <summary>
        /// Updates nearby players when Client is hurt.
        /// </summary>
        /// <param name="hitBy">The Client hurting the current Client.</param>
        public void DamageClient(EntityBase hitBy = null)
        {
            // TODO: Calcualte Damage vs Amour etc
            //this.SendMessage("Hit with: " + hitBy.Inventory.ActiveItem.Type);
            if (hitBy != null)
            {
                // Get the Clients held item.
                this.Health -= 1;
            }
            else
            {
                // Generic damage from Mobs??
                this.Health -= 1;
            }          

            PacketHandler.SendPacket(new UpdateHealthPacket
            {
                Health = this.Health
            });

            foreach (Client c in Server.GetNearbyPlayers(World, X, Y, Z))
            {
                if (c == this)
                    continue;

                c.PacketHandler.SendPacket(new AnimationPacket // Hurt Animation
                {
                    Animation = 2,
                    PlayerId = this.EntityId
                });

                c.PacketHandler.SendPacket(new EntityStatusPacket // Hurt Action
                {
                    EntityId = this.EntityId,
                    EntityStatus = 2
                });
            }

            if (this.Health == 0)
                HandleDeath(hitBy);
        }

        /// <summary>
        /// Handles the death of the Client.
        /// </summary>
        /// <param name="hitBy">Who killed the current Client.</param>
        private void HandleDeath(EntityBase hitBy = null, string deathBy = "")
        {
            // TODO: Add config option for none/global/local death messages
            // ...Or maybe make messages a plugin?
            string deathMessage;

            if (hitBy == null && deathBy == "") // Generic message
            {
                deathBy = "mysteriously!";
            }
            else if (hitBy is Client)
            {
                Client c = (Client)hitBy;
                deathBy = "by " + c.DisplayName.ToString() + " using" + Server.Items.ItemName(c.Inventory.Slots[c.Inventory.ActiveSlot].Type);
            }
            else if (hitBy is Mob)
            {
                Mob m = (Mob)hitBy;
                deathBy = "by " + m.Type;
            }

            deathMessage = this.DisplayName.ToString() + " was killed " + deathBy;

            foreach (Client c in Server.GetNearbyPlayers(World, X, Y, Z))
            {
                c.SendMessage(deathMessage);

                if (c == this)
                    continue;

                c.PacketHandler.SendPacket(new EntityStatusPacket // Death Action
                {
                    EntityId = this.EntityId,
                    EntityStatus = 3
                });
            }

            for (int i = 0; i < Inventory.Slots.Length; i++)
            {
                if (Inventory.Slots[i].Type > 0)
                {
                    Server.DropItem(World, (int)X, (int)Y, (int)Z, Inventory.Slots[i]);
                    Inventory.Slots[i] = ItemStackChraft.Void;
                }
            }
        }

        /// <summary>
        /// Handles the respawning of the Client, called from respawn packet.
        /// </summary>
        private void HandleRespawn()
        {
			PlayerRespawnEvent e = new PlayerRespawnEvent(this, new org.bukkit.Location(World, World.Spawn.X, World.Spawn.Y, World.Spawn.Z));
			Server.BukkitPluginManager.callEvent(e);
			org.bukkit.Location loc = e.getRespawnLocation();

            // HACK: This can no doubt be improved as waiting on the updatechunk thread is quite slow.
            Server.Entities.Remove(this);
			try
			{
				X = loc.getX();
				Y = loc.getY();
				Z = loc.getZ();

				UpdateEntities();
				SendSpawnPosition();
				SendInitialPosition();
				InitializeInventory();
				InitializeHealth();

				PacketHandler.SendPacket(new RespawnPacket { });
			}
			finally
			{
				Server.Entities.Add(this);
			}
        }

        private void PickupItem(ItemEntity item)
        {
            lock (Server.Entities)
            {
                if (!Server.Entities.Contains(item))
                    return;
                Server.Entities.Remove(item);
            }

            foreach (Client c in Server.GetNearbyPlayers(item.World, item.X, item.Y, item.Z))
            {
                c.PacketHandler.SendPacket(new CollectItemPacket
                {
                    EntityId = item.EntityId,
                    PlayerId = EntityId
                });
            }

            Inventory.AddItem(item.ItemId, (sbyte)item.Count, item.Durability);
        }

        
        private string FacingDirection(byte points)
        {

            byte rotation = (byte)(Yaw * 256 / 360); // Gives rotation as 0 - 255, 0 being due E.
            
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

        private void SynchronizeEntities()
        {
            foreach (EntityBase e in Server.GetNearbyEntities(World, X, Y, Z))
            {
                if (e.Equals(this))
                    continue;
                PacketHandler.SendPacket(new EntityTeleportPacket
                {
                    EntityId = e.EntityId,
                    X = e.X,
                    Y = e.Y,
                    Z = e.Z,
                    Yaw = e.PackedYaw,
                    Pitch = e.PackedPitch
                });
            }
        }

        private void UpdateChunksThread()
        {
            while (Running)
            {
                UpdateChunks(Settings.Default.SightRadius);
                Thread.Sleep(100);
            }
        }

        private void UpdateChunks(int radius)
        {
            List<PointI> nearbyChunks = new List<PointI>();
            int chunkX = (int)X >> 4;
            int chunkZ = (int)Z >> 4;
            for (int x = chunkX - radius; x <= chunkX + radius; x++)
            {
                for (int z = chunkZ - radius; z <= chunkZ + radius; z++)
                {
                    nearbyChunks.Add(new PointI(x, z));
                    if (LoadedChunks.Contains(new PointI(x, z)))
                        continue;
                    SendPreChunk(x, z, true);
                }
            }

            foreach (PointI c in nearbyChunks)
            {
                if (LoadedChunks.Contains(c))
                    continue;
                SendChunk(World[c.X, c.Z]);
            }

            foreach (PointI c in LoadedChunks)
            {
                if (nearbyChunks.Contains(c))
                    continue;
                SendPreChunk(c.X, c.Z, false);
                World[c.X, c.Z].RemoveClient(this);
            }

            LoadedChunks = nearbyChunks;
        }

        /// <summary>
        /// Start reading packets from the client in a separate thread.
        /// </summary>
        public void Start()
        {
            RxThread = new Thread((ThreadStart)RxProc);
            RxThread.IsBackground = true;
            RxThread.Start();
        }

        /// <summary>
        /// Stop reading packets from the client, and kill the keep-alive timer.
        /// </summary>
        public void Stop()
        {
            Running = false;
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
			reason = OnKick(reason);
            PacketHandler.SendPacket(new DisconnectPacket
            {
                Reason = reason
            });
            Stop();
        }

		private string OnKick(string reason)
		{
			PlayerKickEvent e = new PlayerKickEvent(this, reason, "§c" + Username + " was kicked from the server.");
			Server.BukkitPluginManager.callEvent(e);
			Server.Broadcast(e.getLeaveMessage());
			return e.getReason();
		}

        private void RxProc()
        {
            try
            {
                while (Running)
                {
                    if (!this.PacketHandler.ProcessPacket())
                        break;
                }
            }
            catch (Exception ex)
            {
                Logger.Log(Logger.LogLevel.Error, "Killing client: " + ex);
            }
            finally
            {
                Dispose();
            }
        }

        /// <summary>
        /// Disposes associated resources and stops the client.  Also removes the client from the server's client/entity lists.
        /// </summary>
        public void Dispose()
        {
            Save();
            Running = false;
            PacketHandler.Dispose();

            Server.Clients.Remove(this.SessionID);
            Server.Entities.Remove(this);
            foreach (PointI c in LoadedChunks)
                World[c.X, c.Z].RemoveClient(this);

            if (Tcp.Connected)
                Tcp.Close();

            GC.Collect();
        }

        /// <summary>
        /// Sends a message to the player via chat.
        /// </summary>
        /// <param name="message">The message to be displayed in the chat HUD.</param>
        public void SendMessage(string message)
        {
            PacketHandler.SendPacket(new ChatMessagePacket
            {
                Message = message
            });
        }

        private void StartKeepAliveTimer()
        {
            KeepAliveTimer = new Timer(KeepAliverTimer_Callback, null, 10000, 10000);
        }

        private bool CheckUsername(string username)
        {
            string usernameToCheck = Regex.Replace(username, Chat.DISALLOWED, "");
            Logger.Log(Logger.LogLevel.Debug, "Username: {0}", usernameToCheck);
            return usernameToCheck == Username;
        }

        private void OnJoined()
        {
            LoggedIn = true;
        }

        #region Permission related commands
        //Check if the player has permissions to use the command
        private bool CanUseCommand(string command)
        {
            return Permissions.CanUseCommand(Username, command);
        }
        //Returns the players prefix
        public string GetPlayerPrefix(string playerName)
        {
            return Permissions.GetPlayerPrefix(playerName);
        }
        //returns the players suffix
        public string GetPlayerSuffix(string playerName)
        {
            return Permissions.GetPlayerSuffix(playerName);
        }
        //returns if a player can build
        public bool CanPlayerBuild(string playerName)
        {
            return Permissions.CanPlayerBuild(playerName);
        }
        #endregion
    }
}
