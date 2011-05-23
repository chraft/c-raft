using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using Chraft.Net;

namespace Chraft.Entity
{
	public class Mob : EntityBase
	{
		public MobType Type { get; set; }
		public MetaData Data { get; private set; }
        public int Health { get; set; }

        public int AttackRange; // Clients within this range will take damage
        public int SightRange; // Clients within this range will be hunted
        public int GotoLoc; // Location as int entity should move towards
        public double gotoX, gotoY, gotoZ; // Location entity should move towards

        public bool Hunter; // Is this mob capable of tracking clients?
        public bool Hunting; // Is this mob currently tracking a client?
        
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
                // Get the Clients held item.
                this.Health -= 1;
            }
            else
            {
                // Generic damage from falling/lava/fire?
                this.Health -= 1;
            }

            foreach (Client c in World.Server.GetNearbyPlayers(World, X, Y, Z))
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

            foreach (Client c in World.Server.GetNearbyPlayers(World, X, Y, Z))
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
                World.Server.Entities.Remove(this);
                removeTimer.Dispose();
            };

            removeTimer.Start();
        }

        public void HuntMode()
        {
            int newGotoLoc;

            foreach (Client c in World.Server.GetNearbyPlayers(World, X, Y, Z))
            {
                if (Math.Abs(c.X - X) <= AttackRange)
                {
                    if (Math.Abs(c.Y - Y) < 1)
                    {
                        if (Math.Abs(c.Z - Z) <= AttackRange)
                        {
                            //c.DamageClient(this);
                        }
                    }
                }

                newGotoLoc = (int)Math.Abs(c.X - X) + (int)Math.Abs(c.Y - Y) + (int)Math.Abs(c.Z - Z);
                if (GotoLoc < newGotoLoc && GotoLoc < SightRange)
                {
                    this.World.Logger.Log(Logger.LogLevel.Debug, "Found: " + X + ", " + Y + ", " + Z);
                    GotoLoc = newGotoLoc;
                    gotoX = c.X;
                    gotoY = c.Y;
                    gotoZ = c.Z;
                    Hunting = true;
                }
            }

            if (Hunting != true)
                PassiveMode();
            else 
                ProcessMovement(gotoX, gotoY, gotoZ);
        }

        public void PassiveMode()
        {
            if (Hunter)
            {
                foreach (Client c in World.Server.GetNearbyPlayers(World, X, Y, Z))
                {
                    int newGotoLoc = (int)Math.Abs(c.X - X) + (int)Math.Abs(c.Y - Y) + (int)Math.Abs(c.Z - Z);
                    if (newGotoLoc < SightRange)
                    {
                        Hunting = true;
                        HuntMode();
                        return;
                    }
                }
            }

            if (gotoX != X && gotoY != Y && gotoZ != Z)
            {
                gotoX = X + 2;
                gotoY = Y;
                gotoZ = Z + 2;
            }
            
            if (Yaw < 128) Yaw += 8;
            else Yaw = 0;

            if (Pitch > 16 && Pitch < 32) Pitch = 128;
            else if (Pitch > 200) Pitch = 0;
            else Pitch += 1;

            ProcessMovement(gotoX, gotoX, gotoX);
        }

        private void ProcessMovement(double mX, double mY, double mZ)
        {
            int x = (int)(X + (Math.Sign(mX - X)));
            int y = (int)(Y -1 );
            int z = (int)(Z + (Math.Sign(mZ - Z)));

            byte b = World.GetBlockId(x, y, z);
            byte b1 = World.GetBlockId(x, y + 1, z);
            byte b2 = World.GetBlockId(x, y + 2, z);
            byte b3 = World.GetBlockId(x, y + 3, z);

            if (b != 0)
                Y += 1;
            X += Math.Sign(mX - X) * 0.2;
            Z += Math.Sign(mZ - Z) * 0.2;

            foreach (Client c in World.Server.GetNearbyPlayers(World, X, Y, Z))
            {
                    c.PacketHandler.SendPacket(new EntityTeleportPacket
                    {
                        EntityId = this.EntityId,
                        X = this.X,
                        Y = this.Y,
                        Z = this.Z,
                        Yaw = this.PackedYaw,
                        Pitch = this.PackedPitch
                    });
            }
        }
	}
}
