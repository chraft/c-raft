using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockDoubleStoneSlab : BlockBase
    {
        public BlockDoubleStoneSlab()
        {
            Name = "DoubleStoneSlab";
            Type = BlockData.Blocks.Double_Stone_Slab;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)BlockData.Blocks.Slab, 1));
        }
    }
}
