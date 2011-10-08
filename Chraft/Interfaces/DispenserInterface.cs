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
	}
}
