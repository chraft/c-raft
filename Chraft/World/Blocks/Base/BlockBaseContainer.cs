using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Interfaces.Containers;
using Chraft.Net;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    public abstract class BlockBaseContainer : BlockBase, IBlockInteractive
    {
        public BlockBaseContainer()
        {
            IsSolid = true;
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            LivingEntity living = (entity as LivingEntity);
            if (living == null)
                return;
            block.MetaData = GetDirection(living, block, targetBlock, face);
            base.Place(entity, block, targetBlock, face);
        }

        protected override bool CanBePlacedOn(EntityBase who, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            Chunk chunk = GetBlockChunk(block);
            Chunk targetChunk = GetBlockChunk(targetBlock);
            if (chunk == null || targetChunk == null)
                return false;
            if (targetSide == BlockFace.Down)
                return false;

            return base.CanBePlacedOn(who, block, targetBlock, targetSide);
        }

        protected virtual byte GetDirection(LivingEntity living, StructBlock block, StructBlock targetBlock, BlockFace face)
        {
            byte direction = 0;
            switch (face)
            {
                case BlockFace.East:
                    direction = (byte)MetaData.Container.East;
                    break;
                case BlockFace.West:
                    direction = (byte)MetaData.Container.West;
                    break;
                case BlockFace.North:
                    direction = (byte)MetaData.Container.North;
                    break;
                case BlockFace.South:
                    direction = (byte)MetaData.Container.South;
                    break;
                default:
                    switch (living.FacingDirection(4)) // Built on floor, set by facing dir
                    {
                        case "N":
                            direction = (byte)MetaData.Container.North;
                            break;
                        case "W":
                            direction = (byte)MetaData.Container.West;
                            break;
                        case "S":
                            direction = (byte)MetaData.Container.South;
                            break;
                        case "E":
                            direction = (byte)MetaData.Container.East;
                            break;
                        default:
                            return 0;

                    }
                    break;
            }
            return direction;
        }

        protected override void UpdateOnDestroy(StructBlock block)
        {
            ContainerFactory.Destroy(block.World, block.Coords);
            base.UpdateOnDestroy(block);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player == null)
                return;
            if (player.CurrentInterface != null)
                return;
            ContainerFactory.Open(player, block.Coords);
        }
    }
}
