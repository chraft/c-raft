using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockWater : BlockBase
    {
        public BlockWater()
        {
            Name = "Water";
            Type = BlockData.Blocks.Water;
            IsLiquid = true;
            Opacity = 0x2;
        }
    }
}
