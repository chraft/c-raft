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
            DropItem = BlockData.Items.Ink_Sack;
            DropItemMeta = 4;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            DropItemAmount = (sbyte)(3 + block.World.Server.Rand.Next(17));
            base.DropItems(entity, block);
        }
    }
}
