using System;
using Chraft.Net.Packets;
using Chraft.World.NBT;

namespace Chraft.Entity {
    partial class Mob {

        public Vector3 Velocity { get; set; } // What direction are we going.

        // Behaviour junk
        private bool AIWaiting;
        public bool Hunter; // Is this mob capable of tracking clients?
        public bool Hunting; // Is this mob currently tracking a client?

        public void Update() {
            // TODO: Theory of Cosines to get direction heading from yaw or pitch.

            if(true) // If to check if we've travelled in a direction long enough. Reset Velocity.
                Velocity = new Vector3(0, 0, 0); // Too lazy so mob is gonna be ADHD.
       if(!AIWaiting)
            switch (new Random().Next(1,5)) {
                case 1:
                    Velocity = new Vector3(1, 0, 0);
                    break;
                case 2:
                    Velocity = new Vector3(-1, 0, 0);
                    break;
                case 3:
                    Velocity = new Vector3(0, 0, 1);
                    break;
                case 4:
                    System.Timers.Timer waitTimer = new System.Timers.Timer(new Random().Next(1, 5) * 1000);
                    waitTimer.Elapsed += delegate {
                        waitTimer.Stop();
                        this.AIWaiting = false;
                        waitTimer.Dispose();
                    };
                    this.AIWaiting = true;
                    waitTimer.Start();
                    break;
                default:
                    Velocity = new Vector3(0, 0, -1);
                    break;
            }
            // TODO: Actual collision prediction.
        if (Velocity.Z != 0) {
            if (World.GetBlockId((int)Position.X, (int)Position.Y, (int)(Position.Z + Velocity.Z)) != 0)
                if (World.GetBlockId((int)Position.X, (int)Position.Y + 1, (int)(Position.Z + Velocity.Z)) != 0)
                    Velocity.Z -= Velocity.Z;
                else
                    Velocity.Y += 1;
        }
        if (Velocity.X != 0) {
            if (World.GetBlockId((int)(Position.X + Velocity.X), (int)Position.Y, (int)Position.Z) != 0)
                if (World.GetBlockId((int)(Position.X + Velocity.X), (int)Position.Y + 1, (int)Position.Z) != 0)
                    Velocity.X -= Velocity.X;
                else
                    Velocity.Y += 1;
        }

            // TODO: Actual gravity
        if (World.GetBlockId((int)Position.X, (int)Position.Y - 1, (int)Position.Z) == 0)
            Velocity.Y -= 1;

            UpdatePosition();
        }

        /*
        private void ProcessMovement(double mX, double mY, double mZ) {
            int x = (int)(Position.X + (Math.Sign(mX - Position.X)));
            int y = (int)(Position.Y - 1);
            int z = (int)(Position.Z + (Math.Sign(mZ - Position.Z)));

            byte b = World.GetBlockId(x, y, z);
            //byte b1 = World.GetBlockId(x, y + 1, z);
            //byte b2 = World.GetBlockId(x, y + 2, z);
            //byte b3 = World.GetBlockId(x, y + 3, z);

            //if (b != 0)
            //    Position.Y += 1;

            Position.X += (World.GetBlockId((int)Math.Sign(mX - Position.X), (int)Position.Y, (int)Position.Z) != 0) ? 0 : Math.Sign(mX - Position.X) * 0.2;
            Position.Z += (World.GetBlockId((int)Position.X, (int)Position.Y, (int)Math.Sign(mZ - Position.Z)) != 0) ? 0 : Math.Sign(mZ - Position.Z) * 0.2;
            Position.Y += (b == 0) ? -1 : 0;
            if (World.GetBlockId((int)this.Position.X + 1, (int)this.Position.Y, (int)this.Position.Z) != 0)
                Position.Y += 1;

        }*/

        public void UpdatePosition() {
            this.Position.X += Velocity.X;
            this.Position.Y += Velocity.Y;
            this.Position.Z += Velocity.Z;
            foreach (Client c in World.Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z)) {
                c.PacketHandler.SendPacket(new EntityTeleportPacket {
                    EntityId = this.EntityId,
                    X = this.Position.X,
                    Y = this.Position.Y,
                    Z = this.Position.Z,
                    Yaw = this.PackedYaw,
                    Pitch = this.PackedPitch
                });
            }
        }
    }
}
