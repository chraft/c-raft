using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockBaseDoor : BlockBase
    {
        public BlockBaseDoor()
        {
            Name = "BaseDoor";
            Opacity = 0x0;
            IsSolid = true;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            LivingEntity living = entity as LivingEntity;
            if (living == null)
                return;
            switch (living.FacingDirection(4))
            {
                case "N":
                    block.MetaData = (byte)MetaData.Door.Northwest;
                    break;
                case "W":
                    block.MetaData = (byte)MetaData.Door.Southwest;
                    break;
                case "S":
                    block.MetaData = (byte)MetaData.Door.Southeast;
                    break;
                case "E":
                    block.MetaData = (byte)MetaData.Door.Northeast;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetBlock, face);
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            if (block.Coords.WorldY > 125)
                return false;
            UniversalCoords blockAbove = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                                   block.Coords.WorldZ);
            if (block.World.GetBlockId(blockAbove) != (byte)BlockData.Blocks.Air)
                return false;
            return base.CanBePlacedOn(who, block, targetBlock, targetSide);
        }

        protected override void UpdateOnPlace(StructBlock block)
        {
            base.UpdateOnPlace(block);
            UniversalCoords blockAbove = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                                   block.Coords.WorldZ);
            if ((block.MetaData & 8) != 0)
            {
                UniversalCoords upperBlock = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                                       block.Coords.WorldZ);
                StructBlock upperHalf = new StructBlock(upperBlock, (byte)Type, (byte)(block.MetaData | 8), block.World);
                BlockHelper.Instance((byte)Type).Spawn(upperHalf);
            }
        }
    }
}
