using System;
using Chraft.Commands;
using Chraft.Net;

namespace Chraft
{
	public class CommandEventArgs : EventArgs
	{
        public IClientCommand Command { get; private set; }
		public string[] Tokens { get; private set; }
		public Client Client { get; private set; }

		public CommandEventArgs(Client client, IClientCommand command, string[] tokens)
		{
			Client = client;
			Tokens = tokens;
            Command = command;
		}
	}
}
