using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockWool : BlockBase
    {
        public BlockWool()
        {
            Name = "Wool";
            Type = BlockData.Blocks.Wool;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Wool;
            DropBlockAmount = 1;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            DropBlockMeta = block.MetaData;
            base.DropItems(entity, block);
        }
    }
}
