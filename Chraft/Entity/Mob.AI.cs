#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.PluginSystem;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utils;
using Chraft.World;


namespace Chraft.Entity {
    partial class Mob
    {

        // Behaviour junk
        private bool AIWaiting;
        public bool Hunter { get; internal set; } // Is this mob capable of tracking entities?
        public bool Hunting { get; internal set; } // Is this mob currently tracking an entity?

        // TODO: this hides the inherited Update from EntityBase, change this
        internal void Update()
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
                double zOffset = (Position.Z + Velocity.Z);
                Chunk chunk = World.GetChunkFromAbs(Position.X, zOffset) as Chunk;

                if (chunk == null)
                    return;

                if (chunk.GetType(UniversalCoords.FromAbsWorld(Position.X, Position.Y, zOffset)) != BlockData.Blocks.Air)
                    if (chunk.GetType(UniversalCoords.FromAbsWorld(Position.X, Position.Y + 1, zOffset)) != BlockData.Blocks.Air)
                        Velocity.Z -= Velocity.Z;
                    else
                        Velocity.Y += 1;
            }
            if (Velocity.X != 0)
            {
                double xOffset = (Position.X + Velocity.X);

                Chunk chunk = World.GetChunkFromAbs(Position.X, Position.Z) as Chunk;

                if (chunk == null)
                    return;

                if (chunk.GetType(UniversalCoords.FromAbsWorld(xOffset, Position.Y, Position.Z)) != BlockData.Blocks.Air)
                    if (chunk.GetType(UniversalCoords.FromAbsWorld(xOffset, Position.Y + 1, Position.Z)) != BlockData.Blocks.Air)
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

        internal void UpdatePosition() {
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
                                                   UniversalCoords.FromAbsWorld(Position), 
                                                   et);
        }
    }
}
