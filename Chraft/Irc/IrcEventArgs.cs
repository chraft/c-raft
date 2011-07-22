using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Irc
{
	public delegate void IrcEventHandler(object sender, IrcEventArgs e);

	public class IrcEventArgs : EventArgs
	{
		public HostMask Prefix { get; private set; }
		public string Command { get; private set; }
		public string[] Args { get; private set; }
		public bool Handled { get; set; }

		public IrcEventArgs(HostMask prefix, string command, string[] args)
		{
			Prefix = prefix;
			Command = command;
			Args = args;
			Handled = false;
		}
	}
}
