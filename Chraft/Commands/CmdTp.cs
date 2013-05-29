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
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.Plugins;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World;

namespace Chraft.Commands
{
    internal class CmdTp : IClientCommand
    {
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient iClient, string commandName, string[] tokens)
        {
            Client client = iClient as Client;
            if (tokens.Length < 1)
            {
                client.SendMessage("§cPlease specify a target.");
                return;
            }
            Client[] targets = client.Owner.Server.GetClients(tokens[0]).ToArray() as Client[];
            if (targets.Length < 1)
            {
                client.SendMessage("§cUnknown player.");
                return;
            }

            Client target = targets[0];
            client.Owner.World = target.Owner.World;
            client.Owner.TeleportTo(new AbsWorldCoords(target.Owner.Position.X, target.Owner.Position.Y, target.Owner.Position.Z));
        }
        
        public void Help(IClient client)
        {
            client.SendMessage("/tp <Target> - Teleports you to <Target>'s location.");
        }

        public string AutoComplete(IClient client, string s)
        {
            if (string.IsNullOrEmpty(s.Trim()))
                return string.Empty;

            if (s.TrimStart().IndexOf(' ') != -1)
                return string.Empty;

            return PluginSystem.Commands.AutoComplete.GetPlayers(client, s.Trim());
        }

        public string Name
        {
            get { return "tp"; }
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
            get { return "chraft.tp"; }
        }

        public IPlugin Iplugin { get; set; }
    }
    internal class CmdSummon : IClientCommand
    {
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient iClient, string commandName, string[] tokens)
        {
            Client client = iClient as Client;
            if (tokens.Length < 1)
            {
                client.SendMessage("§cPlease specify a target.");
                return;
            }
            Client[] targets = client.Owner.Server.GetClients(tokens[0]).ToArray() as Client[];
            if (targets.Length < 1)
            {
                client.SendMessage("§cUnknown payer.");
                return;
            }
            foreach (Client c in targets)
            {
                c.Owner.World = client.Owner.World;
                c.Owner.TeleportTo(new AbsWorldCoords(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z));
            }
        }

        public void Help(IClient client)
        {
            client.SendMessage("/tphere <Player> - Teleports <Player> to you.");
        }

        public string AutoComplete(IClient client, string s)
        {
            if (string.IsNullOrEmpty(s.Trim()))
                return string.Empty;

            if (s.TrimStart().IndexOf(' ') != -1)
                return string.Empty;

            return PluginSystem.Commands.AutoComplete.GetPlayers(client, s.Trim());
        }

        public string Name
        {
            get { return "tphere"; }
        }

        public string Shortcut
        {
            get { return "s"; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.tphere"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
