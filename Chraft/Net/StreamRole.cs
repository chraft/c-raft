using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Net
{
	/// <summary>
	/// Specifies the role that this instance is playing in the communication.
	/// Used for flipping packets that are different when sent from server -> client or client -> server.
	/// </summary>
	public enum StreamRole
	{
		/// <summary>
		/// This stream represents the server-side of communication
		/// </summary>
		Server,
		/// <summary>
		/// This stream represents the client-side of communication
		/// </summary>
		Client
	}
}
