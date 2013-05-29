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
using System.Linq;
using System.Collections.Concurrent;
using System.Text;
using System.Threading;
using Chraft.Entity.Items;
using Chraft.Interfaces;
using Chraft.Net.Packets;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.PluginSystem.World;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Config;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utilities.Misc;
using Chraft.Utils;
using Chraft.World;
using Chraft.Net;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.Entity
{
    public class Player : LivingEntity, IPlayer
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

        public DateTime LastSaveTime;
        public DateTime EnqueuedForSaving;
        public TimeSpan SaveSpan = TimeSpan.FromSeconds(60.0);
        public int ChangesToSave;

        public int Experience { get; internal set; }

        private Client _client;

        public Client Client
        {
            get { return _client; }
            set { _client = value; }
        }

        public override short Health
        {
            get { return _health; }
            set
            {
                _health = MathExtensions.Clamp(value, (short) 0, MaxHealth);
                CheckFood();
            }
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

        public GameMode GameMode { get; set; }


        protected Timer FoodEffectTimer;
        public float MaxFoodSaturation { get { return MaxFood; } }
        private float _foodSaturation; 
        public float FoodSaturation
        {
            get { return _foodSaturation; }
            set { _foodSaturation = MathExtensions.Clamp(value, 0, MaxFoodSaturation); }
        }

        public short MaxFood { get { return 20; } }
        private short _food;
        public short Food {
            get { return _food; }
            set
            {
                _food = MathExtensions.Clamp(value, (short) 0, MaxFood);
                CheckFood();
            }
        }

        public int MaxExhaustion { get { return 40000; } }
        private int _exhaustion;
        public int Exhaustion
        {
            get { return _exhaustion; }
            set
            {
                _exhaustion = MathExtensions.Clamp(value, 0, MaxExhaustion);
                CheckExhaustion();
            }
        }

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

        public IClient GetClient()
        {
            return _client;
        }

        public IInventory GetInventory()
        {
            return Inventory;
        }

        #region Initialization
        public void InitializePosition()
        {
            World = Server.GetDefaultWorld() as WorldManager;
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
                    Inventory[i] = ItemHelper.Void;
                }
                var item = ItemHelper.GetInstance(278);
                item.Count = 1;
                item.Durability = 0;
                Inventory[Inventory.ActiveSlot] = item;
            }

            Inventory.UpdateClient();
        }

        public void InitializeHealth()
        {
            if (Health <= 0)
                Health = 20;

            if (Food < 0)
                Food = 0;

            FoodSaturation = 5;

            SendUpdateHealthPacket();
            CheckFood();
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
            AddExhaustionOnMovement(absCoords);
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
            AddExhaustionOnMovement(absCoords);
            base.MoveTo(absCoords, yaw, pitch);
            UpdateEntities();
        }

        internal override void OnTeleportTo(AbsWorldCoords absCoords)
        {
            base.OnTeleportTo(absCoords);

            Client.StopUpdateChunks();

            UpdateChunks(1, CancellationToken.None, true, false);

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

            UpdateEntities();
            Server.SendEntityToNearbyPlayers(World, this);
            Client.ScheduleUpdateChunks();
        }

        internal override bool ToSkip(Client c)
        {
            return c.GetOwner().Equals(this);
        }

        internal void UpdateEntities()
        {
            Dictionary<int, IEntityBase> nearbyEntities = Server.GetNearbyEntitiesDict(World, UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z));

            foreach (EntityBase e in nearbyEntities.Values)
            {
                if (e.Equals(this))
                    continue;
                
                if (!LoadedEntities.ContainsKey(e.EntityId))
                {
                    _client.SendCreateEntity(e);
                    LoadedEntities.TryAdd(e.EntityId, e);
                }
                if (Health > 0 && Math.Abs(e.Position.X - Position.X) < 1 && Math.Abs(e.Position.Y - Position.Y) < 1 && Math.Abs(e.Position.Z - Position.Z) < 1)
                    if (e is ItemEntity)
                        PickupItem((ItemEntity)e);
                    else if (e is ExpOrbEntity)
                        PickupExpOrb((ExpOrbEntity)e);                    
            }

            Queue<int> entitiesToRemove = new Queue<int>();
            foreach (EntityBase e in LoadedEntities.Values.Where(e => !nearbyEntities.ContainsKey(e.EntityId)))
            {
                EntityBase unused;
                LoadedEntities.TryRemove(e.EntityId, out unused);
                entitiesToRemove.Enqueue(e.EntityId);
            }

            if(entitiesToRemove.Count > 0)
                _client.SendDestroyEntities(entitiesToRemove.ToArray());
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
            SendMetadataUpdate();
        }

        public void StopSprinting()
        {
            Data.IsSprinting = false;
            SendMetadataUpdate();
        }

        /// <summary>
        /// Inaccurate check whether the player is in water. Should use the bounding box.
        /// </summary>
        /// <returns></returns>
        public bool IsInWater()
        {
            var feetBlock = World.GetBlockId(UniversalCoords.FromAbsWorld(Position));
            var headBlock = World.GetBlockId(UniversalCoords.FromAbsWorld(Position.X, Position.Y + 1, Position.Z));
            if (feetBlock == null || headBlock == null)
                return false;

            if (feetBlock == (byte)BlockData.Blocks.Water || feetBlock == (byte)BlockData.Blocks.Still_Water ||
                feetBlock == (byte)BlockData.Blocks.Lava || feetBlock == (byte)BlockData.Blocks.Still_Lava ||
                headBlock == (byte)BlockData.Blocks.Water || headBlock == (byte)BlockData.Blocks.Still_Water ||
                headBlock == (byte)BlockData.Blocks.Lava || headBlock == (byte)BlockData.Blocks.Still_Lava)
                return true;
            return false;
        }
        #endregion

        #region Attack & damage

        public override void Attack(ILivingEntity target)
        {
            if (target == null)
                return;
            short weaponDmg = Inventory.ActiveItem.GetDamage();

            //Start Event
            EntityAttackEventArgs e = new EntityAttackEventArgs(this, weaponDmg, target);
            Server.PluginManager.CallEvent(Event.EntityAttack, e);
            if(e.EventCanceled) return;
            target = (LivingEntity)e.EntityToAttack;
            weaponDmg = e.Damage;
            //End Event

            Exhaustion += 300;
            
            target.Damage(DamageCause.EntityAttack, weaponDmg, this);
        }

        /// <summary>
        /// Updates nearby players when Client is hurt.
        /// </summary>
        /// <param name="cause"></param>
        /// <param name="damageAmount"></param>
        /// <param name="hitBy">The Client hurting the current Client.</param>
        /// <param name="args">First argument should always be the damage amount.</param>
        public override void Damage(DamageCause cause, short damageAmount, IEntityBase hitBy = null, params object[] args)
        {
            if (GameMode == GameMode.Creative)
                return;
            // Armor can't reduce suffocation, fire burn, drowning, starving, magic, generic and falling-in-void damage
            if (cause != DamageCause.Suffocation && cause != DamageCause.Drowning && cause != DamageCause.FireBurn && cause != DamageCause.Void)
                damageAmount = ApplyArmorReduction(damageAmount);
            Exhaustion += 300;
            base.Damage(cause, damageAmount, hitBy, args);
        }

        protected override void SendUpdateOnDamage()
        {
            SendUpdateHealthPacket();
            base.SendUpdateOnDamage();
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
            for (short i = 5; i < 9; i++)
            {
                if (Inventory[i] != null && !ItemHelper.IsVoid(Inventory[i]))
                {
                    if (Inventory[i].Type == (short)BlockData.Blocks.Pumpkin)
                        continue;

                    Inventory[i].DamageItem(Math.Abs(damage));
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
            if (Inventory[5] != null && !ItemHelper.IsVoid(Inventory[5]))
            {
                // We can wear a pumpkin, but it'll not give us any armor
                if (Inventory[5].Type != (short)BlockData.Blocks.Pumpkin)
                {
                    baseArmorPoints += 3;
                    totalCurrentDurability += (short)(BlockData.ToolDuarability[(BlockData.Items)Inventory[5].Type] - Inventory[5].Durability);
                    totalBaseDurability += BlockData.ToolDuarability[(BlockData.Items)Inventory[5].Type];
                }
            }
            // Chest
            if (Inventory[6] != null && !ItemHelper.IsVoid(Inventory[6]))
            {
                baseArmorPoints += 8;
                totalCurrentDurability += (short)(BlockData.ToolDuarability[(BlockData.Items)Inventory[6].Type] - Inventory[6].Durability);
                totalBaseDurability += BlockData.ToolDuarability[(BlockData.Items)Inventory[6].Type];
            }
            // Pants
            if (Inventory[7] != null && !ItemHelper.IsVoid(Inventory[7]))
            {
                baseArmorPoints += 6;
                totalCurrentDurability += (short)(BlockData.ToolDuarability[(BlockData.Items)Inventory[7].Type] - Inventory[7].Durability);
                totalBaseDurability += BlockData.ToolDuarability[(BlockData.Items)Inventory[7].Type];
            }
            // Boots
            if (Inventory[8] != null && !ItemHelper.IsVoid(Inventory[8]))
            {
                baseArmorPoints += 3;
                totalCurrentDurability += (short)(BlockData.ToolDuarability[(BlockData.Items)Inventory[8].Type] - Inventory[8].Durability);
                totalBaseDurability += BlockData.ToolDuarability[(BlockData.Items)Inventory[8].Type];
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
        internal override void HandleDeath(EntityBase killedBy = null, string deathBy = "")
        {
            //Event
            ClientDeathEventArgs clientDeath = new ClientDeathEventArgs(Client, deathBy, killedBy);
            Client.Owner.Server.PluginManager.CallEvent(Event.PlayerDied, clientDeath);
            if (clientDeath.EventCanceled) { return; }
            killedBy = clientDeath.KilledBy as EntityBase;
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
                deathBy = "by " + p.DisplayName + " using" + Server.Items.ItemName(Inventory[Inventory.ActiveSlot].Type);
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
            Inventory.DropAll(UniversalCoords.FromAbsWorld(Position));
            StopFireBurnTimer();
            StopSuffocationTimer();
            StopDrowningTimer();
            DropExperienceOrbs();
        }

        protected void DropExperienceOrbs()
        {
            var exp = (short)Math.Min(Experience, short.MaxValue);
            var level = Utilities.Misc.Experience.GetLevel(exp);
            if (level < 1)
                return;
            var expToDrop = (short)Math.Min(level * 7, 100);
            var orb = new ExpOrbEntity(Server, Server.AllocateEntity(), expToDrop);
            orb.Position = Position;
            Server.AddEntity(orb);
        }

        #endregion

        /// <summary>
        /// Handles the respawning of the Client, called from respawn packet.
        /// </summary>
        internal void HandleRespawn()
        {
            Server.RemoveEntity(this);

            Position = new AbsWorldCoords(
                World.Spawn.WorldX,
                World.Spawn.WorldY + this.EyeHeight,
                World.Spawn.WorldZ);

            _client.StopUpdateChunks();
            UpdateChunks(1, CancellationToken.None, false);
            _client.SendPacket(new RespawnPacket { LevelType = ChraftConfig.LevelType, WorldHeight = 256, GameMode = (sbyte)_client.GetOwner().GameMode, });
            UpdateEntities();
            //SendSpawnPosition();
            _client.SendInitialPosition();
            _client.SendInitialTime();
            InitializeInventory();
            InitializeHealth();
            SendUpdateExperience();
            _client.ScheduleUpdateChunks();

            Server.AddEntity(this);
        }

        public void SendUpdateExperience()
        {
            var exp = (short)(Experience > short.MaxValue ? short.MaxValue : Experience);
            var level = Utilities.Misc.Experience.GetLevel(exp);
            var nextLevelExp = Utilities.Misc.Experience.ExpToNextLevel(level);
            var thisLevelExp = Utilities.Misc.Experience.GetExperience(level);
            float expOnBar = (exp - thisLevelExp)/(float)nextLevelExp;
            
            Client.SendPacket(new ExperiencePacket
            {
                Experience = expOnBar,
                Level = level,
                TotExperience = exp,
            });
        }

        public void AddExperience(short amount)
        {
            long newExp = Experience + amount;
            if (newExp < 0)
                newExp = 0;
            if (newExp > Int32.MaxValue)
                newExp = Int32.MaxValue;
            Experience = (int)newExp;
            SendUpdateExperience();
        }

        private void PickupExpOrb(ExpOrbEntity orb)
        {
            if (Server.GetEntityById(orb.EntityId) == null)
                return;

            Server.SendPacketToNearbyPlayers(orb.World, UniversalCoords.FromAbsWorld(orb.Position), new CollectItemPacket
            {
                EntityId = orb.EntityId,
                PlayerId = EntityId
            });

            Server.RemoveEntity(orb);

            AddExperience(orb.Experience);
        }

        private void PickupItem(ItemEntity item)
        {
            if (Server.GetEntityById(item.EntityId) == null)
                return;

            Server.SendPacketToNearbyPlayers(item.World, UniversalCoords.FromAbsWorld(item.Position), new CollectItemPacket
            {
                EntityId = item.EntityId,
                PlayerId = EntityId
            });

            Server.RemoveEntity(item);

            Inventory.AddItem(item.ItemId, item.Count, item.Durability);
        }

        public void SynchronizeEntities()
        {
            UniversalCoords coords = UniversalCoords.FromAbsWorld(Position);
            foreach (EntityBase e in Server.GetNearbyEntitiesInternal(World, coords))
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

        public IChunk GetCurrentChunk()
        {
            Chunk chunk = World.GetChunkFromAbs(Position.X, Position.Z) as Chunk;

            return chunk;
        }

        internal void UpdateChunks(int radius, CancellationToken token)
        {
            UpdateChunks(radius, token, false, true);
        }

        internal void UpdateChunks(int radius, bool sync, CancellationToken token)
        {
            UpdateChunks(radius, token, sync, true);
        }

        internal void UpdateChunks(int radius, CancellationToken token, bool remove)
        {
            UpdateChunks(radius, token, false, remove);
        }
 
        public void UpdateChunks(int radius, CancellationToken token, bool sync, bool remove)
        {
            int chunkX = (int)(Math.Floor(Position.X)) >> 4;
            int chunkZ = (int)(Math.Floor(Position.Z)) >> 4;
            
            Dictionary<int, int> nearbyChunks = new Dictionary<int, int>();

            MapChunkBulkPacket chunkPacket = null;
            if(sync)
                chunkPacket = new MapChunkBulkPacket();

            for (int x = chunkX - radius; x <= chunkX + radius; ++x)
            {
                for (int z = chunkZ - radius; z <= chunkZ + radius; ++z)
                {
                    if (token.IsCancellationRequested)
                        return;

                    int packedChunk = UniversalCoords.FromChunkToPackedChunk(x, z);

                    nearbyChunks.Add(packedChunk, packedChunk);

                    if (!LoadedChunks.ContainsKey(packedChunk))
                    {
                        Chunk chunk;
                        if (sync)
                            chunk = World.GetChunkFromChunkSync(x, z, true, true) as Chunk;
                        else
                            chunk = World.GetChunkFromChunkAsync(x, z, Client, true, true) as Chunk;

                        LoadedChunks.TryAdd(packedChunk, chunk);

                        if (chunk == null)
                            continue;

                        if (chunk.LightToRecalculate)
                        {
#if PROFILE
                            Stopwatch watch = new Stopwatch();
                            watch.Start();

                            chunk.RecalculateSky();

                            watch.Stop();

                            World.Logger.Log(LogLevel.Info, "Skylight recalc: {0}", watch.ElapsedMilliseconds);
#else
                            chunk.RecalculateSky();
#endif
                        }

                        chunk.AddClient(Client);
                        if(!sync)
                            _client.SendChunk(chunk);
                        else
                            chunkPacket.ChunksToSend.Add(chunk);
                    }
                }
            }

            if(sync)
                _client.Send_Sync_Packet(chunkPacket);        

            if (remove)
            {
                foreach (int c in LoadedChunks.Keys.Where(c => !nearbyChunks.ContainsKey(c)))
                {
                    if (token.IsCancellationRequested)
                        return;
                    Chunk chunk;
                    LoadedChunks.TryRemove(c, out chunk);


                    if (chunk != null)
                        chunk.RemoveClient(_client);
                    
                }
            }

        }

        public void MarkToSave()
        {
            int changes = Interlocked.Increment(ref ChangesToSave);

            if(changes == 1)
            {
                EnqueuedForSaving = DateTime.Now;
                if ((DateTime.Now - LastSaveTime) > SaveSpan)
                    Server.PlayersToSave.Enqueue(Client);
                else
                    Server.PlayersToSavePostponed.Enqueue(Client);   
            }
        }

        internal void OnJoined()
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

            if(!string.IsNullOrEmpty(ChraftConfig.ServerTextureUrl))
            {
                string message = ChraftConfig.ServerTextureUrl + '\0' + 16;
                _client.SendPacket(new PluginMessagePacket
                    {
                        Channel = "MC|TPack",
                        Message = Encoding.UTF8.GetBytes(message),
                    });
            }
        }

        protected void AddExhaustionOnMovement(AbsWorldCoords coords)
        {
            var dx = coords.X - Position.X;
            var dy = coords.Y - Position.Y;
            var dz = coords.Z - Position.Z;

            if (IsInWater())
            {
                var distance = (short)Math.Round(MathHelper.sqrt_double(dx * dx + dz * dz) * 10);
                Exhaustion += 15 * distance;
            }
            else if (Client.OnGround)
            {
                var distance = (short)Math.Round(MathHelper.sqrt_double(dx*dx + dy*dy + dz*dz) * 10);
                if (Data.IsSprinting)
                    Exhaustion += 100 * distance;
                else
                    Exhaustion += 10 * distance;
            }
        }

        public bool IsHungry()
        {
            return Food < 20;
        }

        public bool EatFood(short food, float saturation)
        {
            if (!IsHungry())
                return false;
            Food += food;
            FoodSaturation = Math.Min(FoodSaturation + saturation, Food);
            SendUpdateHealthPacket();
            return true;
        }

        protected void CheckExhaustion()
        {
            if (Exhaustion < 4000)
                return;

            Exhaustion -= 4000;
            if (FoodSaturation > 0)
                FoodSaturation -= 1.0f;
            else
            {
                Food -= 1;
                SendUpdateHealthPacket();
            }
        }

        protected void CheckFood()
        {
            if (IsDead || (Food > 0 && Food < 18))
            {
                if (FoodEffectTimer != null)
                {
                    StopStarvingDamageTimer();
                }
                return;
            }
            if (Food == 0 || (Food >= 18 && Health < MaxHealth))
            {
                if (FoodEffectTimer == null)
                {
                    FoodEffectTimer = new Timer(FoodEffect, null, 4000, 4000);
                }
            }
        }

        protected void FoodEffect(object state)
        {
            if (IsDead || (Food > 0 && Food < 18) || (Food >= 18 && Health == MaxHealth))
            {
                StopStarvingDamageTimer();
                return;
            }

            if (Food >= 18 && Health < MaxHealth)
            {
                AddHealth(1);
            }
            else if (Food == 0)
                Damage(DamageCause.Starve, 1);
        }

        protected void StopStarvingDamageTimer()
        {
            if (FoodEffectTimer != null)
            {
                FoodEffectTimer.Change(Timeout.Infinite, Timeout.Infinite);
                FoodEffectTimer.Dispose();
                FoodEffectTimer = null;
            }
        }

        public void SetHealth(short health)
        {
            Health = health;
            SendUpdateHealthPacket();
        }

        public void AddHealth(short health)
        {
            Health += health;
            SendUpdateHealthPacket();
        }

        protected void SendUpdateHealthPacket()
        {
            _client.SendPacket(new UpdateHealthPacket
            {
                Health = Health,
                Food = Food,
                FoodSaturation = FoodSaturation,
            });
        }

        public void DropActiveSlotItem()
        {

            var activeItemStack = Inventory[Inventory.ActiveSlot];
            if (activeItemStack.Count > 0)
            {
                if (activeItemStack.Count == 1)
                {
                    Server.DropItem(this, activeItemStack);
                }
                else
                {
                    var item = ItemHelper.GetInstance(activeItemStack.Type);
                    item.Durability = activeItemStack.Durability;
                    item.Damage = activeItemStack.Damage;
                    item.Count = 1;
                    Server.DropItem(this, item);
                }
                Inventory.RemoveItem(Inventory.ActiveSlot);
            }
        }

        public void FinishUseActiveSlotItem()
        {
            if (ItemHelper.IsVoid(Inventory.ActiveItem))
                return;

            if (Inventory.ActiveItem is IItemConsumable)
            {
                var consumable = Inventory.ActiveItem as IItemConsumable;
                consumable.FinishConsuming();
            }
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

        public IBans GetBan()
        {
            return GetServer().GetBanSystem().GetBan(Name);
        }
    }
}