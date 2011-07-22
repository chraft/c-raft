using System;

namespace Chraft
{
	public class CommandEventArgs : EventArgs
	{
		public string[] Tokens { get; private set; }
		public Client Client { get; private set; }

		public CommandEventArgs(Client client, string[] tokens)
		{
			Client = client;
			Tokens = tokens;
		}
	}
}
