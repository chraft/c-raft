using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockJackOLantern : BlockBase
    {
        public BlockJackOLantern()
        {
            Name = "JackOLantern";
            Type = BlockData.Blocks.Jack_O_Lantern;
            DropBlock = BlockData.Blocks.Jack_O_Lantern;
            DropBlockAmount = 1;
            Luminance = 0xf;
        }
    }
}
