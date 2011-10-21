using System;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Utils;
using Chraft.World;


namespace Chraft.Entity {
    partial class Mob {

        // Behaviour junk
        private bool AIWaiting;
        public bool Hunter; // Is this mob capable of tracking clients?
        public bool Hunting; // Is this mob currently tracking a client?

        public void Update()
        {
            
            // TODO: Theory of Cosines to get direction heading from yaw or pitch.

            // TODO: confirm when is sine and which is cosine
            //X = (float)Math.Cos(angle); // up is 0 and west(left) is Pi/2 for this
            //Z = (float)Math.Sin(angle); // angle is radians

            if (true) // If to check if we've travelled in a direction long enough. Reset Velocity.
                Velocity = new Vector3(0, 0, 0); // Too lazy so mob is gonna be ADHD.
            if (!AIWaiting)
                switch (new Random().Next(1, 5))
                {
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
                        waitTimer.Elapsed += delegate
                        {
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
            if (Velocity.Z != 0)
            {
                if (World.GetBlockId(UniversalCoords.FromAbsWorld(Position.X, Position.Y, (Position.Z + Velocity.Z))) != 0)
                    if (World.GetBlockId(UniversalCoords.FromAbsWorld(Position.X, Position.Y + 1, (Position.Z + Velocity.Z))) != 0)
                        Velocity.Z -= Velocity.Z;
                    else
                        Velocity.Y += 1;
            }
            if (Velocity.X != 0)
            {
                if (World.GetBlockId(UniversalCoords.FromAbsWorld((Position.X + Velocity.X), Position.Y, Position.Z)) != 0)
                    if (World.GetBlockId(UniversalCoords.FromAbsWorld((Position.X + Velocity.X), Position.Y + 1, Position.Z)) != 0)
                        Velocity.X -= Velocity.X;
                    else
                        Velocity.Y += 1;
            }

            // TODO: Actual gravity
            if (World.GetBlockId(UniversalCoords.FromAbsWorld(Position.X, Position.Y - 1, Position.Z)) == 0)
                Velocity.Y -= 1;

            // Emergency Collision Detection
            if (World.GetBlockId(UniversalCoords.FromAbsWorld((Position.X + Velocity.X), (Position.Y + Velocity.Y), (Position.Z + Velocity.Z))) != 0)
            {
                // We're going straight into a block! Oh nooooooes.
                Velocity.Y += 1;
            }

            UpdatePosition();
        }

        public void UpdatePosition() {
            this.Position = new AbsWorldCoords(this.Position.ToVector() + Velocity);
            EntityTeleportPacket et = new EntityTeleportPacket
            {
                EntityId = this.EntityId,
                X = this.Position.X,
                Y = this.Position.Y,
                Z = this.Position.Z,
                Yaw = this.PackedYaw,
                Pitch = this.PackedPitch
            };
            World.Server.SendPacketToNearbyPlayers(World,
                                                   UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), 
                                                   et);
        }
    }
}
