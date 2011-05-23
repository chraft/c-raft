using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;

namespace Chraft.Net
{
	public delegate void PacketEventHandler<T>(object sender, PacketEventArgs<T> e) where T : Packet;

	public class PacketEventArgs<T> : EventArgs where T : Packet
	{
		public T Packet { get; private set; }

		public PacketEventArgs(T packet)
		{
			Packet = packet;
		}
	}
}
