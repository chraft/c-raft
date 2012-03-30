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

using Chraft.Entity;
using Chraft.Interfaces.Containers;
using Chraft.Net;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;

namespace Chraft.World.Blocks.Base
{
    public abstract class BlockBaseContainer : BlockBase, IBlockInteractive
    {
        public BlockBaseContainer()
        {
            IsSolid = true;
        }

        public override void Place(IEntityBase entity, IStructBlock iBlock, IStructBlock targetIBlock, BlockFace face)
        {
            StructBlock block = (StructBlock) iBlock;
            StructBlock targetBlock = (StructBlock) targetIBlock;
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

        protected override void UpdateWorld(StructBlock block, bool isDestroyed = false)
        {
            if (isDestroyed)
                ContainerFactory.Destroy((WorldManager)block.World, block.Coords);
            base.UpdateWorld(block, isDestroyed);
        }

        public void Interact(IEntityBase entity, IStructBlock block)
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
