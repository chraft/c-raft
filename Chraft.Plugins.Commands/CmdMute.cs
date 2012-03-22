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

using System.Linq;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Misc;

namespace Chraft.Plugins.Commands
{
    public class CmdMute : IClientCommand
    {
        public CmdMute(IPlugin plugin)
        {
            Iplugin = plugin;
        }
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                client.SendMessage("You must specify a player to mute");
                return;
            }

            IClient[] matchedClients = client.GetServer().GetClients(tokens[0]).ToArray();
            IClient clientToMute = null;
            if (matchedClients.Length < 1)
            {
                client.SendMessage("Unknown Player");
                return;
            }
            else if (matchedClients.Length == 1)
            {
                clientToMute = matchedClients[0];
            }
            else if (matchedClients.Length > 1)
            {
                // We've got more than 1 client. I.e. "Test" and "Test123" for the "test" pattern.
                // Looking for exact name match.
                int exactMatchClient = -1;
                for (int i = 0; i < matchedClients.Length; i++)
                {
                    if (matchedClients[i].GetOwner().DisplayName.ToLower() == tokens[0].ToLower())
                        exactMatchClient = i;
                }

                // If we found the player with the exactly same name - he is our target
                if (exactMatchClient != -1)
                {
                    clientToMute = matchedClients[exactMatchClient];
                } else
                {
                    // We do not found a proper target and aren't going to randomly punish anyone
                    client.SendMessage("More than one player found. Provide the exact name.");
                    return;
                }
            }
            bool clientMuted = clientToMute.GetOwner().IsMuted;
            clientToMute.GetOwner().IsMuted = !clientMuted;
            clientToMute.SendMessage(clientMuted ? "You have been unmuted" : "You have been muted");
            client.SendMessage(clientMuted ? clientToMute.GetOwner().DisplayName + " has been unmuted" : clientToMute.GetOwner().DisplayName + " has been muted");
        }

        public void Help(IClient client)
        {
            client.SendMessage("/mute <Target> - Mutes or unmutes <Target>.");
        }

        public string Name
        {
            get { return "mute"; }
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
            get { return "chraft.mute"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
