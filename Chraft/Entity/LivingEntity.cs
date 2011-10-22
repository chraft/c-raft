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

        public virtual void Damage(DamageCause cause, short damageAmount, EntityBase hitBy = null, params object[] args)
        {
            var hitByPlayer = hitBy as Player;
            var hitByMob = hitBy as Mob;
            if (hitByPlayer != null)
            {
                //TODO: Make damage more customizable.  CSV anyone?
                //TODO: Fix damage.
                //Damage values taken from http://www.minecraftwiki.net/wiki/Damage#Dealing_Damage
                short damage = 2;
                ItemStack itemHeld = hitByPlayer.Inventory.ActiveItem;
                switch (itemHeld.Type)
                {
                    case 268:
                    case 283:
                        damage = 5;
                        break;
                    case 272:
                        damage = 7;
                        break;
                    case 267:
                        damage = 9;
                        break;
                    case 276:
                        damage = 11;
                        break;
                    case 273:
                        damage = 3;
                        break;
                    case 274:
                        damage = 4;
                        break;
                    case 275:
                        damage = 5;
                        break;
                }

                //Event
                EntityDamageEventArgs e = new EntityDamageEventArgs(this, damage, hitByPlayer, DamageCause.EntityAttack);
                Server.PluginManager.CallEvent(Event.ENTITY_DAMAGE, e);
                if (e.EventCanceled) return;
                damage = e.Damage;
                hitByPlayer = e.DamagedBy;
                //End Event

                //Debug
                hitByPlayer.Client.SendMessage("You hit a " + Name + " with a " + itemHeld.Type.ToString() + " dealing " + damage.ToString() + " damage.");
                this.Health -= damage;
            }
            else if (hitByMob != null)
            {
                // Hit by a Mob so apply its' attack strength as damage
                short damage = hitByMob.AttackStrength;
                //Event
                EntityDamageEventArgs e = new EntityDamageEventArgs(this, damage, null, DamageCause.EntityAttack);
                Server.PluginManager.CallEvent(Event.ENTITY_DAMAGE, e);
                if (e.EventCanceled) return;
                damage = e.Damage;
                //End Event

                // TODO: Generic damage from falling/lava/fire?
                this.Health -= damage;
            }
            else
            {
                short damage = 1;
                //Event
                EntityDamageEventArgs e = new EntityDamageEventArgs(this, damage, null, DamageCause.EntityAttack);
                Server.PluginManager.CallEvent(Event.ENTITY_DAMAGE, e);
                if (e.EventCanceled) return;
                damage = e.Damage;
                //End Event

                // TODO: Generic damage from falling/lava/fire?
                this.Health -= damage;
            }

            SendUpdateOnDamage();

            // TODO: Entity Knockback

            if (Health <= 0)
                HandleDeath(hitBy);
        }

        /// <summary>
        /// Perform any item drop logic during death
        /// </summary>
        protected abstract void DoDeath(EntityBase killedBy);

        public virtual void HandleDeath(EntityBase killedBy = null, string deathBy = "")
        {
            var killedByPlayer = killedBy as Player;

            //Event
            EntityDeathEventArgs e = new EntityDeathEventArgs(this, killedByPlayer);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_DEATH, e);
            if (e.EventCanceled) return;
            killedByPlayer = e.KilledBy;
            //End Event

            // TODO: Stats/achievements handled in each mob class??? (within DoDeath)
            //if (hitBy != null)
            //{
            //    // TODO: Stats/Achievement hook or something
            //}

            SendUpdateOnDeath();

            // Spawn goodies / perform achievements etc..
            DoDeath(killedBy);

            System.Timers.Timer removeTimer = new System.Timers.Timer(1000);

            removeTimer.Elapsed += delegate
            {
                removeTimer.Stop();
                World.Server.RemoveEntity(this);
                removeTimer.Dispose();
            };

            removeTimer.Start();
        }

        protected virtual void SendUpdateOnDeath()
        {
            World.Server.SendPacketToNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z),
                new EntityStatusPacket // Death Action
                {
                    EntityId = EntityId,
                    EntityStatus = 3
                });
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

