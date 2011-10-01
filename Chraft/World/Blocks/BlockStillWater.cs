using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockStillWater : BlockBase
    {
        public BlockStillWater()
        {
            Name = "StillWater";
            Type = BlockData.Blocks.Still_Water;
            IsLiquid = true;
            Opacity = 0x2;
        }
    }
}
