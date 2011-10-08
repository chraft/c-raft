using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.Utils;

namespace Chraft.Entity
{
	public abstract partial class Mob : EntityBase
	{
        public abstract string Name { get; }

        public MobType Type { get; set; }
		public MetaData Data { get; internal set; }

        /// <summary>
        /// The amount of damage this Mob can inflict
        /// </summary>
        public virtual short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Targets within this range will take damage
        /// </summary>
        public virtual int AttackRange
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// How far the mob can see (i.e. if aggressive a client will be hunted if seen within this range)
        /// </summary>
        public virtual int SightRange 
        {
            get
            {
                return 16;
            }
        }

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

        protected void SendMetadataUpdate()
        {
            World.Server.SendPacketToNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z),
               new EntityMetadataPacket // Metadata update
               {
                   EntityId = this.EntityId,
                   Data = this.Data
               });
        }

        public void DamageMob(EntityBase hitBy = null)
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
                Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_DAMAGE, e);
                if (e.EventCanceled) return;
                damage = e.Damage;
                hitByPlayer = e.DamagedBy;
                //End Event

                //Debug
                hitByPlayer.Client.SendMessage("You hit a " + this.Name + " with a " + itemHeld.Type.ToString() + " dealing " + damage.ToString() + " damage.");
                this.Health -= damage;
            }
            else if (hitByMob != null)
            {
                // Hit by a Mob so apply its' attack strength as damage
                short damage = hitByMob.AttackStrength;
                //Event
                EntityDamageEventArgs e = new EntityDamageEventArgs(this, damage, null, DamageCause.EntityAttack);
                Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_DAMAGE, e);
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
                Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_DAMAGE, e);
                if (e.EventCanceled) return;
                damage = e.Damage;
                //End Event

                // TODO: Generic damage from falling/lava/fire?
                this.Health -= damage;
            }

            foreach (Client c in World.Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
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

            // TODO: Entity Knockback

            if (this.Health <= 0) HandleDeath(hitByPlayer);
        }

        protected virtual void DoInteraction(Client client, ItemStack item)
        {
        }

        /// <summary>
        /// When a player interacts with a mob (right-click) with an item / hand
        /// </summary>
        /// <param name="client">The client that is interacting</param>
        /// <param name="item">The item being used (could be Void e.g. Hand)</param>
        public void InteractWith(Client client, ItemStack item)
        {
            // TODO: create a plugin event for this action

            DoInteraction(client, item);
        }

        /// <summary>
        /// Perform any item drop logic during death
        /// </summary>
        protected abstract void DoDeath(EntityBase killedBy);

        public void HandleDeath(EntityBase killedBy = null)
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

            World.Server.SendPacketToNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z),
                new EntityStatusPacket // Death Action
                {
                    EntityId = EntityId,
                    EntityStatus = 3
                });

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

        public void Despawn()
        {
            Server.RemoveEntity(this);

            // Client.UpdateEntities() will handle any notifications about this entity disappearing
        }
	}
}
