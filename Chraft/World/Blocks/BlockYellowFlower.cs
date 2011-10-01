using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockYellowFlower : BlockBase
    {
        public BlockYellowFlower()
        {
            Name = "YellowFlower";
            Type = BlockData.Blocks.Yellow_Flower;
            IsAir = true;
            DropBlock = BlockData.Blocks.Yellow_Flower;
            DropBlockAmount = 1;
            Opacity = 0x0;
        }
    }
}
