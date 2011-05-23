using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Net
{
	public class InvalidStatePacket : Packet
	{
		public InvalidReason Reason { get; set; }

		public override void Read(BigEndianStream stream)
		{
			Reason = (InvalidReason)stream.ReadByte();
		}

		public override void Write(BigEndianStream stream)
		{
			stream.WriteByte((byte)Reason);
		}

		public enum InvalidReason : byte
		{
			InvalidBed = 0,
			NullA = 1,
			NullB = 2
		}
	}
}
