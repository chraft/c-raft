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
    class BlockSapling : BlockBase, IBlockGrowable
    {
        public BlockSapling()
        {
            Name = "Sapling";
            Type = BlockData.Blocks.Sapling;
            IsAir = true;
            IsSingleHit = true;
            BurnEfficiency = 100;
            Opacity = 0x0;
        }


        protected override bool CanBePlacedOn(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            if (!BlockHelper.Instance(targetBlock.Type).IsFertile || targetSide != BlockFace.Up)
                return false;
            return base.CanBePlacedOn(entity, block, targetBlock, targetSide);
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            LootTable = new List<ItemStack>();
            LootTable.Add(new ItemStack((short)Type, 1, block.MetaData));
            base.DropItems(entity, block);
        }

        public bool CanGrow(StructBlock block)
        {
            if (block.Coords.WorldY > 120)
                return false;
            /*UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                              block.Coords.WorldZ);
            byte lightUp = block.World.GetBlockData(oneUp);
            if (lightUp < 9)
                return false;*/
            return true;
        }

        public void Grow(StructBlock block)
        {
            if (!CanGrow(block))
                return;

            if (block.World.Server.Rand.Next(29) != 0)
                return;

            if ((block.MetaData & 8) == 0)
            {
                block.World.SetBlockData(block.Coords, (byte)(block.MetaData | 8));
                return;
            }

            for (int i = block.Coords.WorldY; i < block.Coords.WorldY + 4; i++)
            {
                block.World.SetBlockAndData(block.Coords.WorldX, i, block.Coords.WorldZ, (byte)BlockData.Blocks.Log, block.MetaData);
                if (block.World.GetBlockId(block.Coords.WorldX, i + 1, block.Coords.WorldZ) != (byte)BlockData.Blocks.Air)
                    break;
            }

            // Grow leaves
            for (int i = block.Coords.WorldY + 2; i < block.Coords.WorldY + 5; i++)
                for (int j = block.Coords.WorldX - 2; j <= block.Coords.WorldX + 2; j++)
                    for (int k = block.Coords.WorldZ - 2; k <= block.Coords.WorldZ + 2; k++)
                        if (!block.World.ChunkExists(j >> 4, k >> 4) || (block.World.GetBlockId(j, i, k) != (byte)BlockData.Blocks.Air))
                            continue;
                        else
                            block.World.SetBlockAndData(j, i, k, (byte)BlockData.Blocks.Leaves,
                                                        block.MetaData);

            for (int i = block.Coords.WorldX - 1; i <= block.Coords.WorldX + 1; i++)
                for (int j = block.Coords.WorldZ - 1; j <= block.Coords.WorldZ + 1; j++)
                    if (!block.World.ChunkExists(i >> 4, j >> 4) || (block.World.GetBlockId(i, block.Coords.WorldY + 5, j) != (byte)BlockData.Blocks.Air))
                        continue;
                    else
                        block.World.SetBlockAndData(i, block.Coords.WorldY + 5, j, (byte)BlockData.Blocks.Leaves,
                                                    block.MetaData);
            AbsWorldCoords absCoords = new AbsWorldCoords(block.Coords);
            foreach (Net.Client c in block.World.Server.GetNearbyPlayers(block.World, absCoords))
            {
                c.SendBlockRegion(block.Coords.WorldX - 3, block.Coords.WorldY, block.Coords.WorldZ - 3, 7, 7, 7);
            }
        }
    }
}