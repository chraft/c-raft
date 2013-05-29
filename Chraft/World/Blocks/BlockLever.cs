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
    class BlockLever : BlockBase, IBlockInteractive
    {
        public BlockLever()
        {
            Name = "Lever";
            Type = BlockData.Blocks.Lever;
            IsAir = true;
            var item = ItemHelper.GetInstance(Type);
            item.Count = 1;
            LootTable.Add(item);
            Opacity = 0x0;
        }

        public override void Place(IEntityBase iEntity, IStructBlock iBlock, IStructBlock targetIBlock, BlockFace face)
        {
            var block = (StructBlock)iBlock;
            var entity = iEntity as EntityBase;

            if (entity == null)
                return;

            switch (face)
            {
                case BlockFace.North:
                    block.MetaData = (byte) MetaData.Lever.NorthWall;
                    break;
                case BlockFace.West:
                    block.MetaData = (byte) MetaData.Lever.WestWall;
                    break;
                case BlockFace.East:
                    block.MetaData = (byte) MetaData.Lever.EastWall;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte) MetaData.Lever.SouthWall;
                    break;
                case BlockFace.Up:
                    // Works weird. Even in the original game
                    if (targetIBlock.Coords.WorldZ > entity.Position.Z)
                        block.MetaData = (byte)MetaData.Lever.EWGround;
                    else if (targetIBlock.Coords.WorldZ < entity.Position.Z)
                        block.MetaData = (byte)MetaData.Lever.EWGround;
                    else if (targetIBlock.Coords.WorldX > entity.Position.X)
                        block.MetaData = (byte)MetaData.Lever.NSGround;
                    else if (targetIBlock.Coords.WorldX < entity.Position.X)
                        block.MetaData = (byte) MetaData.Lever.NSGround;
                    else
                        block.MetaData = (byte)MetaData.Lever.NSGround;
                    break;
                default:
                    return;
            }

            base.Place(entity, block, targetIBlock, face);
        }

        protected override void NotifyDestroy(EntityBase entity, StructBlock sourceBlock, StructBlock targetBlock)
        {
            if (targetBlock.Coords.WorldY > sourceBlock.Coords.WorldY && targetBlock.MetaData == (byte)MetaData.Torch.Standing ||
                targetBlock.Coords.WorldX > sourceBlock.Coords.WorldX && targetBlock.MetaData == (byte)MetaData.Torch.South ||
                targetBlock.Coords.WorldX < sourceBlock.Coords.WorldX && targetBlock.MetaData == (byte)MetaData.Torch.North ||
                targetBlock.Coords.WorldZ > sourceBlock.Coords.WorldZ && targetBlock.MetaData == (byte)MetaData.Torch.West ||
                targetBlock.Coords.WorldZ < sourceBlock.Coords.WorldZ && targetBlock.MetaData == (byte)MetaData.Torch.East)
                Destroy(targetBlock);
            base.NotifyDestroy(entity, sourceBlock, targetBlock);
        }

        public void Interact(IEntityBase entity, IStructBlock block)
        {
            // Switch the lever
        }
    }
}
