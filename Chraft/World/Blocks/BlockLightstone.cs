using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockLightstone : BlockBase
    {
        public BlockLightstone()
        {
            Name = "Lightstone";
            Type = BlockData.Blocks.Lightstone;
            Luminance = 0xf;
            IsSolid = true;

        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            LootTable = new List<ItemStack>();
            if (player != null)
            {
                if (player.Inventory.ActiveItem.Type == (short) BlockData.Items.Wooden_Pickaxe ||
                    player.Inventory.ActiveItem.Type == (short) BlockData.Items.Stone_Pickaxe ||
                    player.Inventory.ActiveItem.Type == (short) BlockData.Items.Iron_Pickaxe ||
                    player.Inventory.ActiveItem.Type == (short) BlockData.Items.Gold_Pickaxe ||
                    player.Inventory.ActiveItem.Type == (short) BlockData.Items.Diamond_Pickaxe)
                {
                    LootTable.Add(new ItemStack((short)BlockData.Items.Lightstone_Dust, 1, (sbyte)(2 + block.World.Server.Rand.Next(2))));
                }
            }
            base.DropItems(entity, block);
        }
    }
}
