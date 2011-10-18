using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockTracks : BlockBase
    {
        public BlockTracks()
        {
            Name = "Tracks";
            Type = BlockData.Blocks.Tracks;
            IsAir = true;
            LootTable.Add(new ItemStack((short)Type, 1));
            Opacity = 0x0;
            BlockBoundsOffset = new BoundingBox(0, 0, 0, 1, 0.125, 1);
        }
    }
}
