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
            LootTable.Add(new ItemStack((short)Type, 1));
            Opacity = 0x0;
        }


        protected override bool CanBePlacedOn(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            if (!targetBlock.World.BlockHelper.Instance(targetBlock.Type).IsFertile || targetSide != BlockFace.Up)
                return false;
            return base.CanBePlacedOn(entity, block, targetBlock, targetSide);
        }

        public void Grow(StructBlock block)
        {
            // Too high
/*            if (Y > 120)
                return;

            // Grow a trunk. Replace only the BlockAir
            for (int i = Y; i < Y + 5; i++)
            {
                if (Chunk.World.GetBlockId(X, i, Z) == (byte)BlockData.Blocks.Air)
                    WorldMgr.GetChunk(X, Z, false, true).ReplaceBlock(this, BlockData.Blocks.Log, BlockMeta);
                else
                    break;
            }

            // Grow leaves
            for (int i = Y + 2; i < Y + 5; i++)
                for (int j = X - 2; j <= X + 2; j++)
                    for (int k = Z - 2; k <= Z + 2; k++)
                        if (!WorldMgr.ChunkExists(j, k) || !(WorldMgr.GetBlock(j, i, k) is BlockAir))
                            continue;
                        else
                            WorldMgr.GetChunk(j, k, false, true).ReplaceBlock(this, BlockData.Blocks.Leaves, BlockMeta);

            for (int i = X - 1; i <= X + 1; i++)
                for (int j = Z - 1; j <= Z + 1; j++)
                    if (!WorldMgr.ChunkExists(i, j) || !(WorldMgr.GetBlock(i, Y + 5, j) is BlockAir))
                        continue;
                    else
                        WorldMgr.GetChunk(i, j, false, true).ReplaceBlock(this, BlockData.Blocks.Leaves, BlockMeta);

            foreach (Client c in WorldMgr.Server.GetNearbyPlayers(WorldMgr, X, Y, Z))
                c.SendBlockRegion(X - 3, Y, Z - 3, 7, 7, 7);
 */
        }
    }
}
