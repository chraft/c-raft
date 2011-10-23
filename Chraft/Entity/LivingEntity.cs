using System;
using System.Threading;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Plugins.Events;
using Chraft.Plugins.Events.Args;
using Chraft.Utils;
using Chraft.World;
using Chraft.World.Blocks;

namespace Chraft.Entity
{
    public abstract class LivingEntity : EntityBase
    {

        public abstract string Name { get; }

        short _health;
        
        public MetaData Data { get; internal set; }
        
        /// <summary>
        /// Current entity Health represented as "halves of a heart", e.g. Health == 9 is 4.5 hearts. This value is clamped between 0 and EntityBase.MaxHealth.
        /// </summary>
        public virtual short Health
        {
            get { return _health; }
            set { _health = MathExtensions.Clamp(value, (short)0, this.MaxHealth); }
        }
        
        /// <summary>
        /// MaxHealth for this entity represented as "halves of a heart".
        /// </summary>
        public virtual short MaxHealth { get { return 20; } }
        
        public virtual float EyeHeight
        {
            get { return this.Height * 0.85f; }
        }

        protected Timer SuffocationTimer;
        protected Timer DrowningTimer;

        protected int TicksDrowning;
        protected int TicksSuffocating;

        public bool CanDrown { get; protected set; }

        public bool CanSuffocate { get; protected set; }

        public bool IsDead { get; protected set; }

        public virtual bool IsEntityAlive { get { return !IsDead && Health > 0; } }
                                                            
        protected bool IsJumping { get; set; }
    
        public override bool Collidable
        {
            get
            {
                return !IsDead;
            }
        }
        
        public override bool Pushable
        {
            get
            {
                return !IsDead;
            }
        }
    
        public LivingEntity(Server server, int entityId)
         : base(server, entityId)
        {
            this.Health = MaxHealth;
            CanDrown = true;
            CanSuffocate = true;
        }
        
        /// <summary>
        /// Determines whether this instance can see the specified entity.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance can see the specified entity; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='entity'>
        /// The entity to check for line of sight to.
        /// </param>
        public bool CanSee(LivingEntity entity)
        {
            return this.World.RayTraceBlocks(new AbsWorldCoords(this.Position.X, this.Position.Y + this.EyeHeight, this.Position.Z), new AbsWorldCoords(entity.Position.X, entity.Position.Y + entity.EyeHeight, entity.Position.Z)) == null;
        }
  
        /// <summary>
        /// Jump this instance.
        /// </summary>
        protected virtual void Jump()
        {
            this.Velocity.Y = 0.42;
        } 
      
        public string FacingDirection(byte compassPoints)
        {

            byte rotation = (byte)((Yaw * 256.0 / 360.0) % 256.0); // Gives rotation as 0 - 255, 0 being due E.

            if (compassPoints == 8)
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

        #region Drowning/suffocation
        public virtual void CheckDrowning()
        {
            if (!CanDrown || IsDead)
                return;
            byte headBlockId = World.GetBlockId(UniversalCoords.FromAbsWorld(Position.X, Position.Y + Height, Position.Z));
            if (BlockHelper.Instance(headBlockId).IsLiquid)
            {
                if (DrowningTimer == null)
                {
                    DrowningTimer = new Timer(Drown, null, 50, 50);
                }
            }
        }

        protected virtual void Drown(object state)
        {
            byte headBlockId = World.GetBlockId(UniversalCoords.FromAbsWorld(Position.X, Position.Y + Height, Position.Z));
            if (IsDead || !BlockHelper.Instance(headBlockId).IsLiquid || !CanDrown)
            {
                StopDrowningTimer();
                return;
            }

            if (TicksDrowning >= 200 && TicksDrowning % 20 == 0) // 10+ Seconds underwater
            {
                Damage(DamageCause.Drowning, 2);
            }
            TicksDrowning++;
        }

        protected void StopDrowningTimer()
        {
            if (DrowningTimer != null)
            {
                DrowningTimer.Change(Timeout.Infinite, Timeout.Infinite);
                DrowningTimer = null;
            }
            TicksDrowning = 0;
        }

        public virtual void CheckSuffocation()
        {
            if (!CanSuffocate || IsDead)
                return;
            byte headBlockId = World.GetBlockId(UniversalCoords.FromAbsWorld(Position.X, Position.Y + EyeHeight, Position.Z));
            if (BlockHelper.Instance(headBlockId).IsOpaque)
            {
                if (SuffocationTimer == null)
                {
                    SuffocationTimer = new Timer(Suffocate, null, 0, 50);
                }
            }
        }

        protected virtual void Suffocate(object state)
        {
            byte headBlockId = World.GetBlockId(UniversalCoords.FromAbsWorld(Position.X, Position.Y + EyeHeight, Position.Z));
            if (IsDead || !BlockHelper.Instance(headBlockId).IsOpaque || !CanSuffocate)
            {
                StopSuffocationTimer();
                return;
            }

            if (TicksSuffocating % 10 == 0)
            {
                Damage(DamageCause.Suffocation, 1);
            }
            TicksSuffocating++;
        }

        protected void StopSuffocationTimer()
        {
            if (SuffocationTimer != null)
            {
                SuffocationTimer.Change(Timeout.Infinite, Timeout.Infinite);
                SuffocationTimer = null;
            }
            TicksSuffocating = 0;
        }
        #endregion

        #region Movement
        public override void OnMoveTo(sbyte x, sbyte y, sbyte z)
        {
            base.OnMoveTo(x, y, z);
            CheckDrowning();
            CheckSuffocation();
        }

        public override void OnMoveRotateTo(sbyte x, sbyte y, sbyte z)
        {
            base.OnMoveRotateTo(x, y, z);
            CheckDrowning();
            CheckSuffocation();
        }

        public override void OnTeleportTo(AbsWorldCoords absCoords)
        {
            base.OnTeleportTo(absCoords);
            CheckDrowning();
            CheckSuffocation();
        }


        #endregion

        #region Attack and damage

        public abstract void Attack(LivingEntity target);

        public virtual void Damage(DamageCause cause, short damageAmount, EntityBase hitBy = null, params object[] args)
        {
            var hitByPlayer = hitBy as Player;

            EntityDamageEventArgs e = new EntityDamageEventArgs(this, damageAmount, hitBy, cause);
            Server.PluginManager.CallEvent(Event.ENTITY_DAMAGE, e);
            if (e.EventCanceled) return;
            damageAmount = e.Damage;
            hitBy = e.DamagedBy;

            // Debug
            if (hitByPlayer != null)
            {
                ItemStack itemHeld = hitByPlayer.Inventory.ActiveItem;
                hitByPlayer.Client.SendMessage("You hit a " + Name + " with a " + itemHeld.Type + " dealing " + damageAmount + " damage.");
            }
            
            Health -= damageAmount;


            SendUpdateOnDamage();

            // TODO: Entity Knockback

            if (Health <= 0)
                HandleDeath(hitBy);
        }

        protected virtual void SendUpdateOnDamage()
        {
            foreach (Client c in World.Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                if (c.Owner == this)
                    continue;

                c.SendPacket(new AnimationPacket // Hurt Animation
                {
                    Animation = 2,
                    PlayerId = this.EntityId
                });

                c.SendPacket(new EntityStatusPacket // Hurt Action
                {
                    EntityId = this.EntityId,
                    EntityStatus = 2
                });
            }
        }

        #endregion

        #region Death

        public virtual void HandleDeath(EntityBase killedBy = null, string deathBy = "")
        {
            //Event
            EntityDeathEventArgs e = new EntityDeathEventArgs(this, killedBy);
            Server.PluginManager.CallEvent(Event.ENTITY_DEATH, e);
            if (e.EventCanceled) return;
            killedBy = e.KilledBy;
            //End Event

            // TODO: Stats/achievements handled in each mob class??? (within DoDeath)
            //if (hitBy != null)
            //{
            //    // TODO: Stats/Achievement hook or something
            //}

            SendUpdateOnDeath();

            // Spawn goodies / perform achievements etc..
            DoDeath(killedBy);
        }

        protected virtual void SendUpdateOnDeath(string deathMessage = "")
        {
            foreach (Client c in Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                if (!string.IsNullOrEmpty(deathMessage))
                    c.SendMessage(deathMessage);

                if (c.Owner == this)
                    continue;

                c.SendPacket(new EntityStatusPacket // Death Action
                {
                    EntityId = EntityId,
                    EntityStatus = 3
                });
            }
        }

        /// <summary>
        /// Perform any item drop logic during death
        /// </summary>
        protected virtual void DoDeath(EntityBase killedBy)
        {
            System.Timers.Timer removeTimer = new System.Timers.Timer(1000);

            removeTimer.Elapsed += delegate
            {
                removeTimer.Stop();
                World.Server.SendRemoveEntityToNearbyPlayers(World, this);
                World.Server.RemoveEntity(this);
                removeTimer.Dispose();
            };

            removeTimer.Start();
        }

        #endregion
        
        /// <summary>
        /// Faces the entity.
        /// </summary>
        /// <param name='entity'>
        /// Entity to face.
        /// </param>
        /// <param name='yawSpeed'>
        /// Yaw speed.
        /// </param>
        /// <param name='pitchSpeed'>
        /// Pitch speed.
        /// </param>
        public void FaceEntity(EntityBase entity, float yawSpeed, float pitchSpeed)
        {
            double xDistance = entity.Position.X - this.Position.X;
            double zDistance = entity.Position.Z - this.Position.Z;
            double yDistance;
            if (entity is LivingEntity)
            {
                LivingEntity livingEntity = entity as LivingEntity;
                yDistance = (this.Position.Y + (double)this.EyeHeight) - (livingEntity.Position.Y + (double)livingEntity.EyeHeight);
            }
            else
            {
                yDistance = (entity.BoundingBox.Minimum.Y + entity.BoundingBox.Maximum.Y) / 2.0 - (this.Position.Y + this.EyeHeight);
            }
            
            double xzDistance = Math.Sqrt(xDistance * xDistance + zDistance * zDistance);
            double destinationYaw = ((Math.Atan2(zDistance, xDistance) * 180.0) / Math.PI) - 90f;
            double destinationPitch = -((Math.Atan2(yDistance, xzDistance) * 180) / Math.PI);
            this.Pitch = -UpdateRotation(this.Pitch, destinationPitch, pitchSpeed);
            this.Yaw = UpdateRotation(this.Yaw, destinationYaw, yawSpeed);
        }
        
        private double UpdateRotation(double currentRotation, double destinationRotation, double rotationSpeed)
        {
            double rotationAmount;
            // Clamp to within -180 to +180
            for (rotationAmount = destinationRotation - currentRotation; rotationAmount < -180.0; rotationAmount += 360.0) { }
            for (; rotationAmount >= 180.0; rotationAmount -= 360.0) { }
            if (rotationAmount > rotationSpeed)
            {
                rotationAmount = rotationSpeed;
            }
            if (rotationAmount < -rotationSpeed)
            {
                rotationAmount = -rotationSpeed;
            }
            return currentRotation + rotationAmount;
        }
    }
}

