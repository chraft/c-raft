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
using System.IO;
using Chraft.Interfaces.Containers;
using Chraft.Net.Packets;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;

namespace Chraft.Interfaces
{
	public class LargeChestInterface : PersistentContainerInterface
	{
        public UniversalCoords SecondCoords { get; private set; }

        /// <summary>
        /// Creates a Large Chest interface for the two chests specified (North or East chest, and South or West chest)
        /// </summary>
        /// <param name="world"></param>
        /// <param name="neChest">The North or East chest coordinates</param>
        /// <param name="swChest">The South or West chest coordinates</param>
        public LargeChestInterface(WorldManager world, UniversalCoords neChest, UniversalCoords swChest)
            : base(world, neChest, InterfaceType.Chest, 54)
		{
            SecondCoords = swChest;
		}

        public enum LargeChestSlots : short
        {
            ChestFirst = 0,
            ChestLast = 53,
            InventoryFirst = 54,
            InventoryLast = 80,
            QuickSlotFirst = 81,
            QuickSlotLast = 89
        }
	}
}
