using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockRedstoneWire : BlockBase
    {
        public BlockRedstoneWire()
        {
            Name = "RedstoneWire";
            Type = BlockData.Blocks.Redstone_Wire;
            IsAir = true;
            IsSingleHit = true;
            LootTable.Add(new ItemStack((short)BlockData.Items.Redstone, 1));
            Opacity = 0x0;
        }
    }
}
