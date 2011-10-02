using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockLapisLazuliOre : BlockBase
    {
        public BlockLapisLazuliOre()
        {
            Name = "LapisLazuliOre";
            Type = BlockData.Blocks.Lapis_Lazuli_Ore;
            IsSolid = true;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            LootTable.Add(new ItemStack((short)BlockData.Items.Ink_Sack, (sbyte)(3 + block.World.Server.Rand.Next(17)), 4));
            base.DropItems(entity, block);
        }
    }
}
