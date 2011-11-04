#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
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
            LootTable.Add(new ItemStack((short)Type, 1));
            BurnEfficiency = 300;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            Chunk chunk = GetBlockChunk(block);
            Chunk targetChunk = GetBlockChunk(targetBlock);

            if (chunk == null || targetChunk == null)
                return;

            // Load the blocks surrounding the position (NSEW) not diagonals
            BlockData.Blocks[] nsewBlocks = new BlockData.Blocks[4];
            UniversalCoords[] nsewBlockPositions = new UniversalCoords[4];
            int nsewCount = 0;

            chunk.ForNSEW(block.Coords, (uc) =>
            {
                byte? nearbyBlockId = block.World.GetBlockId(uc);

                if(nearbyBlockId == null)
                    return;

                nsewBlocks[nsewCount] = (BlockData.Blocks)nearbyBlockId;
                nsewBlockPositions[nsewCount] = uc;
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
                UniversalCoords p = nsewBlockPositions[i];
                if (nsewBlocks[i] == BlockData.Blocks.Chest && chunk.IsNSEWTo(p, (byte)BlockData.Blocks.Chest))
                {
                    // Cannot place next to a double chest
                    return;
                }
            }
            base.Place(entity, block, targetBlock, face);
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player != null)
            {
                SmallChestInterface sci = new SmallChestInterface(block.World, block.Coords);
                sci.Associate(player);
                sci.DropAll(block.Coords);
                sci.Save();
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

            byte? blockId = block.World.GetBlockId(block.Coords);

            if (blockId == null || !BlockHelper.Instance((byte)blockId).IsAir)
            {
                // Cannot open a chest if no space is above it
                return;
            }

            Chunk chunk = GetBlockChunk(block);

            // Double chest?
            if (chunk.IsNSEWTo(block.Coords, block.Type))
            {
                // Is this chest the "North or East", or the "South or West"
                BlockData.Blocks[] nsewBlocks = new BlockData.Blocks[4];
                UniversalCoords[] nsewBlockPositions = new UniversalCoords[4];
                int nsewCount = 0;
                chunk.ForNSEW(block.Coords, (uc) =>
                {
                    nsewBlocks[nsewCount] = (BlockData.Blocks)block.World.GetBlockId(uc);
                    nsewBlockPositions[nsewCount] = uc;
                    nsewCount++;
                });

                if ((byte)nsewBlocks[0] == block.Type) // North
                {
                    player.CurrentInterface = new LargeChestInterface(block.World, nsewBlockPositions[0], block.Coords);
                }
                else if ((byte)nsewBlocks[2] == block.Type) // East
                {
                    player.CurrentInterface = new LargeChestInterface(block.World, nsewBlockPositions[2], block.Coords);
                }
                else if ((byte)nsewBlocks[1] == block.Type) // South
                {
                    player.CurrentInterface = new LargeChestInterface(block.World, block.Coords, nsewBlockPositions[1]);
                }
                else if ((byte)nsewBlocks[3] == block.Type) // West
                {
                    player.CurrentInterface = new LargeChestInterface(block.World, block.Coords, nsewBlockPositions[3]);
                }
            }
            else
            {
                player.CurrentInterface = new SmallChestInterface(block.World, block.Coords);
            }

            player.CurrentInterface.Associate(player);
            player.CurrentInterface.Open();
        }

    }
}
