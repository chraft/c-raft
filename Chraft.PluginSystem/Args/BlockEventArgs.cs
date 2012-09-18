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
using Chraft.PluginSystem.World.Blocks;

namespace Chraft.PluginSystem.Args
{
    /// <summary>
    /// The base EventArgs for an Block Event.
    /// </summary>
    public class BlockEventArgs : ChraftEventArgs
    {
        public IBlockBase Block { get; private set; }
        public BlockEventArgs(IBlockBase block)
        {
            Block = block;
        }
    }

    public class BlockPlaceEventArgs : BlockEventArgs
    {
        public IEntityBase PlacedBy { get; set; }
        public BlockPlaceEventArgs(IBlockBase block, IEntityBase placedBy)
            : base(block)
        {
            PlacedBy = placedBy;
        }
    }
    public class BlockDestroyEventArgs : BlockEventArgs
    {
        public IEntityBase DestroyedBy { get; set; }
        public List<IItemInventory> LootTable { get; set; }

        public BlockDestroyEventArgs(IBlockBase block, IEntityBase destroyedBy)
            : base(block)
        {
            LootTable = block.GetLootTable();
            DestroyedBy = destroyedBy;
        }
    }

    public class BlockTouchEventArgs : BlockEventArgs
    {
        public IEntityBase TouchedBy { get; set; }

        public BlockTouchEventArgs(IBlockBase block, IEntityBase touchedBy)
            : base(block)
        {
            TouchedBy = touchedBy;
        }
    }
}
