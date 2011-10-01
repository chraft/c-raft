using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockSandstone : BlockBase
    {
        public BlockSandstone()
        {
            Name = "Sandstone";
            Type = BlockData.Blocks.Sandstone;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Sandstone;
            DropBlockAmount = 1;
        }
    }
}
