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
            byte blockId = targetBlock.World.GetBlockId(UniversalCoords.FromAbsWorld(block.Coords.WorldX, block.Coords.WorldY - 1, block.Coords.WorldZ));
            // We can place the dead bush only on the sand
            if (blockId != (byte)BlockData.Blocks.Sand)
                return;
            // We can place the dead bush only on top of the sand block
            if (targetBlock.Type != (byte)BlockData.Blocks.Sand || face != BlockFace.Up)
                return;
            base.Place(entity, block, targetBlock, face);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

    }
}
