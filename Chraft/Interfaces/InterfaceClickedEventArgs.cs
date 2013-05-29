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
using Chraft.Net;
using Chraft.Net.Packets;

namespace Chraft.Interfaces
{
    public class InterfaceClickedEventArgs : EventArgs
    {
        public short Slot { get; private set; }
        public short Transaction { get; private set; }
        public WindowClickPacket.MouseButtonClicked MouseButton { get; private set; }
        public Interface Interface { get; private set; }
        public ClickLocation Location { get; private set; }
        public bool Cancelled { get; private set; }

        public InterfaceClickedEventArgs(Interface iface, WindowClickPacket packet)
        {
            this.Interface = iface;
            this.Slot = packet.Slot;
            this.MouseButton = packet.MouseButton;
            this.Transaction = packet.Transaction;

            if (Slot < 0)
            {
                Slot = 0;
                Location = ClickLocation.Void;
            }
            else if (Slot < Interface.SlotCount)
            {
                Location = ClickLocation.Interface;
            }
            else
            {
                Location = ClickLocation.Inventory;
                Slot = (short)(Slot - Interface.SlotCount + 9);
            }
        }

        public void Cancel()
        {
            Cancelled = true;
        }
    }
}
