using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockGravel : BlockBase
    {
        public BlockGravel()
        {
            Name = "Gravel";
            Type = BlockData.Blocks.Gravel;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Gravel;
            DropItem = BlockData.Items.Flint;
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Client client = entity as Client;
            if (client != null)
            {
                if (client.Inventory.ActiveItem.Type == (short)BlockData.Items.Wooden_Spade ||
                    client.Inventory.ActiveItem.Type == (short)BlockData.Items.Stone_Spade ||
                    client.Inventory.ActiveItem.Type == (short)BlockData.Items.Iron_Spade ||
                    client.Inventory.ActiveItem.Type == (short)BlockData.Items.Gold_Spade ||
                    client.Inventory.ActiveItem.Type == (short)BlockData.Items.Diamond_Spade)
                if (block.World.Server.Rand.Next(10) == 0)
                {
                    DropItemAmount = 1;
                    DropBlockAmount = 0;
                }
                else
                {
                    DropItemAmount = 0;
                    DropBlockAmount = 1;
                }
            }
            base.DropItems(entity, block);
        }
    }
}
