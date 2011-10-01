using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockGoldOre : BlockBase
    {
        public BlockGoldOre()
        {
            Name = "GoldOre";
            Type = BlockData.Blocks.Gold_Ore;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Gold_Ore;
            DropBlockAmount = 1;
        }
    }
}
