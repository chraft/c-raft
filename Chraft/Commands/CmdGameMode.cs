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
using Chraft.Net.Packets;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.Plugins;
using Chraft.Utilities;
using Chraft.Utilities.Misc;
using Chraft.Utils;

namespace Chraft.Commands
{
    internal class CmdGameMode : IClientCommand
    {
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient iclient, string commandName, string[] tokens)
        {
            Client client = iclient as Client;
            switch (tokens.Length)
            {
                case 0:
                    ChangeGameMode(client, client.Owner.GameMode == 0 ? 1 : 0);
                    break;
                case 2:
                    if (Int32.Parse(tokens[1]) != 0)
                    {
                        if (Int32.Parse(tokens[1]) != 1)
                        {
                            Help(client);
                            break;
                        }
                    }
                    Client c = client.Owner.Server.GetClients(tokens[0]).FirstOrDefault() as Client;
                    if (c != null)
                    {
                        if (c.Owner.GameMode == Convert.ToByte(tokens[1]))
                        {
                            client.SendMessage(ChatColor.Red + "Player is already in that mode");
                            break;
                        }
                        ChangeGameMode(client, Int32.Parse(tokens[1]));
                        break;
                    }
                    client.SendMessage(string.Format(ChatColor.Red + "Player {0} not found", tokens[0]));
                    break;
                default:
                    Help(client);
                    break;
            }
        }

        private static void ChangeGameMode(Client client, int mode)
        {
            client.SendPacket(new NewInvalidStatePacket
            {
                GameMode = client.Owner.GameMode = Convert.ToByte(mode),
                Reason = NewInvalidStatePacket.NewInvalidReason.ChangeGameMode
            });
        }

        public void Help(IClient client)
        {
            (client as Client).SendMessage("/gamemode <player> [0|1]");
        }

        public string Name
        {
            get { return "gamemode"; }
        }



        public string Shortcut
        {
            get { return "gm"; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.gamemode"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
