using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;

namespace Chraft.World.Blocks
{
    class BlockTallGrass : BlockBase
    {
        public BlockTallGrass()
        {
            Name = "TallGrass";
            Type = BlockData.Blocks.TallGrass;
            IsSingleHit = true;
            IsAir = true;
            IsSolid = true;
            Opacity = 0x0;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            if (face == BlockFace.Down)
                return;
            byte blockId = targetBlock.World.GetBlockId(block.X, block.Y-1, block.Z);
            // We can place the tall grass only on the fertile blocks - dirt, soil, grass)
            if (!targetBlock.World.BlockHelper.Instance(blockId).IsFertile)
                return;
            base.Place(entity, block, targetBlock, face);
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            Client client = entity as Client;
            if (client != null)
            {
                // If hit by a shear - drop the grass
                if (client.Inventory.ActiveItem.Type == (short)BlockData.Items.Shears)
                {
                    LootTable.Add(new ItemStack((short) Type, 1, block.MetaData));
                }
                else
                {
                    // Chance of dropping seeds, 25% ?
                    if (client.Server.Rand.Next(3) == 0)
                        LootTable.Add(new ItemStack((short)BlockData.Items.Seeds, 1));
                }
            }
            base.DropItems(entity, block);
        }

    }
}
