using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockStonePressurePlate : BlockBase
    {
        public BlockStonePressurePlate()
        {
            Name = "StonePressurePlate";
            Type = BlockData.Blocks.Stone_Pressure_Plate;
            IsAir = true;
            DropBlock = BlockData.Blocks.Stone_Pressure_Plate;
            DropBlockAmount = 1;
            Opacity = 0x0;
        }
    }
}
