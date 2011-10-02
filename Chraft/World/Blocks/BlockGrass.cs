using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockGrass : BlockBase
    {
        public BlockGrass()
        {
            Name = "Grass";
            Type = BlockData.Blocks.Grass;
            IsSolid = true;
            IsFertile = true;
            LootTable.Add(new ItemStack((short)BlockData.Blocks.Dirt, 1));
        }

    }
}
