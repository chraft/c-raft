using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockBrownMushroomCap : BlockBase
    {
        public BlockBrownMushroomCap()
        {
            Name = "BrownMushroomCap";
            Type = BlockData.Blocks.BrownMushroomCap;
            IsSolid = true;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            int amount = block.World.Server.Rand.Next(10) - 7;
            if (amount > 0)
                LootTable.Add(new ItemStack((short)BlockData.Blocks.Brown_Mushroom, (sbyte)amount));
            base.DropItems(entity, block);
        }
    }
}
