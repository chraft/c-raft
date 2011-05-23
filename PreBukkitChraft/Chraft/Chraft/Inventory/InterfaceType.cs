using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Inventory
{
	internal enum InterfaceType : sbyte
	{
		Inventory = -2,
		Cursor = -1,
		Chest = 0,
		Workbench = 1,
		Furnace = 2,
		Dispenser = 3
	}
}
