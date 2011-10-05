using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockStickyPiston : BlockBase
    {
        public BlockStickyPiston()
        {
            Name = "StickyPiston";
            Type = BlockData.Blocks.StickyPiston;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
        }
    }
}
