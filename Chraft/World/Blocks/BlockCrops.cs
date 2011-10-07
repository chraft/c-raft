using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockCrops : BlockBase, IBlockGrowable
    {
        public BlockCrops()
        {
            Name = "Crops";
            Type = BlockData.Blocks.Crops;
            IsAir = true;
            IsSingleHit = true;
            Opacity = 0x0;
        }

        protected override void DropItems(EntityBase who, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            // TODO: Fully grown drops 1 Wheat & 0-3 Seeds. 0 seeds - very rarely
            if (block.MetaData == 7)
            {
                LootTable.Add(new ItemStack((short)BlockData.Items.Wheat, 1));
                sbyte seeds = (sbyte)block.World.Server.Rand.Next(3);
                if (seeds > 0)
                    LootTable.Add(new ItemStack((short)BlockData.Items.Seeds, seeds));
            }
            else if (block.MetaData >= 5)
            {
                sbyte seeds = (sbyte)block.World.Server.Rand.Next(3);
                if (seeds > 0)
                    LootTable.Add(new ItemStack((short)BlockData.Items.Seeds, seeds));
            }
            base.DropItems(who, block);
        }

        public void Grow(StructBlock block)
        {
            // Crops grow from 0x0 to 0x7
            if (block.MetaData == 0x07)
                return;
            // TODO: Check if the water within 4 blocks on the same horizontal level and grow faster?
            if (block.World.Server.Rand.Next(10) == 0)
            {
                block.World.SetBlockData(block.Coords, block.MetaData++);
            }
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            if (!targetBlock.World.BlockHelper.Instance(targetBlock.Type).IsPlowed || targetSide != BlockFace.Up)
                return false;
            return base.CanBePlacedOn(who, targetBlock, targetBlock, targetSide);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if (targetBlock.Coords.WorldY > sourceBlock.Coords.WorldY)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }
    }
}
