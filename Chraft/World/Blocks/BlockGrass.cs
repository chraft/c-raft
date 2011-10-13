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

        public bool CanGrow(StructBlock block)
        {
            bool canGrow = false;
            if (block.Coords.WorldY < 127)
            {
                UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                                  block.Coords.WorldZ);
                byte blockAboveId = block.World.GetBlockId(oneUp);
                byte blockAboveLight = block.Chunk.GetBlockLight(oneUp);
                if ((blockAboveLight < 4 && BlockHelper.Instance(blockAboveId).Opacity > 2) || blockAboveLight >= 9)
                    canGrow = true;
            }
            else
            {
                canGrow = true;
            }

            return canGrow;
        }

        public void Grow(StructBlock block)
        {
            if (!CanGrow(block))
                return;

            UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
            byte blockAboveId = block.World.GetBlockId(oneUp);
            byte blockAboveLight = block.Chunk.GetBlockLight(oneUp);
            if (blockAboveLight < 4 && BlockHelper.Instance(blockAboveId).Opacity > 2)
            {
                if (block.World.Server.Rand.Next(3) == 0)
                {
                    block.World.SetBlockAndData(block.Coords, (byte)BlockData.Blocks.Dirt, 0);
                }
                return;
            }

            if (blockAboveLight >= 9)
            {
                int x = block.Coords.WorldX + block.World.Server.Rand.Next(2) - 1;
                int y = block.Coords.WorldY + block.World.Server.Rand.Next(4) - 3;
                int z = block.Coords.WorldZ + block.World.Server.Rand.Next(2) - 1;
                byte newBlockId = block.World.GetBlockId(x, y, z);
                if (newBlockId != (byte)BlockData.Blocks.Dirt)
                    return;

                byte newBlockAboveLight = block.World.GetBlockLight(x, y + 1, z);
                if (newBlockAboveLight >= 4 && BlockHelper.Instance(newBlockId).Opacity <= 2)
                    block.World.SetBlockAndData(x, y, z, (byte)BlockData.Blocks.Grass, 0);
            }
        }
    }
}