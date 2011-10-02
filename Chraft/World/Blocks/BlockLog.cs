using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockLog : BlockBase
    {
        public BlockLog()
        {
            Name = "Log";
            Type = BlockData.Blocks.Log;
            IsSolid = true;
            BurnEfficiency = 300;
            LootTable.Add(new ItemStack((short)Type, 1));
        }
    }
}
