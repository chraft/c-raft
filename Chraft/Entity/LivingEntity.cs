using System;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Plugins.Events;
using Chraft.Plugins.Events.Args;
using Chraft.Utils;
using Chraft.World;

namespace Chraft.Entity
{
    public abstract class LivingEntity : EntityBase
    {

        public abstract string Name { get; }

        short _health;
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
    
        public LivingEntity(Server server, int entityId)
         : base(server, entityId)
        {
            this.Health = MaxHealth;
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

        public string FacingDirection(byte points)
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

        /// <summary>
        /// Perform any item drop logic during death
        /// </summary>
        protected virtual void DoDeath(EntityBase killedBy)
        {
            System.Timers.Timer removeTimer = new System.Timers.Timer(1000);

            removeTimer.Elapsed += delegate
            {
                removeTimer.Stop();
                World.Server.RemoveEntity(this);
                removeTimer.Dispose();
            };

            removeTimer.Start();
        }

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


    }
}

