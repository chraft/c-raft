using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockWallSign : BlockBase
    {
        public BlockWallSign()
        {
            Name = "WallSign";
            Type = BlockData.Blocks.Wall_Sign;
            IsAir = true;
            IsSingleHit = true;
            DropItem = BlockData.Items.Sign;
            DropItemAmount = 1;
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            if (face != BlockFace.West && face != BlockFace.North && face != BlockFace.South && face != BlockFace.East)
                return;
            base.Place(entity, block, targetBlock, face);
        }
    }
}
