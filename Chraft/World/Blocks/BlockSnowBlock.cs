using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockSnowBlock : BlockBase
    {
        public BlockSnowBlock()
        {
            Name = "SnowBlock";
            Type = BlockData.Blocks.Snow_Block;
            IsSolid = true;
            DropItem = BlockData.Items.Snowball;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            // SnowBlock requires 9 snowballs to craft and drops 4-6 snowballs upon destruction.
            // No tools required.
            DropItemAmount = (sbyte)(4 + block.World.Server.Rand.Next(2));
            base.DropItems(entity, block);
        }
    }
}
