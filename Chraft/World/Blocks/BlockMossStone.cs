using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockMossStone : BlockBase
    {
        public BlockMossStone()
        {
            Name = "MossStone";
            Type = BlockData.Blocks.Moss_Stone;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
        }
    }
}
