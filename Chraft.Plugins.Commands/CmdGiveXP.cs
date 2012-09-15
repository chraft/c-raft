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
    public class CmdGiveXP : IClientCommand
    {
        public CmdGiveXP(IPlugin plugin)
        {
            Iplugin = plugin;
        }

        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            short amount;
            IClient target;

            if (tokens.Length == 1)
            {
                if (short.TryParse(tokens[0], out amount))
                {
                    client.GetOwner().AddExperience(amount);
                    client.SendMessage(string.Format("{0}You has been granted with {1} exp", ChatColor.Red, amount));
                    return;
                }
                Help(client);
                return;
            }
            
            if (tokens.Length == 2)
            {
                IClient[] matchedClients = client.GetServer().GetClients(tokens[0]).ToArray();
                if (matchedClients.Length < 1)
                {
                    client.SendMessage("Unknown Player");
                    return;
                }
                if (matchedClients.Length == 1)
                {
                    target = matchedClients[0];
                }
                else
                {
                    int exactMatchClient = -1;
                    for (int i = 0; i < matchedClients.Length; i++)
                    {
                        if (matchedClients[i].GetOwner().DisplayName.ToLower() == tokens[0].ToLower())
                            exactMatchClient = i;
                    }

                    // If we found the player with the exactly same name - he is our target
                    if (exactMatchClient != -1)
                    {
                        target = matchedClients[exactMatchClient];
                    }
                    else
                    {
                        // We do not found a proper target and aren't going to randomly punish anyone
                        client.SendMessage("More than one player found. Provide the exact name.");
                        return;
                    }
                }

                if (short.TryParse(tokens[1], out amount))
                {
                    target.GetOwner().AddExperience(amount);
                    target.SendMessage(string.Format("{0}{1} has been granted with {2} exp", ChatColor.Red, target.GetOwner().DisplayName, amount));
                }
                else
                {
                    Help(client);
                }
                return;
            }

            Help(client);
        }

        public void Help(IClient client)
        {
            client.SendMessage("/givexp [Player] <amount> - gives the player an experience");
        }

        public string Name
        {
            get { return "givexp"; }
            set { }
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
            get { return "chraft.givexp"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
