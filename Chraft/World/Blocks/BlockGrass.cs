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

using Chraft.Entity.Items;
using Chraft.Interfaces;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockGrass : BlockBase
    {
        public BlockGrass()
        {
            Name = "Grass";
            Type = BlockData.Blocks.Grass;
            IsSolid = true;
            IsFertile = true;
            var item = ItemHelper.GetInstance(BlockData.Blocks.Dirt);
            item.Count = 1;
            LootTable.Add(item);
        }

        public bool CanGrow(StructBlock block, Chunk chunk)
        {
            if (chunk == null)
                return false;

            bool canGrow = false;

            if (block.Coords.WorldY < 127)
            {
                UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                                  block.Coords.WorldZ);

                byte blockAboveId = (byte)chunk.GetType(oneUp);
                byte? blockAboveLight = chunk.World.GetEffectiveLight(oneUp);
                if (blockAboveLight != null && ((blockAboveLight < 4 && BlockHelper.Instance.CreateBlockInstance(blockAboveId).Opacity > 2) || blockAboveLight >= 9))
                    canGrow = true;
            }
            else
            {
                canGrow = true;
            }

            return canGrow;
        }

        public void Grow(StructBlock block, Chunk chunk)
        {
            if (!CanGrow(block, chunk))
                return;

            var oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
            byte blockAboveId = (byte)chunk.GetType(oneUp);
            byte? blockAboveLight = chunk.World.GetEffectiveLight(oneUp);
            if (blockAboveLight == null)
                return;
            if (blockAboveLight < 4 && BlockHelper.Instance.CreateBlockInstance(blockAboveId).Opacity > 2)
            {
                if (block.World.Server.Rand.Next(3) == 0)
                {
                    chunk.SetBlockAndData(block.Coords, (byte)BlockData.Blocks.Dirt, 0);
                }
                return;
            }

            if (blockAboveLight >= 9)
            {
                int x = block.Coords.WorldX + block.World.Server.Rand.Next(2) - 1;
                int y = block.Coords.WorldY + block.World.Server.Rand.Next(4) - 3;
                int z = block.Coords.WorldZ + block.World.Server.Rand.Next(2) - 1;

                var nearbyChunk = block.World.GetChunkFromWorld(x, z) as Chunk;

                if (nearbyChunk == null)
                    return;

                byte newBlockId = (byte)nearbyChunk.GetType(x & 0xF, y, z & 0xF);
                if (newBlockId != (byte)BlockData.Blocks.Dirt)
                    return;

                byte? newBlockAboveLight = nearbyChunk.World.GetEffectiveLight(UniversalCoords.FromWorld(x, y + 1, z));
                if (newBlockAboveLight != null && (newBlockAboveLight >= 4 && BlockHelper.Instance.CreateBlockInstance(newBlockId).Opacity <= 2))
                    nearbyChunk.SetBlockAndData(x & 0xF, y, z & 0xF, (byte)BlockData.Blocks.Grass, 0);
            }
        }
    }
}
