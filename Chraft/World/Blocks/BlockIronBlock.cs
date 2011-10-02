using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockIronBlock : BlockBase
    {
        public BlockIronBlock()
        {
            Name = "IronBlock";
            Type = BlockData.Blocks.Iron_Block;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
        }
    }
}
