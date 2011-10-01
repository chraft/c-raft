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
    class BlockChest : BlockBase
    {
        public BlockChest()
        {
            Name = "Chest";
            Type = BlockData.Blocks.Chest;
            DropBlock = BlockData.Blocks.Chest;
            DropBlockAmount = 1;
            BurnEfficiency = 300;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            // Load the blocks surrounding the position (NSEW) not diagonals
            BlockData.Blocks[] nsewBlocks = new BlockData.Blocks[4];
            PointI[] nsewBlockPositions = new PointI[4];
            int nsewCount = 0;
            block.Chunk.ForNSEW(block.X & 0xf, block.Y, block.Z & 0xf, (x1, y1, z1) =>
            {
                nsewBlocks[nsewCount] = (BlockData.Blocks)block.World.GetBlockId(x1, y1, z1);
                nsewBlockPositions[nsewCount] = new PointI(x1, y1, z1);
                nsewCount++;
            });

            // Count chests in list
            if (nsewBlocks.Where((b) => b == BlockData.Blocks.Chest).Count() > 1)
            {
                // Cannot place next to two chests
                return;
            }

            for (int i = 0; i < 4; i++)
            {
                PointI p = nsewBlockPositions[i];
                if (nsewBlocks[i] == BlockData.Blocks.Chest && block.Chunk.IsNSEWTo(p.X & 0xf, p.Y, p.Z & 0xf, (byte)BlockData.Blocks.Chest))
                {
                    // Cannot place next to a double chest
                    return;
                }
            }
            base.Place(entity, block, targetBlock, face);
        }

    }
}
