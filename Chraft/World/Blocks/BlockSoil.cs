using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockSoil : BlockBase
    {
        public BlockSoil()
        {
            Name = "Soil";
            Type = BlockData.Blocks.Soil;
            IsSolid = true;
            IsFertile = true;
            IsPlowed = true;
            LootTable.Add(new ItemStack((short)BlockData.Blocks.Dirt, 1));
        }

    }
}
