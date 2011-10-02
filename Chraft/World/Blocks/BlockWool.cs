using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockWool : BlockBase
    {
        public BlockWool()
        {
            Name = "Wool";
            Type = BlockData.Blocks.Wool;
            IsSolid = true;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            LootTable.Add(new ItemStack((short)BlockData.Blocks.Wool, 1, block.MetaData));
            base.DropItems(entity, block);
        }
    }
}
