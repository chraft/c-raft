using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockLeaves : BlockBase
    {
        public BlockLeaves()
        {
            Name = "Leaves";
            Type = BlockData.Blocks.Leaves;
            Opacity = 0x1;
            IsSolid = true;
            BurnEfficiency = 300;
            DropBlock = BlockData.Blocks.Sapling;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            DropBlockAmount = (sbyte)(block.World.Server.Rand.Next(5) == 0 ? 1 : 0);
            base.DropItems(entity, block);
        }

    }
}
