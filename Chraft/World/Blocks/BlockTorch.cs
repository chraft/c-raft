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
    class BlockTorch : BlockBase
    {
        public BlockTorch()
        {
            Name = "Torch";
            Type = BlockData.Blocks.Torch;
            IsAir = true;
            IsSingleHit = true;
            Luminance = 0xe;
            BurnEfficiency = 300;
            var item = ItemHelper.GetInstance(Type);
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

            switch (face)
            {
                case BlockFace.Down: return;
                case BlockFace.Up: block.MetaData = (byte)MetaData.Torch.Standing;
                    break;
                case BlockFace.West: block.MetaData = (byte)MetaData.Torch.West;
                    break;
                case BlockFace.East: block.MetaData = (byte)MetaData.Torch.East;
                    break;
                case BlockFace.North: block.MetaData = (byte)MetaData.Torch.North;
                    break;
                case BlockFace.South: block.MetaData = (byte)MetaData.Torch.South;
                    break;
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
    }
}
