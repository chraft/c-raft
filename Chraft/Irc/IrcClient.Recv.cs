using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Irc
{
	public partial class IrcClient
	{
		public event IrcEventHandler Received;
		
		public string ServerName { get; private set; }
		public string ServerVersion { get; private set; }
		public string ChanModes { get; private set; }
		public string UserModes { get; private set; }

		private void OnReceive(HostMask prefix, string command, string[] args)
		{
			if (Received != null)
			{
				IrcEventArgs e = new IrcEventArgs(prefix, command, args);
				Received.Invoke(this, e);
				if (e.Handled)
					return;
			}

			switch (command)
			{
			case "NICK": OnNick(prefix, args); break;
			case "PING": OnPing(args); break;
			case "001": OnWelcome(args); break;
			case "002": OnYourHost(args); break;
			case "003": OnCreated(args); break;
			case "004": OnMyInfo(args); break;
			}
		}

		private void OnPing(string[] args)
		{
			WriteLine("PONG :{0}", args[0]);
		}

		private void OnMyInfo(string[] args)
		{
			ServerName = args[0];
			ServerVersion = args[1];
			UserModes = args[2];
			ChanModes = args[3];
		}

		private void OnNick(HostMask prefix, string[] args)
		{
			string oldNick = prefix.Nickname;
			string newNick = args[0];
			if (oldNick.ToLower() == Nickname.ToLower())
				Nickname = newNick;
			Echo("{0} is now known as {1}", oldNick, newNick);
		}

		private void OnWelcome(string[] args)
		{
			Echo(string.Join(" ", args));
		}

		private void OnYourHost(string[] args)
		{
			Echo(string.Join(" ", args));
		}

		private void OnCreated(string[] args)
		{
			Echo(string.Join(" ", args));
		}
	}
}
