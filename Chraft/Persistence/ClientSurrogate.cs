using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces;
using System.Runtime.Serialization;

namespace Chraft.Persistence
{
	[Serializable]
	public sealed class ClientSurrogate
	{
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public float Yaw { get; set; }
		public float Pitch { get; set; }
		public Inventory Inventory { get; set; }
	}
}
