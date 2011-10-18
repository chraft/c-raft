using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockRedMushroomCap : BlockBase
    {
        public BlockRedMushroomCap()
        {
            Name = "RedMushroomCap";
            Type = BlockData.Blocks.RedMushroomCap;
            IsSolid = true;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            int amount = block.World.Server.Rand.Next(10) - 7;
            if (amount > 0)
                LootTable.Add(new ItemStack((short)BlockData.Blocks.Red_Mushroom, (sbyte)amount));
            base.DropItems(entity, block);
        }
    }
}
