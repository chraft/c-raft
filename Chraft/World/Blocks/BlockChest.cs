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
using Chraft.Entity;
using Chraft.Entity.Items;
using Chraft.Net;
using Chraft.PluginSystem.Entity;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockChest : BlockBaseContainer
    {
        public BlockChest()
        {
            Name = "Chest";
            Type = BlockData.Blocks.Chest;
            var item = ItemHelper.GetInstance((short)Type);
            item.Count = 1;
            LootTable.Add(item);
            BurnEfficiency = 300;
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            Chunk chunk = GetBlockChunk(block);
            if (chunk == null)
                return false;

            bool isDoubleChestNearby = false;
            int chestCount = 0;
            chunk.ForNSEW(block.Coords, uc =>
            {
                byte? nearbyBlockId = block.World.GetBlockId(uc);

                if (nearbyBlockId == null)
                    return;

                // Cannot place next to a double chest
                if (nearbyBlockId == (byte)BlockData.Blocks.Chest)
                {
                    chestCount++;
                     if (chunk.IsNSEWTo(uc, (byte)BlockData.Blocks.Chest))
                        isDoubleChestNearby = true;
                }
            });

            if (isDoubleChestNearby || chestCount > 1)
                return false;
            return base.CanBePlacedOn(who, block, targetBlock, targetSide);
        }

        protected override byte GetDirection(LivingEntity living, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            Chunk chunk = GetBlockChunk(block);
            // Load the blocks surrounding the position (NSEW) not diagonals
            var nsewBlocks = new BlockData.Blocks[4];
            var nsewBlockPositions = new UniversalCoords[4];
            int nsewCount = 0;

            int secondChestIndex = -1;
            chunk.ForNSEW(block.Coords, uc =>
            {
                byte? nearbyBlockId = block.World.GetBlockId(uc);

                if (nearbyBlockId == null)
                    return;

                if (nearbyBlockId == (byte)BlockData.Blocks.Chest)
                    secondChestIndex = nsewCount;

                nsewBlocks[nsewCount] = (BlockData.Blocks)nearbyBlockId;
                nsewBlockPositions[nsewCount] = uc;
                nsewCount++;
            });
            byte direction = base.GetDirection(living, block, targetBlock, face);
            if (secondChestIndex != -1)
            {
                var secondChestCoords = nsewBlockPositions[secondChestIndex];
                byte secondChestDirection = chunk.GetData(secondChestCoords);
                if (secondChestDirection != direction)
                {
                    if (secondChestCoords.WorldX == block.Coords.WorldX)
                    {
                        if (direction != (byte)MetaData.Container.South && direction != (byte)MetaData.Container.North)
                            direction = (byte)MetaData.Container.South;                          
                    }
                    else
                    {
                        if (direction != (byte)MetaData.Container.East && direction != (byte)MetaData.Container.West)
                            direction = (byte)MetaData.Container.West;
                    }
                }
                else
                {

                    if (secondChestCoords.WorldX == block.Coords.WorldX && block.MetaData != (byte)MetaData.Container.North && block.MetaData != (byte)MetaData.Container.South)
                        direction = (byte)MetaData.Container.South;
                    else
                        if (block.MetaData != (byte)MetaData.Container.West && block.MetaData != (byte)MetaData.Container.East)
                            direction = (byte)MetaData.Container.West;
                }
            }

            return direction;
        }

        protected override void NotifyPlace(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if (sourceBlock.Type == (byte)BlockData.Blocks.Chest)
                targetBlock.World.SetBlockData(targetBlock.Coords, sourceBlock.MetaData);
            base.NotifyPlace(entity, sourceBlock, targetBlock);
        }

        public void Interact(IEntityBase entity, StructBlock block)
        {
            if (block.Coords.WorldY < 127)
            {
                // Cannot open a chest if no space is above it
                byte? blockId = block.World.GetBlockId(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
                if (blockId == null || !BlockHelper.Instance.IsAir((byte)blockId))
                    return;
            }
            base.Interact(entity, block);
        }
    }
}
