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
using Chraft.Entity.Items;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockCobblestoneStairs : BlockBase
    {
        public BlockCobblestoneStairs()
        {
            Name = "CobblestoneStairs";
            Type = BlockData.Blocks.Cobblestone_Stairs;
            IsSolid = true;
            var item = ItemHelper.GetInstance((short)Type);
            item.Count = 1;
            LootTable.Add(item);
        }

        public override void Place(IEntityBase entity, IStructBlock iBlock, IStructBlock targetIBlock, BlockFace targetSide)
        {
            StructBlock block = (StructBlock)iBlock;
            LivingEntity living = entity as LivingEntity;
            if (living == null)
                return;

            // TODO: Bugged - should depend on the player's Yaw/Pitch
            switch (living.FacingDirection(4))
            {
                case "N":
                    block.MetaData = (byte)MetaData.Stairs.South;
                    break;
                case "E":
                    block.MetaData = (byte)MetaData.Stairs.West;
                    break;
                case "S":
                    block.MetaData = (byte)MetaData.Stairs.North;
                    break;
                case "W":
                    block.MetaData = (byte)MetaData.Stairs.East;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetIBlock, targetSide);
        }
    }
}
