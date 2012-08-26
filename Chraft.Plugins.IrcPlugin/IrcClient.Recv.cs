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
using Chraft.PluginSystem.Server;

namespace Chraft.Plugins.IrcPlugin
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
            _logger.Log(LogLevel.Debug, _plugin.Name, "OnReceive - {0}, {1}, {2}", prefix.Mask, command, args);

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
            _logger.Log(LogLevel.Debug, _plugin.Name, "{0} is now known as {1}", oldNick, newNick);
        }

        private void OnWelcome(string[] args)
        {
            _logger.Log(LogLevel.Debug, _plugin.Name, string.Join(" ", args));
        }

        private void OnYourHost(string[] args)
        {
            _logger.Log(LogLevel.Debug, _plugin.Name, string.Join(" ", args));
        }

        private void OnCreated(string[] args)
        {
            _logger.Log(LogLevel.Debug, _plugin.Name, string.Join(" ", args));
        }
    }
}
