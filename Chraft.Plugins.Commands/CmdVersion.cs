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
using System.Reflection;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Misc;

namespace Chraft.Plugins.Commands
{
    public class CmdVersion : IClientCommand
    {
        public IPlugin Iplugin { get; set; }

        public string Name
        {
            get { return "version"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Information; }
        }

        public string Permission
        {
            get { return "chraft.version"; }
        }

        public CmdVersion(IPlugin plugin)
        {
            Iplugin = plugin;
        }

        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;
            client.SendMessage("Server is powered by C#raft v" + version);
        }

        public void Help(IClient client)
        {
            client.SendMessage("/version - output the version of the server");
        }
    }
}
