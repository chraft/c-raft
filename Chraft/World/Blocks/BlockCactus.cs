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
    class BlockCactus : BlockBase, IBlockGrowable
    {
        public BlockCactus()
        {
            Name = "Cactus";
            Type = BlockData.Blocks.Cactus;
            IsSolid = true;
            Opacity = 0x0;
            LootTable.Add(new ItemStack((short)Type, 1));
            BlockBoundsOffset = new BoundingBox(0.0625, 0, 0.0625, 0.9375, 0.9375, 0.9375);
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            // Cactus can only be placed on the top of the sand block or on the top of the other cactus
            if ((targetBlock.Type != (byte)BlockData.Blocks.Sand && targetBlock.Type != (byte)BlockData.Blocks.Cactus) || targetSide != BlockFace.Up)
                return false;
            // Can be placed only if North/West/East/South is clear
            bool isAir = true;
            block.Chunk.ForNSEW(block.Coords,
                delegate(UniversalCoords uc)
                {
                    if (block.World.GetBlockId(uc) != (byte)BlockData.Blocks.Air)
                        isAir = false;
                });
            if (!isAir)
                return false;

            return base.CanBePlacedOn(who, block, targetBlock, targetSide);
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock thisBlock)
        {
            if ((thisBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                thisBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                thisBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(thisBlock);
            base.NotifyDestroy(entity, sourceBlock, thisBlock);
        }

        public bool CanGrow(StructBlock block)
        {
            // BlockMeta = 0x0 is a freshly planted cactus.
            // The data value is incremented at random intervals.
            // When it becomes 15, a new cactus block is created on top as long as the total height does not exceed 3.
            int maxHeight = 3;

            if (block.Coords.WorldY == 127)
                return false;

            UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);
            byte blockId = block.World.GetBlockId(oneUp);
            if (blockId != (byte)BlockData.Blocks.Air)
                return false;

            // Calculating the cactus length below this block
            int cactusHeightBelow = 0;
            for (int i = block.Coords.WorldY - 1; i >= 0; i--)
            {
                if (block.World.GetBlockId(UniversalCoords.FromWorld(block.Coords.WorldX, i, block.Coords.WorldZ)) != (byte)BlockData.Blocks.Cactus)
                    break;
                cactusHeightBelow++;
            }

            if ((cactusHeightBelow + 1) >= maxHeight)
                return false;

            bool isAir = true;
            block.Chunk.ForNSEW(oneUp,
                delegate(UniversalCoords uc)
                {
                    if (block.World.GetBlockId(uc) != (byte)BlockData.Blocks.Air)
                        isAir = false;
                });

            if (!isAir)
                return false;

            return true;
        }

        public void Grow(StructBlock block)
        {
            if (!CanGrow(block))
                return;

            UniversalCoords oneUp = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1, block.Coords.WorldZ);

            if (block.MetaData < 0xe) // 14
            {
                block.Chunk.SetData(block.Coords, ++block.MetaData, false);
                return;
            }

            block.World.SetBlockData(block.Coords, 0);
            block.World.SetBlockAndData(oneUp, (byte)BlockData.Blocks.Cactus, 0x0);
        }

        public override void Touch(EntityBase entity, StructBlock block, BlockFace face)
        {
            if (!entity.Server.GetEntities().Contains(entity))
                return;
            if (entity is ItemEntity)
            {
                entity.Server.RemoveEntity(entity);
            } else if (entity is Mob)
            {
                Mob mob = entity as Mob;
                mob.DamageMob();
            } else if (entity is Player)
            {
                Player p = entity as Player;
                p.Client.DamageClient(DamageCause.Contact, 1);
            }
        }
    }
}
