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

using System.Collections.Generic;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Item;
using Chraft.Utilities.Blocks;

namespace Chraft.PluginSystem.World.Blocks
{
    public interface IBlockBase
    {
        List<IItemInventory> GetLootTable();

        /// <summary>
        /// Destroy the block and drop the loot (if any)
        /// </summary>
        void Destroy(IStructBlock block);

        /// <summary>
        /// Destroy the block and drop the loot (if any)
        /// </summary>
        void Destroy(IEntityBase who, IStructBlock block);

        /// <summary>
        /// Block was touched by someone
        /// </summary>
        void Touch(IEntityBase who, IStructBlock block, BlockFace face);

        /// <summary>
        /// Place the block
        /// </summary>
        void Place(IStructBlock block, IStructBlock targetBlock, BlockFace face);

        /// <summary>
        /// Place the block
        /// </summary>
        void Place(IEntityBase who, IStructBlock block, IStructBlock targetBlock, BlockFace face);
    }
}
