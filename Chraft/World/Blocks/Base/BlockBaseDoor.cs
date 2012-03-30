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
using Chraft.Net;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;

namespace Chraft.World.Blocks.Base
{
    public abstract class BlockBaseDoor : BlockBase
    {
        protected BlockBaseDoor()
        {
            Name = "BaseDoor";
            Opacity = 0x0;
            IsSolid = true;
        }

        public override void Place(IEntityBase entity, IStructBlock iBlock, IStructBlock targetIBlock, BlockFace face)
        {
            StructBlock block = (StructBlock)iBlock;
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
            base.Place(entity, block, targetIBlock, face);
        }
  
        public virtual bool IsOpen(StructBlock block)
        {
            // TODO: correctly implement block state for doors
            return false;
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

        protected override void UpdateWorld(StructBlock block, bool isDestroyed = false)
        {
            base.UpdateWorld(block, isDestroyed);
            if (isDestroyed)
                return;
            if ((block.MetaData & 8) != 0 && block.Coords.WorldY < 127)
            {
                UniversalCoords upperBlock = UniversalCoords.FromWorld(block.Coords.WorldX, block.Coords.WorldY + 1,
                                                                       block.Coords.WorldZ);
                StructBlock upperHalf = new StructBlock(upperBlock, (byte)Type, (byte)(block.MetaData | 8), block.World);
                BlockHelper.Instance.CreateBlockInstance((byte)Type).Spawn(upperHalf);
            }
        }
    }
}
