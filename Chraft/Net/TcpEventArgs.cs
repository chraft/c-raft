using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;

namespace Chraft.Net
{
	public class TcpEventArgs : EventArgs
	{
		/// <summary>
		/// The TCP client associated with the event.
		/// </summary>
		public Socket TcpSocket { get; private set; }
		
		/// <summary>
		/// Whether or not the TCP event was cancelled
		/// </summary>
		public bool Cancelled { get; set; }

		internal TcpEventArgs(Socket socket)
		{
            TcpSocket = socket;
		}
	}
}
