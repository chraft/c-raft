using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockIce : BlockBase
    {
        public BlockIce()
        {
            Name = "Ice";
            Type = BlockData.Blocks.Ice;
            Opacity = 0x2;
        }
    }
}
