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
    class BlockRedstoneOre : BlockBase
    {
        public BlockRedstoneOre()
        {
            Name = "RedstoneOre";
            Type = BlockData.Blocks.Redstone_Ore;
            IsSolid = true;
            Luminance = 0x9;
        }

        protected override void DropItems(EntityBase entity, StructBlock block, List<ItemStack> overridedLoot = null)
        {
            overridedLoot = new List<ItemStack>();
            overridedLoot.Add(new ItemStack((short)BlockData.Items.Redstone, (sbyte)(2 + block.World.Server.Rand.Next(4))));
            base.DropItems(entity, block, overridedLoot);
        }
    }
}
