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
using Chraft.Interfaces.Recipes;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;
using Chraft.Entity;

namespace Chraft.Interfaces
{
	public class WorkbenchInterface : CraftingInterface
	{
		public WorkbenchInterface()
			: base(InterfaceType.Workbench, 9, 10)
		{
		}

		internal override void Associate(Player player)
		{
			base.Associate(player);
		}

        bool _useProvidedDropCoordinates = false;

	    private UniversalCoords _DropCoords;

        /// <summary>
        /// Opens the Workbench and specifies where items should be dropped if exiting the workbench with items still in it.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="z"></param>
        public virtual void Open(UniversalCoords coords)
        {
            _useProvidedDropCoordinates = true;
            _DropCoords = coords;

            this.Open();
        }

        protected override void DoClose()
        {
            base.DoClose();

            // Drop all items from the workbench
            if (_useProvidedDropCoordinates)
            {
                base.DropAll(_DropCoords);
            }
            else
            {
                base.DropAll(UniversalCoords.FromAbsWorld(Owner.Position));
            }
        }

        public enum WorkbenchSlots : short
        {
            CraftingOutput = 0,
            CraftingInputFirst = 1,
            CraftingInputLast = 9,
            InventoryFirst = 10,
            InventoryLast = 36,
            QuickSlotFirst = 37,
            QuickSlotLast = 45
        }
	}
}
