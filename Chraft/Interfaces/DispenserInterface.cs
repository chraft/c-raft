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
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;

namespace Chraft.Interfaces
{
	public class DispenserInterface : PersistentContainerInterface
	{
        public DispenserInterface(WorldManager world, UniversalCoords coords)
            : base(world, coords, InterfaceType.Dispenser, 9)
		{
		}

        public enum DispenserSlots : short
        {
            DispenserFirst = 0,
            DispenserLast = 8,
            InventoryFirst = 9,
            InventoryLast = 35,
            QuickSlotFirst = 36,
            QuickSlotLast = 44
        }
	}
}
