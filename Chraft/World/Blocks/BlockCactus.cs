using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockCactus : BlockBase, IBlockGrowable
    {
        public BlockCactus()
        {
            Name = "Cactus";
            Type = BlockData.Blocks.Cactus;
            IsSolid = true;
            Opacity = 0x0;
            LootTable.Add(new ItemStack((short)Type, 1));
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            // Cactus can only be placed on the top of the sand block or on the top of the other cactus
            if ((targetBlock.Type != (byte)BlockData.Blocks.Sand && targetBlock.Type != (byte)BlockData.Blocks.Cactus) || targetSide != BlockFace.Up)
                return false;
            // Can be placed only if North/West/East/South is clear
            byte type1 = targetBlock.World.GetBlockId(targetBlock.X-1, targetBlock.Y + 1, targetBlock.Z);
            byte type2 = targetBlock.World.GetBlockId(targetBlock.X+1, targetBlock.Y + 1, targetBlock.Z);
            byte type3 = targetBlock.World.GetBlockId(targetBlock.X, targetBlock.Y + 1, targetBlock.Z-1);
            byte type4 = targetBlock.World.GetBlockId(targetBlock.X, targetBlock.Y + 1, targetBlock.Z+1);
            if (!targetBlock.World.BlockHelper.Instance(type1).IsAir || !targetBlock.World.BlockHelper.Instance(type2).IsAir ||
                !targetBlock.World.BlockHelper.Instance(type3).IsAir || !targetBlock.World.BlockHelper.Instance(type4).IsAir)
            {
                return false;
            }

            return base.CanBePlacedOn(who, block, targetBlock, targetSide);
        }

        public void Grow(StructBlock block)
        {
            // BlockMeta = 0x0 is a freshly planted cactus.
            // The data value is incremented at random intervals.
            // When it becomes 15, a new cactus block is created on top as long as the total height does not exceed 3.
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            base.DropItems(entity, block);

            // TODO: If the top block is a cactus as well - destroy it
        }
    }
}
