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
using Chraft.PluginSystem;
using Chraft.PluginSystem.Server;

namespace Chraft.Plugins.IrcPlugin
{
    public partial class IrcClient
    {
        private Thread _thread;
        private volatile bool _running = true;
        private bool _connecting = true;
        private StreamReader _rx;
        private StreamWriter _tx;
        private IPluginLogger _logger;
        private IPlugin _plugin;

        /// <summary>
        /// Gets the end point.
        /// </summary>
        public IPEndPoint EndPoint { get; private set; }

        /// <summary>
        /// Gets the nickname.
        /// </summary>
        public string Nickname { get; private set; }

        public IrcClient(IPEndPoint endPoint, string nickname, IPluginLogger logger, IPlugin plugin)
        {
            EndPoint = endPoint;
            _logger = logger;
            _plugin = plugin;
            Nickname = nickname.Replace(' ', '_');
            Start();
        }

        public void WriteLine(string message)
        {
            _tx.WriteLine(message);
            _tx.Flush();
        }

        public void WriteLine(string format, params object[] args)
        {
            WriteLine(string.Format(format, args));
        }

        public void Join(string channel)
        {
            _logger.Log(LogLevel.Debug, _plugin.Name, "Joining channel: {0}", channel);
            WriteLine("JOIN {0}", channel.Replace(' ', '_'));
        }

        private void Start()
        {
            _logger.Log(LogLevel.Debug, _plugin.Name, "Starting IRC client");
            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        public void Stop()
        {
            _logger.Log(LogLevel.Debug, _plugin.Name, "Stopping IRC client");
            _running = false;
            if (_tx != null && _tx.BaseStream.CanWrite)
                _tx.WriteLine("QUIT :C#raft server shutting down.");
        }

        private void Run()
        {
            TcpClient tcp = new TcpClient();
            try
            {
                tcp.Connect(EndPoint);
            }
            catch (Exception)
            {
                _logger.Log(LogLevel.Warning, _plugin.Name, "Could not connect to irc channel");
                return;
            }

            using (NetworkStream stream = tcp.GetStream())
            {
                _tx = new StreamWriter(stream);
                _rx = new StreamReader(stream);
                _tx.NewLine = "\r\n";
                _tx.WriteLine("NICK " + Nickname);
                _tx.WriteLine("USER Chraft * * :C#raft Minecraft Server");
                _tx.Flush();

                while (_running)
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
            string line = _rx.ReadLine();
            if (string.IsNullOrWhiteSpace(line))
                return;

            _logger.Log(LogLevel.Debug, _plugin.Name, "IRC Rx: {0}", line);

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
                _logger.Log(LogLevel.Trivial, _plugin.Name, "IRC Parse:: Line:{0}, Prefix:{1}", line, prefix);
            }

            int sep = line.IndexOf(' ');
            command = (sep < 0 ? line : line.Remove(sep)).ToUpper();
            if (sep >= 0)
                line = line.Substring(sep + 1);

            _logger.Log(LogLevel.Trivial, _plugin.Name, "IRC Parse:: Line:{0}, Prefix:{1}, Command:", line, prefix, command);
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
