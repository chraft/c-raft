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
    class BlockReed : BlockBase, IBlockGrowable
    {
        public readonly int MaxHeight = 3;

        public BlockReed()
        {
            Name = "Reed";
            Type = BlockData.Blocks.Reed;
            Opacity = 0x0;
            IsAir = true;
            IsSolid = true;
            IsSingleHit = true;
            LootTable.Add(new ItemStack((short)BlockData.Items.Reeds, 1));
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            if (targetBlock.Type == (byte)BlockData.Blocks.Reed && targetSide == BlockFace.Up)
                return true;

            if ((targetBlock.Type != (byte)BlockData.Blocks.Sand &&
                targetBlock.Type != (byte)BlockData.Blocks.Dirt &&
                targetBlock.Type != (byte)BlockData.Blocks.Grass &&
                targetBlock.Type != (byte)BlockData.Blocks.Soil) || targetSide != BlockFace.Up)
                return false;

            bool isWater = false;
            block.Chunk.ForNSEW(targetBlock.Coords,
                delegate(UniversalCoords uc)
                {
                    if (block.World.GetBlockId(uc) == (byte)BlockData.Blocks.Water || block.World.GetBlockId(uc) == (byte)BlockData.Blocks.Still_Water)
                        isWater = true;
                });

            if (!isWater)
                return false;

            return base.CanBePlacedOn(who, block, targetBlock, targetSide);
        }

        public bool CanGrow(StructBlock block)
        {
            // Can't grow above the sky
            if (block.Coords.WorldY == 127)
                return false;

            // Can grow only if the block above is free
            byte blockId = block.World.GetBlockId(UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ));
            if (blockId != (byte)BlockData.Blocks.Air)
                return false;

            // MetaData = 0x0 is a freshly planted reed (sugar cane).
            // The data value is incremented randomly.
            // When it becomes 15, a new reed block is created on top as long as the total height does not exceed 3.

            // Calculating the reed length below this block
            int reedHeightBelow = 0;
            for (int i = block.Coords.WorldY - 1; i >= 0; i--)
            {
                if (block.World.GetBlockId(block.Coords.WorldX, i, block.Coords.WorldZ) != (byte)BlockData.Blocks.Reed)
                    break;
                reedHeightBelow++;
            }

            // If the total reed height is bigger than the maximal height - it'll not grow
            if ((reedHeightBelow + 1) >= MaxHeight)
                return false;

            // Checking if there are water next to the basement block
            bool isWater = false;
            blockId = 0;
            block.Chunk.ForNSEW(UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY - reedHeightBelow - 1, block.Coords.WorldZ),
                delegate(UniversalCoords uc)
                {
                    blockId = block.World.GetBlockId(uc);
                    if (blockId == (byte)BlockData.Blocks.Water || blockId == (byte)BlockData.Blocks.Still_Water)
                    {
                        isWater = true;
                    }
                });

            if (!isWater && reedHeightBelow < MaxHeight)
            {
                UniversalCoords baseBlock = UniversalCoords.FromWorld(block.Coords.WorldX,
                                                                      block.Coords.WorldY - reedHeightBelow,
                                                                      block.Coords.WorldZ);
                BlockHelper.Instance(block.Type).Destroy(new StructBlock(baseBlock, block.Type, block.MetaData, block.World));
                return false;
            }

            return true;
        }

        public void Grow(StructBlock block)
        {
            if (!CanGrow(block))
                return;

            if (block.MetaData < 0xe) // 14
            {
                block.World.SetBlockData(block.Coords, ++block.MetaData);
                return;
            }

            block.World.SetBlockData(block.Coords, 0);
            UniversalCoords blockAbove = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                                   block.Coords.WorldZ);
            block.World.SetBlockAndData(blockAbove, (byte)BlockData.Blocks.Reed, 0x0);
        }
    }
}