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
using Chraft.Interfaces.Containers;
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
            LivingEntity living = entity as LivingEntity;
            if (living == null)
                return;

            Chunk chunk = GetBlockChunk(block);
            Chunk targetChunk = GetBlockChunk(targetBlock);

            if (chunk == null || targetChunk == null)
                return;

            switch (face) //Bugged for double chests!
            {
                case BlockFace.East:
                    block.MetaData = (byte)MetaData.Container.East;
                    break;
                case BlockFace.West:
                    block.MetaData = (byte)MetaData.Container.West;
                    break;
                case BlockFace.North:
                    block.MetaData = (byte)MetaData.Container.North;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte)MetaData.Container.South;
                    break;
                default:
                    switch (living.FacingDirection(4)) // Built on floor, set by facing dir
                    {
                        case "N":
                            block.MetaData = (byte)MetaData.Container.North;
                            break;
                        case "W":
                            block.MetaData = (byte)MetaData.Container.West;
                            break;
                        case "S":
                            block.MetaData = (byte)MetaData.Container.South;
                            break;
                        case "E":
                            block.MetaData = (byte)MetaData.Container.East;
                            break;
                        default:
                            return;

                    }
                    break;
            }

            // Load the blocks surrounding the position (NSEW) not diagonals
            BlockData.Blocks[] nsewBlocks = new BlockData.Blocks[4];
            UniversalCoords[] nsewBlockPositions = new UniversalCoords[4];
            int nsewCount = 0;

            chunk.ForNSEW(block.Coords, uc =>
            {
                byte? nearbyBlockId = block.World.GetBlockId(uc);

                if(nearbyBlockId == null)
                    return;

                nsewBlocks[nsewCount] = (BlockData.Blocks)nearbyBlockId;
                nsewBlockPositions[nsewCount] = uc;
                nsewCount++;
            });

            int count = 0;
            int secondChest = -1;
            for (int i = 0; i < 4; i++)
            {
                UniversalCoords p = nsewBlockPositions[i];
                if (nsewBlocks[i] == BlockData.Blocks.Chest)
                {
                    count++;
                    if (chunk.IsNSEWTo(p, (byte)BlockData.Blocks.Chest) || count > 1)
                    {
                        // Cannot place next to a double chest
                        return;
                    }
                    secondChest = i;
                }
            }
            if (secondChest != -1)
            {
                chunk.SetData(nsewBlockPositions[secondChest], block.MetaData);
            }

            base.Place(entity, block, targetBlock, face);
        }

        protected override void UpdateOnDestroy(StructBlock block)
        {
            ContainerFactory.Destroy(block.World, block.Coords);
            base.UpdateOnDestroy(block);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player == null)
                return;
            if (player.CurrentInterface != null)
                return;
            if (block.Coords.WorldY < 127)
            {
                // Cannot open a chest if no space is above it
                byte? blockId = block.World.GetBlockId(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
                if (blockId == null || !BlockHelper.Instance((byte)blockId).IsAir)
                    return;
            }
            ContainerFactory.Open(player, block.Coords);
        }

    }
}
