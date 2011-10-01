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
    class BlockSignPost : BlockBase
    {
        public BlockSignPost()
        {
            Name = "SignPost";
            Type = BlockData.Blocks.Sign_Post;
            IsAir = true;
            IsSingleHit = true;
            DropItem = BlockData.Items.Sign;
            DropItemAmount = 1;
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            return;
        }
    }
}
