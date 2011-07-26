using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Interfaces
{
	public class DispenserInterface : SingleContainerInterface
	{
        public DispenserInterface(World.WorldManager world, int x, int y, int z)
            : base(world, InterfaceType.Dispenser, x, y, z, 9)
		{
		}
	}
}
