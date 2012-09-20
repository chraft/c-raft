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
using Chraft.Entity;
using Chraft.Entity.Items;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockBrownMushroom : BlockBaseMushroom
    {
        public BlockBrownMushroom()
        {
            Name = "BrownMushroom";
            Type = BlockData.Blocks.Brown_Mushroom;
            var item = ItemHelper.GetInstance((short)Type);
            item.Count = 1;
            LootTable.Add(item);
        }

        public override void Fertilize(EntityBase entity, StructBlock block)
        {
            Chunk chunk = GetBlockChunk(block);

            if (chunk == null)
                return;

            BlockData.Blocks blockBelow = chunk.GetType(block.Coords.BlockX, block.Coords.BlockY - 1,
                                                                   block.Coords.BlockZ);

            if (blockBelow != BlockData.Blocks.Dirt && blockBelow != BlockData.Blocks.Grass && blockBelow != BlockData.Blocks.Mycelium)
                return;

            int stemHeight = block.World.Server.Rand.Next(3) + 4;
            int capY = block.Coords.WorldY + stemHeight + 1;
            if (capY > 127)
                return;

            for (int dY = block.Coords.WorldY + 1; dY < capY - 1; dY++)
            {
                BlockData.Blocks blockUp = chunk.GetType(block.Coords.BlockX, dY, block.Coords.BlockZ);

                if (blockUp != BlockData.Blocks.Air && blockUp != BlockData.Blocks.Leaves)
                    return;
            }

            int absdX, absdZ;
            byte? blockId;
            for (int dX = -3; dX < 4; dX++)
                for (int dZ = -3; dZ < 4; dZ++)
                {
                    absdX = Math.Abs(dX);
                    absdZ = Math.Abs(dZ);
                    if (absdX == 3 && absdZ == 3)
                        continue;
                    blockId = block.World.GetBlockId(block.Coords.WorldX + dX, capY, block.Coords.WorldZ + dZ);
                    if (blockId == null || (blockId != (byte)BlockData.Blocks.Air && blockId != (byte)BlockData.Blocks.Leaves))
                        return;
                }
            

            byte metaData = (byte)MetaData.HugeMushroom.NorthWeastSouthEast;
            for (int dY = block.Coords.WorldY; dY < capY; dY++)                           
                if (chunk.GetType(block.Coords.BlockX, dY, block.Coords.BlockZ) != BlockData.Blocks.Leaves)
                    chunk.SetBlockAndData(block.Coords.BlockX, dY, block.Coords.BlockZ, (byte) BlockData.Blocks.BrownMushroomCap, metaData);

            for (int dX = -3; dX < 4; dX++)
                for (int dZ = -3; dZ < 4; dZ++)
                {
                    Chunk currentChunk = block.World.GetChunkFromWorld(block.Coords.WorldX + dX, block.Coords.WorldZ + dZ) as Chunk;
                    if (currentChunk == null)
                        continue;

                    absdX = Math.Abs(dX);
                    absdZ = Math.Abs(dZ);
                    if (absdX == 3 && absdZ == 3)
                        continue;

                    BlockData.Blocks nearbyBlockId = currentChunk.GetType(block.Coords.BlockX + dX, capY, block.Coords.BlockZ + dZ);
                    if (nearbyBlockId == BlockData.Blocks.Leaves)
                        continue;

                    if (absdX < 3 && absdZ < 3)
                        metaData = (byte)MetaData.HugeMushroom.Top;
                    else if ((dX == -3 && dZ == -2) || (dZ == -3 && dX == -2))
                        metaData = (byte)MetaData.HugeMushroom.TopNorthWest;
                    else if ((dX == -3 && dZ == 2) || (dZ == 3 && dX == -2))
                        metaData = (byte)MetaData.HugeMushroom.TopSouthWest;
                    else if ((dX == 3 && dZ == -2) || (dZ == -3 && dX == 2))
                        metaData = (byte)MetaData.HugeMushroom.TopNorthEast;
                    else if ((dX == 3 && dZ == 2) || (dZ == 3 && dX == 2))
                        metaData = (byte)MetaData.HugeMushroom.TopSouthEast;
                    else if (dX == -3 && absdZ < 2)
                        metaData = (byte)MetaData.HugeMushroom.TopWest;
                    else if (dX == 3 && absdZ < 2)
                        metaData = (byte)MetaData.HugeMushroom.TopEast;
                    else if (dZ == -3 && absdX < 2)
                        metaData = (byte)MetaData.HugeMushroom.TopNorth;
                    else if (dZ == 3 && absdX < 2)
                        metaData = (byte)MetaData.HugeMushroom.TopSouth;

                    currentChunk.SetBlockAndData(block.Coords.BlockX + dX, capY, block.Coords.BlockZ + dZ, (byte)BlockData.Blocks.BrownMushroomCap, metaData);
                }
        }
    }
}
