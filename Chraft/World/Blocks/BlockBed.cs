using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockBed : BlockBase
    {
        public BlockBed()
        {
            Name = "Bed";
            Type = BlockData.Blocks.Bed;
            BurnEfficiency = 300;
            LootTable.Add(new ItemStack((short)BlockData.Items.Bed, 1));
            BlockBoundsOffset = new BoundingBox(0, 0, 0, 1, 0.5625, 1);
        }
    }
}
