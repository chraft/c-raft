using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Interfaces
{
    public class SmallChestInterface : SingleContainerInterface
    {
        public SmallChestInterface(World.WorldManager world, int x, int y, int z)
            : base(world, InterfaceType.Chest, x, y, z, 27)
        {
        }
    }
}
