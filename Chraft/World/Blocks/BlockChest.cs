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
    class BlockChest : BlockBase, IBlockInteractive
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

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Client client = entity as Client;
            if (client != null)
            {
                SmallChestInterface sci = new SmallChestInterface(block.World, block.X, block.Y, block.Z);
                sci.Associate(client);
                sci.DropAll(block.X, block.Y, block.Z);
                sci.Save();
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

            if (!block.World.BlockHelper.Instance(block.World.GetBlockId(block.X, block.Y, block.Z)).IsAir)
            {
                // Cannot open a chest if no space is above it
                return;
            }

            Chunk chunk = client.World.GetBlockChunk(block.X, block.Y, block.Z);

            // Double chest?
            // TODO: simplify chunk API so that no bit shifting is required
            if (chunk.IsNSEWTo(block.X & 0xf, block.Y, block.Z & 0xf, block.Type))
            {
                // Is this chest the "North or East", or the "South or West"
                BlockData.Blocks[] nsewBlocks = new BlockData.Blocks[4];
                PointI[] nsewBlockPositions = new PointI[4];
                int nsewCount = 0;
                chunk.ForNSEW(block.X & 0xf, block.Y, block.Z & 0xf, (x1, y1, z1) =>
                {
                    nsewBlocks[nsewCount] = (BlockData.Blocks)block.World.GetBlockId(x1, y1, z1);
                    nsewBlockPositions[nsewCount] = new PointI(x1, y1, z1);
                    nsewCount++;
                });

                if ((byte)nsewBlocks[0] == block.Type) // North
                {
                    client.CurrentInterface = new LargeChestInterface(block.World, nsewBlockPositions[0], new PointI(block.X, block.Y, block.Z));
                }
                else if ((byte)nsewBlocks[2] == block.Type) // East
                {
                    client.CurrentInterface = new LargeChestInterface(block.World, nsewBlockPositions[2], new PointI(block.X, block.Y, block.Z));
                }
                else if ((byte)nsewBlocks[1] == block.Type) // South
                {
                    client.CurrentInterface = new LargeChestInterface(block.World, new PointI(block.X, block.Y, block.Z), nsewBlockPositions[1]);
                }
                else if ((byte)nsewBlocks[3] == block.Type) // West
                {
                    client.CurrentInterface = new LargeChestInterface(block.World, new PointI(block.X, block.Y, block.Z), nsewBlockPositions[3]);
                }
            }
            else
            {
                client.CurrentInterface = new SmallChestInterface(block.World, block.X, block.Y, block.Z);
            }

            client.CurrentInterface.Associate(client);
            client.CurrentInterface.Open();
        }

    }
}
