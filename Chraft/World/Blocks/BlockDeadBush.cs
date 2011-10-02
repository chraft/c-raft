using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockDeadBush : BlockBase
    {
        public BlockDeadBush()
        {
            Name = "DeadBush";
            Type = BlockData.Blocks.DeadBush;
            IsSingleHit = true;
            IsAir = true;
            IsSolid = true;
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            if (face == BlockFace.Down)
                return;
            byte blockId = targetBlock.World.GetBlockId(block.X, block.Y - 1, block.Z);
            // We can place the dead bush only on the sand
            if (blockId != (byte)BlockData.Blocks.Sand)
                return;
            // We can place the dead bush only on top of the sand block
            if (targetBlock.Type != (byte)BlockData.Blocks.Sand || face != BlockFace.Up)
                return;
            base.Place(entity, block, targetBlock, face);
        }
    }
}
