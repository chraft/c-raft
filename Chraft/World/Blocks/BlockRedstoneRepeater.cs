using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockRedstoneRepeater : BlockBase
    {
        public BlockRedstoneRepeater()
        {
            Name = "RedstoneRepeater";
            Type = BlockData.Blocks.Redstone_Repeater;
            Opacity = 0x0;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)BlockData.Items.Redstone_Repeater, 1));
        }
    }
}
