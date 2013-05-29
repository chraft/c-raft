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
    class BlockLadder : BlockBase
    {
        public BlockLadder()
        {
            Name = "Ladder";
            Type = BlockData.Blocks.Ladder;
            IsAir = true;
            IsWaterProof = true;
            var item = ItemHelper.GetInstance(Type);
            item.Count = 1;
            LootTable.Add(item);
            Opacity = 0x0;
        }

        public override void Place(IEntityBase entity, IStructBlock iBlock, IStructBlock targetIBlock, BlockFace face)
        {
            var block = (StructBlock)iBlock;
            switch (face)
            {
                case BlockFace.East:
                    block.MetaData = (byte)MetaData.Ladders.East;
                    break;
                case BlockFace.West:
                    block.MetaData = (byte)MetaData.Ladders.West;
                    break;
                case BlockFace.North:
                    block.MetaData = (byte)MetaData.Ladders.North;
                    break;
                case BlockFace.South:
                    block.MetaData = (byte)MetaData.Ladders.South;
                    break;
                default:
                    return;
            }
            base.Place(entity, block, targetIBlock, face);
        }
    }
}
