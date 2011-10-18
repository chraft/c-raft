using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockSlab : BlockBase
    {
        public BlockSlab()
        {
            Name = "Slab";
            Type = BlockData.Blocks.Slab;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
            BlockBoundsOffset = new BoundingBox(0, 0, 0, 1, 0.5, 1);
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            // TODO : If (Block  Y - 1 = Stair && Block Y = Air) Then DoubleStair
            // Else if (Buildblock = Stair) Then DoubleStair
            base.Place(entity, block, targetBlock, face);
        }
    }
}
