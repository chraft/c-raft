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
using System.Collections.Generic;
using System.Linq;
using Chraft.Entity.Items;
using Chraft.Entity.Items.Base;
using Chraft.Interfaces;
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.Plugins;
using Chraft.Utilities;
using Chraft.Utilities.Misc;

namespace Chraft.Commands
{
    internal class CmdGive : IClientCommand
    {
       public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient iclient, string commandName, string[] tokens)
        {
            Client client = (Client)iclient;
            ItemInventory item;
            string itemName = string.Empty;
            short metaData = 0;
            uint amount = 0;
            List<IClient> who = new List<IClient>();

            if (tokens.Length < 1)
            {
                client.SendMessage("§cPlease specify a target and an item or just an item to give it to yourself.");
                return;
            }

            if (tokens[0].Contains(':'))
            {
                itemName = tokens[0].Split(':')[0].Trim();
                short.TryParse(tokens[0].Split(':')[1].Trim(), out metaData);
                item = client.Owner.Server.Items[itemName] as ItemInventory;
                item.Durability = metaData;
            }
            else
            {
                item = client.Owner.Server.Items[tokens[0]] as ItemInventory;
            }

            if (tokens.Length == 1)
            {
                // Trying to give something to yourself
                who.Add(client);
            }
            else if (tokens.Length == 2)
            {
                // Trying to give yourself an item with amount specified
                if (uint.TryParse(tokens[1], out amount))
                {
                    if (item != null && !ItemHelper.IsVoid(item))
                        who.Add(client);
                    else
                    {
                        if (tokens[1].Contains(':'))
                        {
                            itemName = tokens[1].Split(':')[0].Trim();
                            short.TryParse(tokens[1].Split(':')[1].Trim(), out metaData);
                            item = client.Owner.Server.Items[itemName] as ItemInventory;
                            item.Durability = metaData;
                        }

                        else
                        {
                            item = client.Owner.Server.Items[tokens[1]] as ItemInventory;
                        }
                        who.AddRange(client.Owner.Server.GetClients(tokens[0]));
                    }

                }
                else
                {
                    // OR trying to give something to a player(s)
                    who.AddRange(client.Owner.Server.GetClients(tokens[0]));
                    if (tokens[1].Contains(':'))
                    {
                        itemName = tokens[1].Split(':')[0].Trim();
                        short.TryParse(tokens[1].Split(':')[1].Trim(), out metaData);
                        item = client.Owner.Server.Items[itemName] as ItemInventory;
                        item.Durability = metaData;
                    }
                    else
                    {
                        item = client.Owner.Server.Items[tokens[1]] as ItemInventory;
                    }
                }
            }
            else
            {
                // Trying to give item to other player with amount specified
                if (uint.TryParse(tokens[2], out amount))
                {
                    who.AddRange(client.Owner.Server.GetClients(tokens[0]));
                    if (tokens[1].Contains(':'))
                    {
                        itemName = tokens[1].Split(':')[0].Trim();
                        short.TryParse(tokens[1].Split(':')[1].Trim(), out metaData);
                        item = client.Owner.Server.Items[itemName] as ItemInventory;
                        item.Durability = metaData;
                    }
                    else
                    {
                        item = client.Owner.Server.Items[tokens[1]] as ItemInventory;
                    }
                }

            }

            if (item == null || ItemHelper.IsVoid(item))
            {
                client.SendMessage("§cUnknown item.");
                return;
            }

            if (who.Count < 1)
            {
                client.SendMessage("§cUnknown player.");
                return;
            }

            if (amount > 0)
                item.Count = (sbyte) amount;

            foreach (Client c in who)
                c.Owner.Inventory.AddItem(item.Type, item.Count, item.Durability);
            client.SendMessage("§7Item given to " + who.Count + " player" + (who.Count > 1 ? "s" : ""));
        }

        public void Help(IClient client)
        {
            client.SendMessage(
                "/give <Player> <Item OR Block>[:MetaData] [Amount] - Gives <Player> [Amount] of <Item OR Block>.");
            client.SendMessage("/give <Item OR Block>[:MetaData] [Amount] - Gives you [Amount] of <Item OR Block>.");
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
            get { return "give"; }
        }

        public string Shortcut
        {
            get { return "i"; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.give"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}