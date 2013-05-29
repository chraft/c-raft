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
using System.Linq;
using Chraft.Entity;
using Chraft.Entity.Items;
using Chraft.Interfaces;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.World;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Collision;
using Chraft.Utilities.Coords;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockCactus : BlockBase, IBlockGrowable
    {
        public BlockCactus()
        {
            Name = "Cactus";
            Type = BlockData.Blocks.Cactus;
            IsSolid = true;
            Opacity = 0x0;
            var item = ItemHelper.GetInstance((short)Type);
            item.Count = 1;
            LootTable.Add(item);
            BlockBoundsOffset = new BoundingBox(0.0625, 0, 0.0625, 0.9375, 0.9375, 0.9375);
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            // Cactus can only be placed on the top of the sand block or on the top of the other cactus
            if ((targetBlock.Type != (byte)BlockData.Blocks.Sand && targetBlock.Type != (byte)BlockData.Blocks.Cactus) || targetSide != BlockFace.Up)
                return false;
            // Can be placed only if North/West/East/South is clear
            bool isAir = true;

            Chunk chunk = GetBlockChunk(block);

            if (chunk == null)
                return false;

            chunk.ForNSEW(block.Coords,
                delegate(UniversalCoords uc)
                {
                    byte? blockId = block.World.GetBlockId(uc);
                    if (blockId == null || blockId != (byte)BlockData.Blocks.Air)
                        isAir = false;
                });
            if (!isAir)
                return false;

            return base.CanBePlacedOn(who, block, targetBlock, targetSide);
        }

        protected override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock thisBlock)
        {
            if ((thisBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                thisBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                thisBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(thisBlock);
            base.NotifyDestroy(entity, sourceBlock, thisBlock);
        }

        public bool CanGrow(IStructBlock block, IChunk iChunk)
        {
            Chunk chunk = iChunk as Chunk;
            if (chunk == null)
                return false;

            // BlockMeta = 0x0 is a freshly planted cactus.
            // The data value is incremented at random intervals.
            // When it becomes 15, a new cactus block is created on top as long as the total height does not exceed 3.
            int maxHeight = 3;

            if (block.Coords.WorldY == 127)
                return false;

            UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
            BlockData.Blocks blockId = chunk.GetType(oneUp);
            if (blockId != BlockData.Blocks.Air)
                return false;

            // Calculating the cactus length below this block
            int cactusHeightBelow = 0;
            for (int i = block.Coords.WorldY - 1; i >= 0; i--)
            {
                blockId = chunk.GetType(UniversalCoords.FromWorld(block.Coords.WorldX, i, block.Coords.WorldZ));
                if (blockId != BlockData.Blocks.Cactus)
                    break;
                cactusHeightBelow++;
            }

            if ((cactusHeightBelow + 1) >= maxHeight)
                return false;

            bool isAir = true;        

            chunk.ForNSEW(oneUp,
                delegate(UniversalCoords uc)
                {
                    byte? nearbyBlockId = block.WorldInterface.GetBlockId(uc);
                    if (nearbyBlockId == null || nearbyBlockId != (byte)BlockData.Blocks.Air)
                        isAir = false;
                });

            if (!isAir)
                return false;

            return true;
        }

        public void Grow(IStructBlock iBlock, IChunk ichunk)
        {
            Chunk chunk = (Chunk) ichunk;
            StructBlock block = (StructBlock) iBlock;
            if (!CanGrow(block, ichunk))
                return;

            UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);

            if (block.MetaData < 0xe) // 14
            {
                chunk.SetData(block.Coords, ++block.MetaData, false);
                return;
            }

            chunk.SetData(block.Coords, 0);
            StructBlock newCactus = new StructBlock(oneUp, (byte)Type, 0, block.World);
            Spawn(newCactus);
        }

        public override void Touch(IEntityBase ientity, IStructBlock iBlock, BlockFace face)
        {
            EntityBase entity = (EntityBase) ientity;
            if (!entity.Server.GetEntities().Contains(entity))
                return;
            if (entity is ItemEntity)
            {
                entity.Server.RemoveEntity(entity);
            }
            else if (entity is LivingEntity)
            {
                LivingEntity living = entity as LivingEntity;
                living.TouchedCactus();
            }
        }
    }
}
