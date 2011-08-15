using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.Entity
{
	public abstract partial class Mob : EntityBase
	{
        public abstract string Name { get; }
        public abstract short AttackStrength { get; }

        public MobType Type { get; set; }
		public MetaData Data { get; internal set; }

        public int AttackRange; // Clients within this range will take damage
        public int SightRange; // Clients within this range will be hunted
        public int GotoLoc; // Location as int entity should move towards
        public Vector3 gotoPos; // Location entity should move towards

        protected Mob(WorldManager world, int entityId, MobType type, MetaData data)
			: base(world.Server, entityId)
		{
            if (data == null)
                data = new MetaData();
            this.Data = data;
            this.Type = type;
            this.World = world;
            this.Health = this.MaxHealth;
		}

        public void DamageMob(Client hitBy = null)
        {
            
            if (hitBy != null)
            {
                //TODO: Make damage more customizable.  CSV anyone?
                //TODO: Fix damage.
                //Damage values taken from http://www.minecraftwiki.net/wiki/Damage#Dealing_Damage
                short damage = 2;
                ItemStack itemHeld = hitBy.Inventory.ActiveItem;
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
                EntityDamageEventArgs e = new EntityDamageEventArgs(this, damage, hitBy, DamageCause.EntityAttack);
                Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_DAMAGE, e);
                if (e.EventCanceled) return;
                damage = e.Damage;
                hitBy = e.DamagedBy;
                //End Event

                //Debug
                hitBy.SendMessage("You hit a " + this.Name + " with a " + itemHeld.Type.ToString() + " dealing " + damage.ToString() + " damage.");
                this.Health -= damage;
            }
            else
            {
                short damage = 1;
                //Event
                EntityDamageEventArgs e = new EntityDamageEventArgs(this, damage, null, DamageCause.EntityAttack);
                Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_DAMAGE, e);
                if (e.EventCanceled) return;
                damage = e.Damage;
                //End Event

                // TODO: Generic damage from falling/lava/fire?
                this.Health -= damage;
            }

            foreach (Client c in World.Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
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

            // TODO: Entity Knockback

            if (this.Health == 0) HandleDeath(hitBy);
        }

        /// <summary>
        /// Perform any item drop logic during death
        /// </summary>
        //protected abstract void DoDrop();

        public void HandleDeath(Client hitBy = null)
        {
            //Event
            EntityDeathEventArgs e = new EntityDeathEventArgs(this, hitBy);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_DEATH, e);
            if (e.EventCanceled) return;
            hitBy = e.KilledBy;
            //End Event
            
            if (hitBy != null)
            {
                // TODO: Stats/Achievement hook or something
            }

            World.Server.SendPacketToNearbyPlayers(World, Position.X, Position.Y, Position.Z, 
                new EntityStatusPacket // Death Action
                {
                    EntityId = this.EntityId,
                    EntityStatus = 3
                });

            // Spawn goodies
            //DoDrop();

            System.Timers.Timer removeTimer = new System.Timers.Timer(1000);

            removeTimer.Elapsed += delegate
            {
                removeTimer.Stop();
                World.Server.RemoveEntity(this);
                removeTimer.Dispose();
            };

            removeTimer.Start();
        }

        public void Despawn()
        {
            this.Server.RemoveEntity(this);

            // Client.UpdateEntities() will handle any notifications about this entity disappearing
        }
	}
}
