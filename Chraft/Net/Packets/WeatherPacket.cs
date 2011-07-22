using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;

namespace Chraft.Net
{
	public class WeatherPacket : Packet
	{
		public int EntityId { get; set; }
		public bool Unknown { get; set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }

		public override void Read(BigEndianStream stream)
		{
			EntityId = stream.ReadInt();
			Unknown = stream.ReadBool();
			X = stream.ReadDoublePacked();
			Y = stream.ReadDoublePacked();
			Z = stream.ReadDoublePacked();
		}

		public override void Write(BigEndianStream stream)
		{
			stream.Write(EntityId);
			stream.Write(Unknown);
			stream.Write(X);
			stream.Write(Y);
			stream.Write(Z);
		}
	}
}
