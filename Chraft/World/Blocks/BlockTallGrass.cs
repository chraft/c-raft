using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockTallGrass : BlockBase
    {
        public BlockTallGrass()
        {
            Name = "TallGrass";
            Type = BlockData.Blocks.TallGrass;
            IsSingleHit = true;
            IsAir = true;
            IsSolid = true;
            Opacity = 0x0;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            LootTable.Add(new ItemStack((short)Type, 1, block.MetaData));
            base.DropItems(entity, block);
        }

    }
}
