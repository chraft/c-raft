using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockStillLava : BlockBase
    {
        public BlockStillLava()
        {
            Name = "StillLava";
            Type = BlockData.Blocks.Still_Lava;
            IsLiquid = true;
            Luminance = 0xf;
        }
    }
}
