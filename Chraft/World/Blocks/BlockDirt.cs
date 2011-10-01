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
    class BlockDirt : BlockBase
    {
        public BlockDirt()
        {
            Name = "Dirt";
            Type = BlockData.Blocks.Dirt;
            IsSolid = true;
            IsFertile = true;
            DropBlock = BlockData.Blocks.Dirt;
            DropBlockAmount = 1;
        }
    }
}
