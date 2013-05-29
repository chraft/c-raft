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
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    class BlockLava : BlockBase
    {
        public BlockLava()
        {
            Name = "Lava";
            Type = BlockData.Blocks.Lava;
            IsLiquid = true;
            Luminance = 0xf;
        }

        public override void Touch(IEntityBase entity, IStructBlock iBlock, BlockFace face)
        {
            var living = entity as LivingEntity;
            if (living != null)
            {
                living.TouchedLava();
            }
            base.Touch(entity, iBlock, face);
        }
    }
}
