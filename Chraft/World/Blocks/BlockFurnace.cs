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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Interfaces;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    class BlockFurnace : BlockBase, IBlockInteractive
    {
        public BlockFurnace()
        {
            Name = "Furnace";
            Type = BlockData.Blocks.Furnace;
            IsSolid = true;
            LootTable.Add(new ItemStack((short)Type, 1));
        }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace targetSide)
        {
            LivingEntity living = (entity as LivingEntity);
            if (living == null)
                return;

            switch (targetSide) //Bugged, as the client has a mind of its own for facing
            {
                case BlockFace.East:
                    block.MetaData = (byte)MetaData.Furnace.East;
                    break;
                case BlockFace.West:
                    block.MetaData = (byte)MetaData.Furnace.West;
                    break;
                case BlockFace.North:
                    block.MetaData = (byte)MetaData.Furnace.North;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte)MetaData.Furnace.South;
                    break;
                default:
                    switch (living.FacingDirection(4)) // Built on floor, set by facing dir
                    {
                        case "N":
                            block.MetaData = (byte)MetaData.Furnace.North;
                            break;
                        case "W":
                            block.MetaData = (byte)MetaData.Furnace.West;
                            break;
                        case "S":
                            block.MetaData = (byte)MetaData.Furnace.South;
                            break;
                        case "E":
                            block.MetaData = (byte)MetaData.Furnace.East;
                            break;
                        default:
                            return;

                    }
                    break;
            }
            base.Place(entity, block, targetBlock, targetSide);
        }

        protected override void DropItems(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player != null)
            {
                FurnaceInterface fi = new FurnaceInterface(block.World, block.Coords);
                fi.Associate(player);
                fi.DropAll(block.Coords);
                fi.Save();
            }
            base.DropItems(entity, block);
        }

        public void Interact(EntityBase entity, StructBlock block)
        {
            Player player = entity as Player;
            if (player == null)
                return;
            if (player.CurrentInterface != null)
                return;
            player.CurrentInterface= new FurnaceInterface(block.World, block.Coords);
            player.CurrentInterface.Associate(player);
            player.CurrentInterface.Open();
        }

    }
}
