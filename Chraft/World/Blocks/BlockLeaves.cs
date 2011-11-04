using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockLeaves : BlockBase
    {
        public BlockLeaves()
        {
            Name = "Leaves";
            Type = BlockData.Blocks.Leaves;
            Opacity = 0x1;
            IsSolid = true;
            BurnEfficiency = 300;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            Player player = entity as Player;
            if (player != null)
            {
                if (player.Inventory.ActiveItem.Type == (short)BlockData.Items.Shears)
                    LootTable.Add(new ItemStack((short)Type, 1, block.MetaData));              
                else if (block.World.Server.Rand.Next(5) == 0)
                    LootTable.Add(new ItemStack((short)BlockData.Blocks.Sapling, 1));
            }
            base.DropItems(entity, block);
        }

    }
}
