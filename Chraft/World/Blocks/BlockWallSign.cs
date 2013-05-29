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
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockWallSign : BlockSignBase
    {
        public BlockWallSign()
        {
            Name = "WallSign";
            Type = BlockData.Blocks.Wall_Sign;
            IsAir = true;
            IsSolid = true;
            var item = ItemHelper.GetInstance(BlockData.Items.Sign);
            item.Count = 1;
            LootTable.Add(item);
            Opacity = 0x0;
        }

        public override void Place(IEntityBase entity, IStructBlock iBlock, IStructBlock targetIBlock, BlockFace face)
        {
            var block = (StructBlock)iBlock;

            switch (face)
            {
                case BlockFace.West:
                    block.MetaData = (byte)MetaData.SignWall.West;
                    break;
                case BlockFace.East:
                    block.MetaData = (byte)MetaData.SignWall.East;
                    break;
                case BlockFace.North:
                    block.MetaData = (byte)MetaData.SignWall.North;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte)MetaData.SignWall.South;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetIBlock, face);
        }

        protected override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            // TODO: Destroy if adjacent block is destroyed
                //Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }
    }
}
