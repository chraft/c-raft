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
    class BlockGrass : BlockBase, IBlockGrowable
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
            // Grass become Dirt after some time if something solid is on top of it
            bool isAir = true;
            if (block.Coords.WorldY < 127)
            {
                UniversalCoords blockAbove = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                                       block.Coords.BlockZ);
                isAir = block.World.BlockHelper.Instance(block.World.GetBlockId(blockAbove)).IsAir;
            }
            return isAir;
        }

        public void Grow(StructBlock block)
        {
            if (!CanGrow(block))
                return;

            if (block.World.Server.Rand.Next(10) == 0)
            {
                block.World.SetBlockAndData(block.Coords, (byte)BlockData.Blocks.Dirt, 0);
            }
        }
    }
}
