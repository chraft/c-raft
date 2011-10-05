using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockReed : BlockBase
    {
        public BlockReed()
        {
            Name = "Reed";
            Type = BlockData.Blocks.Reed;
            Opacity = 0x0;
            IsAir = true;
            IsSolid = true;
            IsSingleHit = true;
            LootTable.Add(new ItemStack((short)Type, 1));
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            // TODO: Check there is water nearby before placing.
            base.Place(entity, block, targetBlock, face);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if (targetBlock.Y > sourceBlock.Y)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }
    }
}
