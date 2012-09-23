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
using System.Collections.Generic;
using Chraft.Entity;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.PluginSystem.World;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Collision;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks.Base;

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
            BlockBoundsOffset = new BoundingBox(0.1, 0, 0.1, 0.9, 0.8, 0.9);
        }


        protected override bool CanBePlacedOn(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            if (!BlockHelper.Instance.IsFertile(targetBlock.Type) || targetSide != BlockFace.Up)
                return false;
            return base.CanBePlacedOn(entity, block, targetBlock, targetSide);
        }

        protected override void DropItems(EntityBase entity, StructBlock block, List<ItemInventory> overridedLoot = null)
        {
            overridedLoot = new List<ItemInventory>();
            var item = ItemHelper.GetInstance((short) Type);
            item.Count = 1;
            item.Durability = block.MetaData;
            overridedLoot.Add(item);
            base.DropItems(entity, block, overridedLoot);
        }

        public bool CanGrow(IStructBlock block, IChunk chunk)
        {
            if (chunk == null || block.Coords.WorldY > 120)
                return false;
            /*UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                              block.Coords.WorldZ);
            byte lightUp = block.World.GetBlockData(oneUp);
            if (lightUp < 9)
                return false;*/
            return true;
        }

        public void Grow(IStructBlock iBlock, IChunk ichunk)
        {
            var chunk = ichunk as Chunk;

            var block = (StructBlock) iBlock;

            if (!CanGrow(block, chunk))
                return;

            var blockUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
            if (block.World.GetEffectiveLight(blockUp) < 9)
                return;

            if (block.World.Server.Rand.Next(29) != 0)
                return;

            if ((block.MetaData & 8) == 0)
            {
                chunk.SetData(block.Coords, (byte)(block.MetaData | 8));
                return;
            }

            for (int i = block.Coords.WorldY; i < block.Coords.WorldY + 4; i++)
            {
                chunk.SetBlockAndData(block.Coords.BlockX, i, block.Coords.BlockZ, (byte)BlockData.Blocks.Wood, block.MetaData);
                if(chunk.GetType(block.Coords.BlockX, i + 1, block.Coords.BlockZ) != BlockData.Blocks.Air)
                    break;
            }

            // Grow leaves
            for (int i = block.Coords.WorldY + 2; i < block.Coords.WorldY + 5; i++)
                for (int j = block.Coords.WorldX - 2; j <= block.Coords.WorldX + 2; j++)
                    for (int k = block.Coords.WorldZ - 2; k <= block.Coords.WorldZ + 2; k++)
                    {
                        var nearbyChunk = block.World.GetChunkFromWorld(i, k) as Chunk;
                        if (nearbyChunk == null || (nearbyChunk.GetType(j & 0xF, i, k & 0xF) != BlockData.Blocks.Air))
                            continue;


                        nearbyChunk.SetBlockAndData(j & 0xF, i, k & 0xF, (byte)BlockData.Blocks.Leaves,
                                                        block.MetaData);
                    }

            for (int i = block.Coords.WorldX - 1; i <= block.Coords.WorldX + 1; i++)
                for (int j = block.Coords.WorldZ - 1; j <= block.Coords.WorldZ + 1; j++)
                {
                    var nearbyChunk = block.World.GetChunkFromWorld(i, j) as Chunk;
                    if (nearbyChunk == null || nearbyChunk.GetType(i & 0xF, block.Coords.WorldY + 5, j & 0xF) != BlockData.Blocks.Air)
                        continue;


                    nearbyChunk.SetBlockAndData(i & 0xF, block.Coords.WorldY + 5, j & 0xF, (byte)BlockData.Blocks.Leaves,
                                                    block.MetaData);
                }
        }
    }
}