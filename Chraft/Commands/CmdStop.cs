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
using System.Runtime.CompilerServices;
using System.Threading;
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Plugins;
using Chraft.Utilities;
using Chraft.Utilities.Misc;

[assembly: InternalsVisibleTo("ChraftServer")]
namespace Chraft.Commands
{
   
    internal class CmdStop : IServerCommand, IClientCommand
    {
        public IServerCommandHandler ServerCommandHandler { get; set; }
        public IClientCommandHandler ClientCommandHandler { get; set; }
        public string Name
        {
            get { return "stop"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.stop"; }
        }

        public IPlugin Iplugin { get; set; }

        public void Use(IServer iServer, string commandName, string[] tokens)
        {
            Server server = iServer as Server;
            server.Broadcast("The server is shutting down.");
            server.Logger.Log(LogLevel.Info, "The server is shutting down.");
            Thread.Sleep(5000);
            server.Stop();
            Thread.Sleep(10);
            Console.WriteLine("Press Enter to exit.");
        }

        public void Help(IServer server)
        {
            server.GetLogger().Log(LogLevel.Info, "Shuts down the server.");
        }

        public void Use(IClient iClient, string commandName, string[] tokens)
        {
            Client client = iClient as Client;
            client.Owner.Server.Broadcast("The server is shutting down.");
            client.Owner.Server.Logger.Log(LogLevel.Info, "The server is shutting down.");
            Thread.Sleep(5000);
            client.Owner.Server.Stop();
            Thread.Sleep(10);
            Console.WriteLine("Press Enter to exit.");
        }

        public void Help(IClient client)
        {
            client.SendMessage("Shuts down the server.");
        }

        public string AutoComplete(IClient client, string s)
        {
            return string.Empty;
        }
    }
}
