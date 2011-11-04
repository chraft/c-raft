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
using Chraft.Plugins;

[assembly: InternalsVisibleTo("ChraftServer")]
namespace Chraft.Commands
{
   
    internal class CmdStop : IServerCommand, IClientCommand
    {
        public ServerCommandHandler ServerCommandHandler { get; set; }
        public ClientCommandHandler ClientCommandHandler { get; set; }
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

        public void Use(Server server, string commandName, string[] tokens)
        {
            server.Broadcast("The server is shutting down.");
            server.Logger.Log(Logger.LogLevel.Info, "The server is shutting down.");
            Thread.Sleep(5000);
            server.Stop();
            Thread.Sleep(10);
            Console.WriteLine("Press Enter to exit.");
        }

        public void Help(Server server)
        {
            server.Logger.Log(Logger.LogLevel.Info, "Shuts down the server.");
        }
        public void Use(Client client, string commandName, string[] tokens)
        {
            client.Owner.Server.Broadcast("The server is shutting down.");
            client.Owner.Server.Logger.Log(Logger.LogLevel.Info, "The server is shutting down.");
            Thread.Sleep(5000);
            client.Owner.Server.Stop();
            Thread.Sleep(10);
            Console.WriteLine("Press Enter to exit.");
        }

        public void Help(Client client)
        {
            client.SendMessage("Shuts down the server.");
        }
    }
}
