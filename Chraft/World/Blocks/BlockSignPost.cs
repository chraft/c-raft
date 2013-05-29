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
    class BlockSignPost : BlockSignBase
    {
        public BlockSignPost()
        {
            Name = "SignPost";
            Type = BlockData.Blocks.Sign_Post;
            IsAir = true;
            IsSolid = true;
            IsWaterProof = true;
            var item = ItemHelper.GetInstance(BlockData.Items.Sign);
            item.Count = 1;
            LootTable.Add(item);
            Opacity = 0x0;
        }

        public override void Place(IEntityBase entity, IStructBlock iBlock, IStructBlock targetIBlock, BlockFace face)
        {
            var block = (StructBlock)iBlock;
            var living = (entity as LivingEntity);
            if (living == null)
                return;
            switch (living.FacingDirection(8))
            {
                case "N":
                    block.MetaData = (byte)MetaData.SignPost.North;
                    break;
                case "NE":
                    block.MetaData = (byte)MetaData.SignPost.Northeast;
                    break;
                case "E":
                    block.MetaData = (byte)MetaData.SignPost.East;
                    break;
                case "SE":
                    block.MetaData = (byte)MetaData.SignPost.Southeast;
                    break;
                case "S":
                    block.MetaData = (byte)MetaData.SignPost.South;
                    break;
                case "SW":
                    block.MetaData = (byte)MetaData.SignPost.Southwest;
                    break;
                case "W":
                    block.MetaData = (byte)MetaData.SignPost.West;
                    break;
                case "NW":
                    block.MetaData = (byte)MetaData.SignPost.Northwest;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetIBlock, face);
        }

        protected override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if ((targetBlock.Coords.WorldY - sourceBlock.Coords.WorldY) == 1 &&
                targetBlock.Coords.WorldX == sourceBlock.Coords.WorldX &&
                targetBlock.Coords.WorldZ == sourceBlock.Coords.WorldZ)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }
    }
}
