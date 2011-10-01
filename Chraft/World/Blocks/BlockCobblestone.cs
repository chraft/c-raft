using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockCobblestone : BlockBase
    {
        public BlockCobblestone()
        {
            Name = "Cobblestone";
            Type = BlockData.Blocks.Cobblestone;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Cobblestone;
            DropBlockAmount = 1;
        }
    }
}
