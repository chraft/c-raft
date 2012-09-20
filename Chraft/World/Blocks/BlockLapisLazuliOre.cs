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
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Interfaces;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockLapisLazuliOre : BlockBase
    {
        public BlockLapisLazuliOre()
        {
            Name = "LapisLazuliOre";
            Type = BlockData.Blocks.Lapis_Lazuli_Ore;
            IsSolid = true;
        }

        protected override void DropItems(EntityBase entity, StructBlock block, List<ItemInventory> overridedLoot = null)
        {
            overridedLoot = new List<ItemInventory>();
            ItemInventory item = ItemHelper.GetInstance((short) BlockData.Items.Ink_Sack);
            item.Count = (sbyte) (3 + block.World.Server.Rand.Next(17));
            item.Durability = 4;
            overridedLoot.Add(item);
            base.DropItems(entity, block, overridedLoot);
        }
    }
}
