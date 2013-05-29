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
using Chraft.Net;
using Chraft.Interfaces;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockWorkbench : BlockBase, IBlockInteractive
    {
        public BlockWorkbench()
        {
            Name = "Workbench";
            Type = BlockData.Blocks.Workbench;
            IsSolid = true;
            var item = ItemHelper.GetInstance(Type);
            item.Count = 1;
            LootTable.Add(item);
            BurnEfficiency = 300;
        }

        public override void Place(IEntityBase entity, IStructBlock iBlock, IStructBlock targetBlock, BlockFace face)
        {
            var block = (StructBlock) iBlock;
            var living = (entity as LivingEntity);
            if (living == null)
                return;

            switch (face) //Bugged, as the client has a mind of its own for facing
            {
                case BlockFace.East:
                    block.MetaData = (byte)MetaData.Container.East;
                    break;
                case BlockFace.West:
                    block.MetaData = (byte)MetaData.Container.West;
                    break;
                case BlockFace.North:
                    block.MetaData = (byte)MetaData.Container.North;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte)MetaData.Container.South;
                    break;
                default:
                    switch (living.FacingDirection(4)) // Built on floor, set by facing dir
                    {
                        case "N":
                            block.MetaData = (byte)MetaData.Container.North;
                            break;
                        case "W":
                            block.MetaData = (byte)MetaData.Container.West;
                            break;
                        case "S":
                            block.MetaData = (byte)MetaData.Container.South;
                            break;
                        case "E":
                            block.MetaData = (byte)MetaData.Container.East;
                            break;
                        default:
                            return;

                    }
                    break;
            }
            base.Place(entity, block, targetBlock, face);
        }

        public void Interact(IEntityBase entity, IStructBlock block)
        {
            var player = entity as Player;
            if (player == null)
                return;
            if (player.CurrentInterface != null)
                return;
            player.CurrentInterface = new WorkbenchInterface();
            player.CurrentInterface.Associate(player);
            ((WorkbenchInterface)player.CurrentInterface).Open(block.Coords);
        }
    }
}
