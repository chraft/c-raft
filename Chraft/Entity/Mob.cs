using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.Utils;
using Chraft.World.Blocks;

namespace Chraft.Entity
{
	public abstract partial class Mob : LivingEntity
	{
        public MobType Type { get; set; }

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
        /// How far the mob can see (i.e. if aggressive a player will be hunted if seen within this range)
        /// </summary>
        public virtual int SightRange 
        {
            get
            {
                return 16;
            }
        }

	    public virtual int MaxSpawnedPerGroup
	    {
            get
            {
                return 4;
            }

	    }

        public int GotoLoc; // Location as int entity should move towards
        public Vector3 gotoPos; // Location entity should move towards

        protected Mob(WorldManager world, int entityId, MobType type, MetaData data)
			: base(world.Server, entityId, data)
		{
            this.Type = type;
            this.World = world;
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

        public override void Attack(LivingEntity target)
        {
            if (target == null)
                return;
            target.Damage(DamageCause.EntityAttack, AttackStrength, this);
        }

        public void Despawn()
        {
            Server.RemoveEntity(this);

            // Client.UpdateEntities() will handle any notifications about this entity disappearing
        }


        protected virtual double BlockPathWeight(UniversalCoords coords)
        {
            return 0.0;
        }

        /// <summary>
        /// In addition to <see cref="LivingEntity.CanSpawnHere"/> determine if the BlockPathWeight of the location is suitable for spawning
        /// </summary>
        /// <returns>True if the <see cref="Creature"/> can spawn at <see cref="EntityBase.Position"/>; otherwise False</returns>
        public override bool CanSpawnHere()
        {
            return base.CanSpawnHere() && BlockPathWeight(this.BlockPosition) >= 0.0F;
        }
	}
}
