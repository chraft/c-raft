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
using Chraft.Entity;
using Chraft.Net;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockCake : BlockBase, IBlockInteractive
    {
        public BlockCake()
        {
            Name = "Cake";
            Type = BlockData.Blocks.Cake;
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            // Eat the cake. No food restoration at the moment.

            // Restore hp/food

            if (block.MetaData == (byte)MetaData.Cake.OneLeft)
            {
                // Cake is dead.
                Destroy(entity, block);
            } else
            {
                // Eat one piece
                block.World.SetBlockData(block.Coords, block.MetaData++);
            }
        }
    }
}
