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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockRedstoneOreGlowing : BlockBase
    {
        public BlockRedstoneOreGlowing()
        {
            Name = "RedstoneOreGlowing";
            Type = BlockData.Blocks.Redstone_Ore_Glowing;
            IsSolid = true;
            Luminance = 0x7;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            LootTable.Add(new ItemStack((short)BlockData.Items.Redstone, (sbyte)(2 + block.World.Server.Rand.Next(4))));
            base.DropItems(entity, block);
        }
    }
}
