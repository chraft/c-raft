using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockLadder : BlockBase
    {
        public BlockLadder()
        {
            Name = "Ladder";
            Type = BlockData.Blocks.Ladder;
            IsAir = true;
            DropBlock = BlockData.Blocks.Ladder;
            DropBlockAmount = 1;
            Opacity = 0x0;
        }
    }
}
