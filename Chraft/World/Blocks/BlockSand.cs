using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Physics;

namespace Chraft.World.Blocks
{
    class BlockSand : BlockBase
    {
        public BlockSand()
        {
            Name = "Sand";
            Type = BlockData.Blocks.Sand;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
        }

        public override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                    targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                    targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
            {
                StartPhysics(targetBlock);
            }
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            if (!CanBePlacedOn(entity, block, targetBlock, face))
                return;

            if (!RaisePlaceEvent(entity, block))
                return;

            UpdateOnPlace(block);

            RemoveItem(entity);

            if (block.Coords.WorldY > 1)
                if (block.World.GetBlockId(block.Coords.WorldX, block.Coords.WorldY - 1, block.Coords.WorldZ) == (byte)BlockData.Blocks.Air)
                    StartPhysics(block);
        }

        protected void StartPhysics(StructBlock block)
        {
            Remove(block);
            FallingSand fsBlock = new FallingSand(block.World, new Location(block.Coords.WorldX + 0.5, block.Coords.WorldY + 0.5, block.Coords.WorldZ + 0.5));
            fsBlock.Start();
            block.World.PhysicsBlocks.TryAdd(fsBlock.EntityId, fsBlock);
        }
    }
}
