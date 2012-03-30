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
using System.Collections.Generic;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockLeaves : BlockBase
    {
        public BlockLeaves()
        {
            Name = "Leaves";
            Type = BlockData.Blocks.Leaves;
            Opacity = 0x1;
            IsSolid = true;
            BurnEfficiency = 300;
        }

        protected override void DropItems(EntityBase entity, StructBlock block, List<ItemStack> overridedLoot = null)
        {
            overridedLoot = new List<ItemStack>();
            Player player = entity as Player;
            if (player != null)
            {
                if (player.Inventory.ActiveItem.Type == (short)BlockData.Items.Shears)
                    overridedLoot.Add(new ItemStack((short)Type, 1, block.MetaData));              
                else if (block.World.Server.Rand.Next(5) == 0)
                    overridedLoot.Add(new ItemStack((short)BlockData.Blocks.Sapling, 1));
            }
            base.DropItems(entity, block, overridedLoot);
        }

    }
}
