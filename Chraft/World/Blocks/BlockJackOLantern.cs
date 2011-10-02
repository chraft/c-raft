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
            LootTable.Add(new ItemStack((short)Type, 1));
            Luminance = 0xf;
        }
    }
}
