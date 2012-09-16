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
using System.Linq;
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Plugins;
using Chraft.Utilities;
using Chraft.Utilities.Misc;

namespace Chraft.Commands
{
    internal class CmdSay : IClientCommand, IServerCommand
    {
        public IClientCommandHandler ClientCommandHandler { get; set; }
        public IServerCommandHandler ServerCommandHandler { get; set; }
        public void Use(IClient iClient, string commandName, string[] tokens)
        {
            Client client = iClient as Client;
            client.Owner.Server.Broadcast(tokens.Aggregate("", (current, t) => current + (t + " ")));
        }

        public void Help(IClient client)
        {
            client.SendMessage("/say <Message> - broadcasts a message to the server.");
        }

        public string AutoComplete(IClient client, string str)
        {
            if (string.IsNullOrEmpty(str.Trim()))
                return string.Empty;
            var parts = str.Split(new []{' '}, StringSplitOptions.RemoveEmptyEntries);
            return PluginSystem.Commands.AutoComplete.GetPlayers(client, parts[parts.Length - 1].Trim());
        }

        public string Name
        {
            get { return "say"; }
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
            get { return "chraft.say"; }
        }

        public IPlugin Iplugin { get; set; }

        public void Use(IServer iServer, string commandName, string[] tokens)
        {
            Server server = iServer as Server;
            string message = "";
            //for loop that starts at one so that we do not include "say".
            for (int i = 1; i < tokens.Length; i++)
            {
                message += tokens[i] + " ";
            }

            //Event
            ServerChatEventArgs e = new ServerChatEventArgs(server, message);
            server.PluginManager.CallEvent(Event.ServerChat, e);
            if (e.EventCanceled) return;
            message = e.Message;
            //End Event

            server.Broadcast(message);
        }

        public void Help(IServer server)
        {
            server.GetLogger().Log(LogLevel.Info, "/say <message> - broadcasts a message to the server.");
        }
    }
}
