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
            IsSolid = true;
            IsSingleHit = true;
            DropBlock = BlockData.Blocks.Reed;
            DropBlockAmount = 1;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            // TODO: Check there is water nearby before placing.
            base.Place(entity, block, targetBlock, face);
        }
    }
}
