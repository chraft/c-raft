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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces;
using Chraft.World;
using Chraft.World.Blocks;
using Chraft.Entity;

namespace Chraft.Plugins.Events.Args
{
    /// <summary>
    /// The base EventArgs for an Block Event.
    /// </summary>
    public class BlockEventArgs : ChraftEventArgs
    {
        public BlockBase Block { get; private set; }
        public BlockEventArgs(BlockBase block)
        {
            Block = block;
        }
    }

    public class BlockPlaceEventArgs : BlockEventArgs
    {
        public EntityBase PlacedBy { get; set; }
        public BlockPlaceEventArgs(BlockBase block, EntityBase placedBy)
            : base(block)
        {
            PlacedBy = placedBy;
        }
    }
    public class BlockDestroyEventArgs : BlockEventArgs
    {
        public EntityBase DestroyedBy{ get; set; }
        public List<ItemStack> LootTable { get; set; }

        public BlockDestroyEventArgs(BlockBase block, EntityBase destroyedBy)
            : base(block)
        {
            LootTable = block.LootTable;
            DestroyedBy = destroyedBy;
        }
    }

    public class BlockTouchEventArgs : BlockEventArgs
    {
        public EntityBase TouchedBy { get; set; }

        public BlockTouchEventArgs(BlockBase block, EntityBase touchedBy)
            : base(block)
        {
            TouchedBy = touchedBy;
        }
    }
}
