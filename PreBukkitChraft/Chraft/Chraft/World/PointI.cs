using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.World
{
	// Immutable
	public struct PointI
	{
		public readonly int X;
		public readonly int Y;
		public readonly int Z;
		public PointI Chunk { get { return new PointI(X >> 4, Y >> 4); } }
		public PointI Block { get { return new PointI(X << 4, Y << 4); } }

		public PointI(int x, int y, int z)
		{
			X = x;
			Y = y;
			Z = z;
		}

		public PointI(int x, int z)
		{
			X = x;
			Y = 0;
			Z = z;
		}
	}
}
