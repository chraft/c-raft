using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockDiamondOre : BlockBase
    {
        public BlockDiamondOre()
        {
            Name = "DiamondOre";
            Type = BlockData.Blocks.Diamond_Ore;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)BlockData.Items.Diamond, 1));
        }
    }
}
