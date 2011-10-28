using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft.Interfaces
{
    public class SmallChestInterface : SingleContainerInterface
    {
        public SmallChestInterface(World.WorldManager world, UniversalCoords coords)
            : base(world, InterfaceType.Chest, coords, 27)
        {
        }

        public enum SmallChestSlots : short
        {
            ChestFirst = 0,
            ChestLast = 26,
            InventoryFirst = 27,
            InventoryLast = 53,
            QuickSlotFirst = 54,
            QuickSlotLast = 62
        }
    }
}
