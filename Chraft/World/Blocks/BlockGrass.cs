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
    class BlockGrass : BlockBase
    {
        public BlockGrass()
        {
            Name = "Grass";
            Type = BlockData.Blocks.Grass;
            IsSolid = true;
            IsFertile = true;
            LootTable.Add(new ItemStack((short)BlockData.Blocks.Dirt, 1));
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
                if (blockAboveLight != null && ((blockAboveLight < 4 && BlockHelper.Instance(blockAboveId).Opacity > 2) || blockAboveLight >= 9))
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

            UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
            byte blockAboveId = (byte)chunk.GetType(oneUp);
            byte? blockAboveLight = chunk.World.GetEffectiveLight(oneUp);
            if (blockAboveLight == null)
                return;
            if (blockAboveLight < 4 && BlockHelper.Instance(blockAboveId).Opacity > 2)
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

                Chunk nearbyChunk = block.World.GetChunkFromWorld(x, z, false, false);

                if (nearbyChunk == null)
                    return;

                byte newBlockId = (byte)nearbyChunk.GetType(x & 0xF, y, z & 0xF);
                if (newBlockId != (byte)BlockData.Blocks.Dirt)
                    return;

                byte? newBlockAboveLight = nearbyChunk.World.GetEffectiveLight(UniversalCoords.FromWorld(x, y + 1, z));
                if (newBlockAboveLight != null && (newBlockAboveLight >= 4 && BlockHelper.Instance(newBlockId).Opacity <= 2))
                    nearbyChunk.SetBlockAndData(x & 0xF, y, z & 0xF, (byte)BlockData.Blocks.Grass, 0);
            }
        }
    }
}
