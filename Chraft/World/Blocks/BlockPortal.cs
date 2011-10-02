using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockPortal : BlockBase
    {
        public BlockPortal()
        {
            Name = "Portal";
            Type = BlockData.Blocks.Portal;
            IsAir = true;
            Opacity = 0;
            Luminance = 0x11;
        }

    }
}
