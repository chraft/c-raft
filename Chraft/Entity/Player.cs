using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
using System.Text.RegularExpressions;
using System.Threading;
using Chraft.Commands;
using Chraft.Interfaces;
using Chraft.Net.Packets;
using Chraft.Plugins.Events;
using Chraft.Plugins.Events.Args;
using Chraft.Utils;
using Chraft.World;
using Chraft.Net;

namespace Chraft.Entity
{
    public class Player : LivingEntity
    {
        public override string Name
        {
            get { return DisplayName; }
        }
        public ConcurrentDictionary<int, Chunk> LoadedChunks = new ConcurrentDictionary<int, Chunk>();
        private List<EntityBase> LoadedEntities = new List<EntityBase>();
        public volatile bool LoggedIn = false;
        public PermissionHandler PermHandler;
        public ClientPermission Permissions;
        public Interface CurrentInterface = null;

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

        public override float EyeHeight
        {
            get { return 1.62f; }
        }

        public bool Ready { get; set; }

        public byte GameMode { get; set; }

        public Player(Server server, int entityId) : base(server, entityId)
        {
            EnsureServer(server);
            Inventory = null;
            DisplayName = Username;
            InitializePosition();
            PermHandler = new PermissionHandler(server);
        }

        public void InitializePosition()
        {
            World = Server.GetDefaultWorld();
            Position = new AbsWorldCoords(
                World.Spawn.WorldX,
                World.Spawn.WorldY + this.EyeHeight,
                World.Spawn.WorldZ);
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
                Health = Health,
                Food = Food,
                FoodSaturation = FoodSaturation,
            });
        }

        #region Suffocation/drowning
        protected override void Suffocate(object state)
        {
            if (!LoggedIn)
            {
                StopSuffocationTimer();
                return;
            }
            Console.WriteLine("Suffocation");
            base.Suffocate(state);
        }

        protected override void Drown(object state)
        {
            if (!LoggedIn)
            {
                StopDrowningTimer();
                return;
            }
            base.Drown(state);
        }
        #endregion

        #region Movement

        /// <summary>
        /// Move less than four blocks to the given destination.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        public override void MoveTo(AbsWorldCoords absCoords)
        {
            base.MoveTo(absCoords);
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
        public override void MoveTo(AbsWorldCoords absCoords, float yaw, float pitch)
        {
            base.MoveTo(absCoords, yaw, pitch);
            UpdateEntities();
        }

        public override void OnTeleportTo(AbsWorldCoords absCoords)
        {
            base.OnTeleportTo(absCoords);
            Client.SendPacket(new PlayerPositionRotationPacket
                                  {
                                      X = absCoords.X,
                                      Y = absCoords.Y + EyeHeight,
                                      Z = absCoords.Z,
                                      Yaw = (float)Yaw,
                                      Pitch = (float)Pitch,
                                      Stance = Client.Stance,
                                      OnGround = false
                                  });
        }

        public void UpdateEntities()
        {
            IEnumerable<EntityBase> nearbyEntities = Server.GetNearbyEntities(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)).ToList();

            foreach (EntityBase e in nearbyEntities)
            {
                if (e.Equals(this))
                    continue;
                if (!LoadedEntities.Contains(e))
                    _Client.SendCreateEntity(e);
                if (Health > 0 && e is ItemEntity && Math.Abs(e.Position.X - Position.X) < 1 && Math.Abs(e.Position.Y - Position.Y) < 1 && Math.Abs(e.Position.Z - Position.Z) < 1)
                    PickupItem((ItemEntity)e);
            }

            foreach (EntityBase e in LoadedEntities.Where(e => !nearbyEntities.Contains(e)))
            {
                _Client.SendDestroyEntity(e);
            }

            LoadedEntities = new List<EntityBase>(nearbyEntities);
        }

        #endregion

        #region Attack & damage

        public override void Attack(LivingEntity target)
        {
            if (target == null)
                return;
            short weaponDmg = GetWeaponDamage();
            target.Damage(DamageCause.EntityAttack, weaponDmg, this);
        }

        /// <summary>
        /// Updates nearby players when Client is hurt.
        /// </summary>
        /// <param name="cause"></param>
        /// <param name="damageAmount"></param>
        /// <param name="hitBy">The Client hurting the current Client.</param>
        /// <param name="args">First argument should always be the damage amount.</param>
        public override void Damage(DamageCause cause, short damageAmount, EntityBase hitBy = null, params object[] args)
        {
            if (GameMode == 1)
                return;
            base.Damage(cause, damageAmount, hitBy, args);
        }

        protected override void SendUpdateOnDamage()
        {
            Client.SendPacket(new UpdateHealthPacket
            {
                Health = Health,
                Food = Food,
                FoodSaturation = FoodSaturation,
            });
            base.SendUpdateOnDamage();
        }

        public short GetWeaponDamage()
        {
            short damage = 1;
            if (Inventory.ActiveItem.Type < 256)
                return damage;
            switch ((BlockData.Items)Inventory.ActiveItem.Type)
            {
                case BlockData.Items.Wooden_Spade:
                case BlockData.Items.Gold_Spade:
                    damage = 1;
                    break;
                case BlockData.Items.Wooden_Pickaxe:
                case BlockData.Items.Gold_Pickaxe:
                case BlockData.Items.Stone_Spade:
                    damage = 2;
                    break;
                case BlockData.Items.Wooden_Axe:
                case BlockData.Items.Gold_Axe:
                case BlockData.Items.Stone_Pickaxe:
                case BlockData.Items.Iron_Spade:
                    damage = 3;
                    break;
                case BlockData.Items.Wooden_Sword:
                case BlockData.Items.Gold_Sword:
                case BlockData.Items.Stone_Axe:
                case BlockData.Items.Iron_Pickaxe:
                case BlockData.Items.Diamond_Spade:
                    damage = 4;
                    break;
                case BlockData.Items.Stone_Sword:
                case BlockData.Items.Iron_Axe:
                case BlockData.Items.Diamond_Pickaxe:
                    damage = 5;
                    break;
                case BlockData.Items.Iron_Sword:
                case BlockData.Items.Diamond_Axe:
                    damage = 6;
                    break;
                case BlockData.Items.Diamond_Sword:
                    damage = 7;
                    break;
            }

            return damage;
        }


        #endregion

        #region Death

        /// <summary>
        /// Handles the death of the Client.
        /// </summary>
        /// <param name="hitBy">Who killed the current Client.</param>
        public override void HandleDeath(EntityBase killedBy = null, string deathBy = "")
        {
            //Event
            ClientDeathEventArgs clientDeath = new ClientDeathEventArgs(Client, deathBy, killedBy);
            Client.Owner.Server.PluginManager.CallEvent(Event.PLAYER_DIED, clientDeath);
            if (clientDeath.EventCanceled) { return; }
            killedBy = clientDeath.KilledBy;
            //End Event


            // TODO: Add config option for none/global/local death messages
            // ...Or maybe make messages a plugin?
            string deathMessage = string.Empty;

            if (killedBy == null && deathBy == "") // Generic message
            {
                deathBy = "mysteriously!";
            }
            else if (killedBy is Player)
            {
                var p = (Player)killedBy;
                deathBy = "by " + p.DisplayName + " using" + Server.Items.ItemName(Inventory.Slots[Inventory.ActiveSlot].Type);
            }
            else if (killedBy is Mob)
            {
                var m = (Mob)killedBy;
                deathBy = "by " + m.Type;
            }

            deathMessage = DisplayName + " was killed " + deathBy;

            SendUpdateOnDeath(deathMessage);

            DoDeath(killedBy);
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            Inventory.DropAll(UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z));
        }

        #endregion

        /// <summary>
        /// Handles the respawning of the Client, called from respawn packet.
        /// </summary>
        public void HandleRespawn()
        {
            // This can no doubt be improved as waiting on the updatechunk thread is quite slow.
            Server.RemoveEntity(this);

            Position = new AbsWorldCoords(
                World.Spawn.WorldX,
                World.Spawn.WorldY + this.EyeHeight,
                World.Spawn.WorldZ);

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

            foreach (Client c in Server.GetNearbyPlayers(item.World, new AbsWorldCoords(item.Position.X, item.Position.Y, item.Position.Z)))
            {
                c.SendPacket(new CollectItemPacket
                {
                    EntityId = item.EntityId,
                    PlayerId = EntityId
                });
            }

            Inventory.AddItem(item.ItemId, item.Count, item.Durability);
        }

        public void SynchronizeEntities()
        {
            AbsWorldCoords absCoords = new AbsWorldCoords(Position.X, Position.Y, Position.Z);
            foreach (EntityBase e in Server.GetNearbyEntities(World, absCoords))
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

        public Chunk GetCurrentChunk()
        {
            Chunk chunk = World.GetChunkFromAbs(Position.X, Position.Z, false, false);

            return chunk;
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
            List<int> nearbyChunks = new List<int>();
            List<int> toUpdate = new List<int>();

            int chunkX = (int)Position.X >> 4;
            int chunkZ = (int)Position.Z >> 4;

            for (int x = chunkX - radius; x <= chunkX + radius; ++x)
            {
                for (int z = chunkZ - radius; z <= chunkZ + radius; ++z)
                {
                    if (token.IsCancellationRequested)
                        return;

                    int packedChunk = UniversalCoords.FromChunkToPackedChunk(x, z);
                    //_Client.Logger.Log(Logger.LogLevel.Info, "Chunk {0} {1} Packed: {2}", x, z, packedChunk);
                    nearbyChunks.Add(packedChunk);

                    if (!LoadedChunks.ContainsKey(packedChunk))
                    {
                        toUpdate.Add(packedChunk);
                        _Client.SendPreChunk(x, z, true, sync);
                    }
                }
            }

            foreach (int c in toUpdate)
            {
                if (token.IsCancellationRequested)
                    return;
                int x = UniversalCoords.FromPackedChunkToX(c);
                int z = UniversalCoords.FromPackedChunkToZ(c);

                Chunk chunk = World.GetChunkFromChunk(x, z, true, true);
                //_Client.Logger.Log(Logger.LogLevel.Info, "Packed {0} Unpacked Chunk {1} {2}", c, x, z);
                chunk.AddClient(_Client);
                LoadedChunks.TryAdd(c, chunk);
                _Client.SendChunk(chunk, sync);

                _Client.SendSignTexts(chunk);
            }

            if (remove)
            {
                foreach (int c in LoadedChunks.Keys.Where(c => !nearbyChunks.Contains(c)))
                {
                    if (token.IsCancellationRequested)
                        return;

                    _Client.SendPreChunk(UniversalCoords.FromPackedChunkToX(c), UniversalCoords.FromPackedChunkToZ(c), false, sync);
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
            ClientJoinedEventArgs e = new ClientJoinedEventArgs(Client);
            Server.PluginManager.CallEvent(Event.PLAYER_JOINED, e);
            //We kick the player because it would not work to use return.
            if (e.EventCanceled)
            {
                _Client.Kick("");
                return; //return here so we do not display message
            }
            DisplayMessage = e.BrodcastMessage;
            //End Event
            Server.Broadcast(DisplayMessage);
        }


        public void SetHealth(short health)
        {
            Health = health;
            _Client.SendPacket(new UpdateHealthPacket
            {
                Health = Health,
                Food = Food,
                FoodSaturation = FoodSaturation,
            });
        }

        public void DropItem()
        {
            Server.DropItem(this, Inventory.Slots[Inventory.ActiveSlot]);
        }


        #region Permission related commands
        //Check if the player has permissions to use the command from a command object
        public bool CanUseCommand(ICommand command)
        {
            return PermHandler.HasPermission(this, command);
        }

        
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
