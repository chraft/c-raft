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
            LootTable.Add(new ItemStack((short)BlockData.Blocks.Furnace, 1));
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            // You can not place the furnace that is already burning.
        }

        protected override void UpdateOnDestroy(StructBlock block)
        {
            FurnaceInterface.StopBurning(block.World, block.Coords);
            base.UpdateOnDestroy(block);
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player != null)
            {
                FurnaceInterface fi = new FurnaceInterface(block.World, block.Coords);
                fi.Associate(player);
                fi.DropAll(block.Coords);
                fi.Save();
            }
            base.DropItems(entity, block);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player == null)
                return;
            if (player.CurrentInterface != null)
                return;
            player.CurrentInterface = new FurnaceInterface(block.World, block.Coords);
            player.CurrentInterface.Associate(player);
            player.CurrentInterface.Open();
        }

    }
}
