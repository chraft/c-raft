using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft
{
	public class ChatEventArgs : EventArgs
	{
		/// <summary>
		/// Gets or sets the chat message to be sent.
		/// </summary>
		public string Message { get; set; }

		/// <summary>
		/// Gets or sets whether the even is altogether cancelled.
		/// </summary>
		public bool Cancelled { get; set; }
	}
}
