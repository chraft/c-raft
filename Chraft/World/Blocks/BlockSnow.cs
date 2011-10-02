using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockSnow : BlockBase
    {
        public BlockSnow()
        {
            Name = "Snow";
            Type = BlockData.Blocks.Snow;
            IsAir = true;
            Opacity = 0x0;
            IsSolid = true;
        }

        protected override void  DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            Client client = entity as Client;
            if (client != null)
            {
                if (client.Inventory.ActiveItem.Type == (short)BlockData.Items.Wooden_Spade ||
                    client.Inventory.ActiveItem.Type == (short)BlockData.Items.Stone_Spade ||
                    client.Inventory.ActiveItem.Type == (short)BlockData.Items.Iron_Spade ||
                    client.Inventory.ActiveItem.Type == (short)BlockData.Items.Gold_Spade ||
                    client.Inventory.ActiveItem.Type == (short)BlockData.Items.Diamond_Spade)
                {
                    LootTable.Add(new ItemStack((short)BlockData.Items.Snowball, 1));
                }
            }
            base.DropItems(entity, block);
        }
    }
}
