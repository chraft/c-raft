using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockBurningFurnace : BlockBase, IBlockInteractive
    {
        public BlockBurningFurnace()
        {
            Name = "BurningFurnace";
            Type = BlockData.Blocks.Burning_Furnace;
            IsSolid = true;
            DropBlock = BlockData.Blocks.Furnace;
            DropBlockAmount = 1;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            // You can not place the furnace that is already burning.
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Client client = entity as Client;
            if (client != null)
            {
                FurnaceInterface fi = new FurnaceInterface(block.World, block.X, block.Y, block.Z);
                fi.Associate(client);
                fi.DropAll(block.X, block.Y, block.Z);
                fi.Save();
            }
            base.DropItems(entity, block);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            Client client = entity as Client;
            if (client == null)
                return;
            if (client.CurrentInterface != null)
                return;
            client.CurrentInterface = new FurnaceInterface(block.World, block.X, block.Y, block.Z);
            client.CurrentInterface.Associate(client);
            client.CurrentInterface.Open();
        }

    }
}
