using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockCoalOre : BlockBase
    {
        public BlockCoalOre()
        {
            Name = "CoalOre";
            Type = BlockData.Blocks.Coal_Ore;
            IsSolid = true;
            DropItem = BlockData.Items.Coal;
            DropItemAmount = 1;
        }
    }
}
