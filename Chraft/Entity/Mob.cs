using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.World;

namespace Chraft.Entity
{
	public partial class Mob : EntityBase
	{
		public MobType Type { get; set; }
		public MetaData Data { get; private set; }

        public int AttackRange; // Clients within this range will take damage
        public int SightRange; // Clients within this range will be hunted
        public int GotoLoc; // Location as int entity should move towards
        public Vector3 gotoPos; // Location entity should move towards
        
		public Mob(Server server, int entityId, MobType type)
			: this(server, entityId, type, new MetaData())
		{
		}

		public Mob(Server server, int entityId, MobType type, MetaData data)
			: base(server, entityId)
		{
			Data = data;
			Type = type;
		}

        public void DamageMob(Client hitBy = null)
        {
            if (hitBy != null)
            {
                // TODO: Get the Clients held item.
                this.Health -= 1;
            }
            else
            {
                // TODO: Generic damage from falling/lava/fire?
                this.Health -= 1;
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

        public void HandleDeath(Client hitBy = null)
        {
            if (hitBy != null)
            {
                // TODO: Stats/Achievement hook or something
            }

            foreach (Client c in World.Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                c.PacketHandler.SendPacket(new EntityStatusPacket // Death Action
                {
                    EntityId = this.EntityId,
                    EntityStatus = 3
                });
            }

            // TODO: Spawn goodies

            System.Timers.Timer removeTimer = new System.Timers.Timer(1000);

            removeTimer.Elapsed += delegate
            {
                removeTimer.Stop();
                World.Server.RemoveEntity(this);
                removeTimer.Dispose();
            };

            removeTimer.Start();
        }
	}
}
