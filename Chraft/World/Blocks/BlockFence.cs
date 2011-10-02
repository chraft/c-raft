using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockFence : BlockBase
    {
        public BlockFence()
        {
            Name = "Fence";
            Type = BlockData.Blocks.Fence;
            IsAir = true;
            LootTable.Add(new ItemStack((short)Type, 1));
            BurnEfficiency = 300;
            Opacity = 0x0;
        }
    }
}
