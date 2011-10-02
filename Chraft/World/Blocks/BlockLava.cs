using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockLava : BlockBase
    {
        public BlockLava()
        {
            Name = "Lava";
            Type = BlockData.Blocks.Lava;
            IsLiquid = true;
            Luminance = 0xf;
        }
    }
}
