using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockPumpkin : BlockBase
    {
        public BlockPumpkin()
        {
            Name = "Pumpkin";
            Type = BlockData.Blocks.Pumpkin;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Pumpkin;
            DropBlockAmount = 1;
        }
    }
}
