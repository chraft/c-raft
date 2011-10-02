using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockRedstoneOre : BlockBase
    {
        public BlockRedstoneOre()
        {
            Name = "RedstoneOre";
            Type = BlockData.Blocks.Redstone_Ore;
            IsSolid = true;
            Luminance = 0x9;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            LootTable.Add(new ItemStack((short)BlockData.Items.Redstone, (sbyte)(2 + block.World.Server.Rand.Next(4))));
            base.DropItems(entity, block);
        }
    }
}
