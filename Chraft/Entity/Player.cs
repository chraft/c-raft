using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.Concurrent;
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
        private ConcurrentDictionary<int, EntityBase> LoadedEntities = new ConcurrentDictionary<int, EntityBase>();
        public volatile bool LoggedIn = false;
        public PermissionHandler PermHandler;
        public ClientPermission Permissions;
        public Interface CurrentInterface = null;
        public AbsWorldCoords LoginPosition;

        private Client _client;

        public Client Client
        {
            get { return _client; }
            set { _client = value; }
        }

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

        public float FoodSaturation { get; set; }
        public short Food { get; set; }

        protected short _lastDamageRemainder = 0;

        public Player(Server server, int entityId, Client client)
            : base(server, entityId, null)
        {
            _client = client;
            EnsureServer(server);
            Inventory = null;
            DisplayName = client.Username;
            InitializePosition();
            PermHandler = new PermissionHandler(server);
        }

        #region Initialization
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

            _client.SendPacket(new UpdateHealthPacket
            {
                Health = Health,
                Food = Food,
                FoodSaturation = FoodSaturation,
            });
        }
        #endregion

        #region Fire/burning damage
        public override void TouchedFire()
        {
            if (!LoggedIn)
            {
                StopFireBurnTimer();
                return;
            }
            base.TouchedFire();
        }

        public override void TouchedLava()
        {
            if (!LoggedIn)
            {
                StopFireBurnTimer();
                return;
            }
            base.TouchedLava();
        }

        protected override void FireBurn(object state)
        {
            if (!LoggedIn)

            {
                StopFireBurnTimer();
                return;
            }
            base.FireBurn(state);
        }
        #endregion
        #region Suffocation/drowning
        protected override void Suffocate(object state)
        {
            if (!LoggedIn)
            {
                StopSuffocationTimer();
                return;
            }
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

        public override bool ToSkip(Client c)
        {
            return c.Owner.Equals(this);
        }

        public void UpdateEntities()
        {
            Dictionary<int, EntityBase> nearbyEntities = Server.GetNearbyEntitiesDict(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z));

            foreach (EntityBase e in nearbyEntities.Values)
            {
                if (e.Equals(this))
                    continue;
                
                if (!LoadedEntities.ContainsKey(e.EntityId))
                {
                    _client.SendCreateEntity(e);
                    LoadedEntities.TryAdd(e.EntityId, e);
                }
                if (Health > 0 && e is ItemEntity && Math.Abs(e.Position.X - Position.X) < 1 && Math.Abs(e.Position.Y - Position.Y) < 1 && Math.Abs(e.Position.Z - Position.Z) < 1)
                    PickupItem((ItemEntity)e);
            }

            foreach (EntityBase e in LoadedEntities.Values.Where(e => !nearbyEntities.ContainsKey(e.EntityId)))
            {
                EntityBase unused;
                LoadedEntities.TryRemove(e.EntityId, out unused);
                _client.SendDestroyEntity(e);
            }
        }

        public void StartCrouching()
        {
            Data.IsCrouched = true;
            SendMetadataUpdate(false);
        }

        public void StopCrouching()
        {
            Data.IsCrouched = false;
            SendMetadataUpdate(false);
        }

        public void StartSprinting()
        {
            Data.IsSprinting = true;
            SendMetadataUpdate(false);
        }

        public void StopSprinting()
        {
            Data.IsSprinting = false;
            SendMetadataUpdate(false);
        }
        #endregion

        #region Attack & damage

        public override void Attack(LivingEntity target)
        {
            if (target == null)
                return;
            short weaponDmg = GetWeaponDamage();

            //Start Event
            EntityAttackEventArgs e = new EntityAttackEventArgs(this, weaponDmg, target);
            Server.PluginManager.CallEvent(Event.EntityAttack, e);
            if(e.EventCanceled) return;
            target = (LivingEntity)e.EntityToAttack;
            weaponDmg = e.Damage;
            //End Event

            
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
            // Armor can't reduce suffocation, fire burn, drowning, starving, magic, generic and falling-in-void damage
            if (cause != DamageCause.Suffocation && cause != DamageCause.Drowning && cause != DamageCause.FireBurn && cause != DamageCause.Void)
                damageAmount = ApplyArmorReduction(damageAmount);
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

        protected short ApplyArmorReduction(short initialDamage)
        {
            short armorPoints = GetArmor();
            int j = 25 - armorPoints;
            int k = initialDamage * j + _lastDamageRemainder;
            DamageArmor(initialDamage);
            short reducedDamage = (short)(k / 25);
            _lastDamageRemainder = (short)(k % 25);
            return reducedDamage;
        }

        public void DamageArmor(short damage)
        {
            for (int i = 5; i < 9; i++)
            {
                if (!ItemStack.IsVoid(Inventory.Slots[i]))
                {
                    if (Inventory.Slots[i].Type == (short)BlockData.Blocks.Pumpkin)
                        continue;

                    Inventory.DamageItem((short)i, Math.Abs(damage));
                }
            }
        }

        protected short GetArmor()
        {
            short baseArmorPoints = 0;
            short totalCurrentDurability = 0;
            short totalBaseDurability = 0;
            short effectiveArmor = 0;

            // Helmet
            if (!ItemStack.IsVoid(Inventory.Slots[5]))
            {
                // We can wear a pumpkin, but it'll not give us any armor
                if (Inventory.Slots[5].Type != (short)BlockData.Blocks.Pumpkin)
                {
                    baseArmorPoints += 3;
                    totalCurrentDurability += (short)(BlockData.ToolDuarability[(BlockData.Items)Inventory.Slots[5].Type] - Inventory.Slots[5].Durability);
                    totalBaseDurability += BlockData.ToolDuarability[(BlockData.Items)Inventory.Slots[5].Type];
                }
            }
            // Chest
            if (!ItemStack.IsVoid(Inventory.Slots[6]))
            {
                baseArmorPoints += 8;
                totalCurrentDurability += (short)(BlockData.ToolDuarability[(BlockData.Items)Inventory.Slots[6].Type] - Inventory.Slots[6].Durability);
                totalBaseDurability += BlockData.ToolDuarability[(BlockData.Items)Inventory.Slots[6].Type];
            }
            // Pants
            if (!ItemStack.IsVoid(Inventory.Slots[7]))
            {
                baseArmorPoints += 6;
                totalCurrentDurability += (short)(BlockData.ToolDuarability[(BlockData.Items)Inventory.Slots[7].Type] - Inventory.Slots[7].Durability);
                totalBaseDurability += BlockData.ToolDuarability[(BlockData.Items)Inventory.Slots[7].Type];
            }
            // Boots
            if (!ItemStack.IsVoid(Inventory.Slots[8]))
            {
                baseArmorPoints += 3;
                totalCurrentDurability += (short)(BlockData.ToolDuarability[(BlockData.Items)Inventory.Slots[8].Type] - Inventory.Slots[8].Durability);
                totalBaseDurability += BlockData.ToolDuarability[(BlockData.Items)Inventory.Slots[8].Type];
            }
            if (totalBaseDurability > 0)
                effectiveArmor = (short)Math.Floor(baseArmorPoints*((double) totalCurrentDurability/totalBaseDurability));

            return effectiveArmor;
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
            Client.Owner.Server.PluginManager.CallEvent(Event.PlayerDied, clientDeath);
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
                var m = (Mob) killedBy;
                deathBy = "by " + m.Type;
            }

            deathMessage = DisplayName + " was killed " + deathBy;

            SendUpdateOnDeath(deathMessage);

            DoDeath(killedBy);
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            Inventory.DropAll(UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z));
            StopFireBurnTimer();
            StopSuffocationTimer();
            StopDrowningTimer();
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

            _client.StopUpdateChunks();
            UpdateChunks(1, CancellationToken.None, false);
            _client.SendPacket(new RespawnPacket { });
            UpdateEntities();
            //SendSpawnPosition();
            _client.SendInitialPosition();
            _client.SendInitialTime();
            InitializeInventory();
            InitializeHealth();
            _client.ScheduleUpdateChunks();

            Server.AddEntity(this);
        }

        private void PickupItem(ItemEntity item)
        {
            if (Server.GetEntityById(item.EntityId) == null)
                return;

            Server.RemoveEntity(item);

            Server.SendPacketToNearbyPlayers(item.World, UniversalCoords.FromAbsWorld(item.Position.X, item.Position.Y, item.Position.Z), new CollectItemPacket
            {
                EntityId = item.EntityId,
                PlayerId = EntityId
            });

            Inventory.AddItem(item.ItemId, item.Count, item.Durability);
        }

        public void SynchronizeEntities()
        {
            AbsWorldCoords absCoords = new AbsWorldCoords(Position.X, Position.Y, Position.Z);
            foreach (EntityBase e in Server.GetNearbyEntities(World, absCoords))
            {
                if (e.Equals(this))
                    continue;

                _client.SendPacket(new EntityTeleportPacket
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
            int chunkX = (int)(Math.Floor(Position.X)) >> 4;
            int chunkZ = (int)(Math.Floor(Position.Z)) >> 4;
            
            Dictionary<int, int> nearbyChunks = new Dictionary<int, int>();
            
            for (int x = chunkX - radius; x <= chunkX + radius; ++x)
            {
                for (int z = chunkZ - radius; z <= chunkZ + radius; ++z)
                {
                    if (token.IsCancellationRequested)
                        return;

                    int packedChunk = UniversalCoords.FromChunkToPackedChunk(x, z);
                    //_Client.Logger.Log(Logger.LogLevel.Info, "Chunk {0} {1} Packed: {2}", x, z, packedChunk);
                    nearbyChunks.Add(packedChunk, packedChunk);

                    if (!LoadedChunks.ContainsKey(packedChunk))
                    {
                        Chunk chunk = World.GetChunkFromChunk(x, z, true, true);
                        LoadedChunks.TryAdd(packedChunk, chunk);
                        chunk.AddClient(Client);
                        _client.SendPreChunk(x, z, true, sync);
                        _client.SendChunk(chunk, sync);
                    }
                }
            }
            

            if (remove)
            {
                foreach (int c in LoadedChunks.Keys.Where(c => !nearbyChunks.ContainsKey(c)))
                {
                    if (token.IsCancellationRequested)
                        return;

                    _client.SendPreChunk(UniversalCoords.FromPackedChunkToX(c), UniversalCoords.FromPackedChunkToZ(c), false, sync);
                    Chunk chunk;
                    LoadedChunks.TryRemove(c, out chunk);
                    chunk.RemoveClient(_client);
                }
            }

        }

        public void OnJoined()
        {
            LoggedIn = true;
            string DisplayMessage = DisplayName + " has logged in";
            //Event
            ClientJoinedEventArgs e = new ClientJoinedEventArgs(Client);
            Server.PluginManager.CallEvent(Event.PlayerJoined, e);
            //We kick the player because it would not work to use return.
            if (e.EventCanceled)
            {
                _client.Kick("");
                return; //return here so we do not display message
            }
            DisplayMessage = e.BrodcastMessage;
            //End Event
            Server.Broadcast(DisplayMessage);
        }


        public void SetHealth(short health)
        {
            Health = health;
            _client.SendPacket(new UpdateHealthPacket
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
            return PermHandler.HasPermission(_client.Username, command);
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