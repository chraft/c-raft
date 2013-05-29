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
using Chraft.Entity;
using Chraft.Entity.Items;
using Chraft.Interfaces;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utils;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks.Physics
{
    public class FallingSand : BaseFallingPhysics
    {
        protected byte BlockId;

        public FallingSand(WorldManager world, AbsWorldCoords pos) : base(world, pos)
        {
            Type = Net.Packets.AddObjectVehiclePacket.ObjectType.FallingObjects;
            BlockId = (byte) BlockData.Blocks.Sand;
            Velocity = new Vector3(0, -0.4D, 0);
        }

        public override void Simulate()
        {
            int x = (int)Math.Floor(Position.X);
            int y = (int)Math.Floor(Position.Y);
            int z = (int)Math.Floor(Position.Z);
            byte? blockId = World.GetBlockId(x, y, z);
            if (blockId == null || blockId != (byte)BlockData.Blocks.Air)
            {
                Stop();
                return;
            }

            if (Position.Y <= 1)
            {
                Stop(true);
                return;
            }

            Position = new AbsWorldCoords(Position.ToVector() + Velocity);
        }

        protected override void OnStop()
        {
            UniversalCoords currentBlockCoords = UniversalCoords.FromAbsWorld(Position);
            byte? blockId = World.GetBlockId(currentBlockCoords);

            if (blockId == null)
                return;

            if (BlockHelper.Instance.IsAir((byte)blockId))
            {
                var item = ItemHelper.GetInstance(BlockId);
                item.Count = 1;
                World.Server.DropItem(World, currentBlockCoords, item);
            }
            else
            {
                UniversalCoords aboveBlockCoords = UniversalCoords.FromWorld(currentBlockCoords.WorldX,
                                                                             currentBlockCoords.WorldY + 1,
                                                                             currentBlockCoords.WorldZ);
                StructBlock aboveBlock = new StructBlock(aboveBlockCoords, BlockId, 0, World);
                BlockHelper.Instance.CreateBlockInstance(BlockId).Spawn(aboveBlock);

                foreach (LivingEntity living in World.Server.GetNearbyLivings(World, aboveBlockCoords))
                {
                    if (Math.Abs(living.Position.X - aboveBlockCoords.WorldX) < 2 &&
                        Math.Abs(living.Position.Z - aboveBlockCoords.WorldZ) < 2 &&
                        Math.Abs(living.Position.Y + living.Height - aboveBlockCoords.WorldY) < 2)
                        living.CheckSuffocation();
                }
            }
            base.OnStop();
        }
    }
}
