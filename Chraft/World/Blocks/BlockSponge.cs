using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockSponge : BlockBase
    {
        public BlockSponge()
        {
            Name = "Sponge";
            Type = BlockData.Blocks.Sponge;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
        }
    }
}
