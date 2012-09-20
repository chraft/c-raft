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

using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockIce : BlockBase
    {
        public BlockIce()
        {
            Name = "Ice";
            Type = BlockData.Blocks.Ice;
            Opacity = 0x2;
            IsSolid = true;
            Slipperiness = 0.98;
        }

        protected override void UpdateWorld(StructBlock block, bool isDestroyed = false)
        {
            if (!isDestroyed)
            {
                base.UpdateWorld(block, isDestroyed);
                return;
            }

            var water = new StructBlock(block.Coords, (byte)BlockData.Blocks.Water, 0, block.World);
            BlockHelper.Instance.CreateBlockInstance((byte)BlockData.Blocks.Water).Spawn(water);
        }
    }
}
