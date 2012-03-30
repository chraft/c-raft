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
using Chraft.PluginSystem;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Collision;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockTallGrass : BlockBase
    {
        public BlockTallGrass()
        {
            Name = "TallGrass";
            Type = BlockData.Blocks.TallGrass;
            IsSingleHit = true;
            IsAir = true;
            IsSolid = true;
            Opacity = 0x0;
            BlockBoundsOffset = new BoundingBox(0.1, 0, 0.1, 0.9, 0.8, 0.9);
        }

        public override void Place(IEntityBase entity, IStructBlock iBlock, IStructBlock targetIBlock, BlockFace face)
        {
            StructBlock block = (StructBlock)iBlock;
            StructBlock targetBlock = (StructBlock) targetIBlock;

            if (face == BlockFace.Down)
                return;
            byte? blockId = targetBlock.World.GetBlockId(UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY - 1, block.Coords.WorldZ));
            // We can place the tall grass only on the fertile blocks - dirt, soil, grass)
            if (blockId == null || !BlockHelper.Instance.IsFertile((byte)blockId))
                return;
            base.Place(entity, block, targetBlock, face);
        }

        protected override void DropItems(EntityBase entity, StructBlock block, List<ItemStack> overridedLoot = null)
        {
            overridedLoot = new List<ItemStack>();
            Player player = entity as Player;
            if (player != null)
            {
                // If hit by a shear - drop the grass
                if (player.Inventory.ActiveItem.Type == (short)BlockData.Items.Shears)
                    overridedLoot.Add(new ItemStack((short) Type, 1, block.MetaData));
                else if (player.Server.Rand.Next(3) == 0)
                        overridedLoot.Add(new ItemStack((short)BlockData.Items.Seeds, 1));
            }
            base.DropItems(entity, block, overridedLoot);
        }

        protected override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }
    }
}
