using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockGlass : BlockBase
    {
        public BlockGlass()
        {
            Name = "Glass";
            Type = BlockData.Blocks.Glass;
            Opacity = 0x0;
        }
    }
}
