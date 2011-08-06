using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Interfaces
{
	public class FurnaceInterface : SingleContainerInterface
	{
		public FurnaceInterface(World.WorldManager world, int x, int y, int z)
            : base(world, InterfaceType.Furnace, x, y, z, 3)
		{
		}
	}
}
