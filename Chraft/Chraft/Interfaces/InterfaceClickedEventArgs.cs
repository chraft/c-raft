using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;

namespace Chraft.Interfaces
{
	public class InterfaceClickedEventArgs : EventArgs
	{
		public short Slot { get; private set; }
		public short Transaction { get; private set; }
		public bool RightClick { get; private set; }
		public Interface Interface { get; private set; }
		public ClickLocation Location { get; private set; }
		public bool Cancelled { get; private set; }

		public InterfaceClickedEventArgs(Interface iface, WindowClickPacket packet)
		{
			this.Interface = iface;
			this.Slot = packet.Slot;
			this.RightClick = packet.RightClick;
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
