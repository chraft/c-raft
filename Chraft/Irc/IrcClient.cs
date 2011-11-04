#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Threading;
using System.Net.Sockets;
using System.IO;

namespace Chraft.Irc
{
	public partial class IrcClient
	{
		private Thread Thread;
		private volatile bool Running = true;
		private bool Connecting = true;
		private StreamReader Rx;
		private StreamWriter Tx;

		public IPEndPoint EndPoint { get; private set; }
		public string Nickname { get; private set; }

		public IrcClient(IPEndPoint endPoint, string nickname)
		{
			this.EndPoint = endPoint;
			this.Nickname = nickname.Replace(' ', '_');
			Start();
		}

		public void Echo(string line)
		{
			Console.WriteLine(line);
		}

		public void WriteLine(string message)
		{
			Echo("IRC Tx: {0}", message);
			Tx.WriteLine(message);
			Tx.Flush();
		}

		public void WriteLine(string format, params object[] args)
		{
			WriteLine(string.Format(format, args));
		}

		public void Join(string channel)
		{
			WriteLine("JOIN {0}", channel.Replace(' ', '_'));
		}

		private void Echo(string format, params object[] args)
		{
			Echo(string.Format(format, args));
		}

		private void Start()
		{
			Thread = new Thread(Run);
			Thread.IsBackground = true;
			Thread.Start();
		}

		public void Stop()
		{
			Running = false;
			if (Tx != null && Tx.BaseStream.CanWrite)
				Tx.WriteLine("QUIT :C#raft server shutting down.");
		}

		private void Run()
		{
			TcpClient tcp = new TcpClient();
			tcp.Connect(EndPoint);

			using (NetworkStream stream = tcp.GetStream())
			{
				Tx = new StreamWriter(stream);
				Rx = new StreamReader(stream);
				Tx.NewLine = "\r\n";
				Tx.WriteLine("NICK " + Nickname);
				Tx.WriteLine("USER Chraft * * :C#raft Minecraft Server");
				Tx.Flush();

				while (Running)
				{
					try
					{
						RunProc();
					}
					catch (Exception ex)
					{
						Console.WriteLine(ex);
					}
				}
			}
			tcp.Close();
		}

		private void RunProc()
		{
			string line = Rx.ReadLine();
			if (string.IsNullOrWhiteSpace(line))
				return;

			Echo("IRC Rx: {0}", line);

			string prefix, command;
			string[] args;
			Parse(line, out prefix, out command, out args);
			OnReceive(new HostMask(prefix), command, args);
		}

		private void Parse(string line, out string prefix, out string command, out string[] args)
		{
			prefix = "";
			command = "";
			List<string> argl = new List<string>();

			if (line.StartsWith(":"))
			{
				string[] parts = line.Substring(1).Split(new char[] { ' ' }, 2);
				line = parts[1];
				prefix = parts[0];
			}

			int sep = line.IndexOf(' ');
			command = (sep < 0 ? line : line.Remove(sep)).ToUpper();
			if (sep >= 0)
				line = line.Substring(sep + 1);

			do
			{
				if (line.StartsWith(":"))
				{
					argl.Add(line.Substring(1));
					goto ret;
				}

				string arg = line.Remove(sep);
				line = line.Substring(sep + 1);
				argl.Add(arg);
			}
			while ((sep = line.IndexOf(' ')) > 0);

			argl.Add(line);
		ret:
			args = argl.ToArray();
		}
	}
}
