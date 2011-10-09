using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockTNT : BlockBase
    {
        public BlockTNT()
        {
            Name = "TNT";
            Type = BlockData.Blocks.TNT;
            IsSingleHit = true;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
        }
    }
}
