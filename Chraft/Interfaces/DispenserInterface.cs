using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Interfaces
{
	public class DispenserInterface : SingleContainerInterface
	{
        public DispenserInterface(World.WorldManager world, UniversalCoords coords)
            : base(world, InterfaceType.Dispenser, coords, 9)
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
