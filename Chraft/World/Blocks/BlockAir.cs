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

namespace Chraft.World.Blocks
{
    class BlockAir : BlockBase
    {
        public BlockAir()
        {
            Name = "Air";
            Type = BlockData.Blocks.Air;
            IsAir = true;
            Opacity = 0x0;
        }

        public override void Destroy(EntityBase entity, StructBlock block) {}

        public override void Touch(EntityBase entity, StructBlock block, BlockFace face) { }

        public override void Place(EntityBase entity, StructBlock block, StructBlock targetBlock, BlockFace targetSide) { }


    }
}
