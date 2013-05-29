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
    class BlockLightstone : BlockBase
    {
        public BlockLightstone()
        {
            Name = "Lightstone";
            Type = BlockData.Blocks.Lightstone;
            Luminance = 0xf;
            IsSolid = true;

        }

        protected override void DropItems(EntityBase entity, StructBlock block, List<ItemInventory> overridedLoot = null)
        {
            var player = entity as Player;
            overridedLoot = new List<ItemInventory>();
            if (player != null)
                if (player.Inventory.ActiveItem.Type == (short)BlockData.Items.Wooden_Pickaxe ||
                    player.Inventory.ActiveItem.Type == (short)BlockData.Items.Stone_Pickaxe ||
                    player.Inventory.ActiveItem.Type == (short)BlockData.Items.Iron_Pickaxe ||
                    player.Inventory.ActiveItem.Type == (short)BlockData.Items.Gold_Pickaxe ||
                    player.Inventory.ActiveItem.Type == (short)BlockData.Items.Diamond_Pickaxe)
                {
                    var item = ItemHelper.GetInstance(BlockData.Items.Lightstone_Dust);
                    item.Count = (sbyte)(2 + block.World.Server.Rand.Next(2));
                    overridedLoot.Add(item);
                }
            base.DropItems(entity, block, overridedLoot);
        }
    }
}
