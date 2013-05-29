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

        protected override void DropItems(EntityBase entity, StructBlock block, List<ItemInventory> overridedLoot = null)
        {
            overridedLoot = new List<ItemInventory>();
            var player = entity as Player;
            if (player != null)
            {
                ItemInventory item;
                if (player.Inventory.ActiveItem is ItemShears)
                {
                    item = ItemHelper.GetInstance((short)Type);
                    item.Count = 1;
                    item.Durability = block.MetaData;
                    overridedLoot.Add(item);
                }
                else if (block.World.Server.Rand.Next(5) == 0)
                {
                    item = ItemHelper.GetInstance((short)BlockData.Blocks.Sapling);
                    item.Count = 1;
                    overridedLoot.Add(item);
                }
            }
            base.DropItems(entity, block, overridedLoot);
        }

    }
}
