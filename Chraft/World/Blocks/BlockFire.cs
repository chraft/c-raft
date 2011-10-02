using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockFire : BlockBase
    {
        public BlockFire()
        {
            Name = "Fire";
            Type = BlockData.Blocks.Fire;
            IsAir = true;
            Opacity = 0x0;
            Luminance = 0xf;
        }
    }
}
