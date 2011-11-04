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
using Chraft.Net;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockBurningFurnace : BlockBase, IBlockInteractive
    {
        public BlockBurningFurnace()
        {
            Name = "BurningFurnace";
            Type = BlockData.Blocks.Burning_Furnace;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)BlockData.Blocks.Furnace, 1));
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            // You can not place the furnace that is already burning.
        }

        protected override void UpdateOnDestroy(StructBlock block)
        {
            FurnaceInterface.StopBurning(block.World, block.Coords);
            base.UpdateOnDestroy(block);
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player != null)
            {
                FurnaceInterface fi = new FurnaceInterface(block.World, block.Coords);
                fi.Associate(player);
                fi.DropAll(block.Coords);
                fi.Save();
            }
            base.DropItems(entity, block);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player == null)
                return;
            if (player.CurrentInterface != null)
                return;
            player.CurrentInterface = new FurnaceInterface(block.World, block.Coords);
            player.CurrentInterface.Associate(player);
            player.CurrentInterface.Open();
        }

    }
}
