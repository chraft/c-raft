using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockWood : BlockBase
    {
        public BlockWood()
        {
            Name = "Wood";
            Type = BlockData.Blocks.Wood;
            IsSolid = true;
            BurnEfficiency = 300;
            LootTable.Add(new ItemStack((short)Type, 1));
        }
    }
}
