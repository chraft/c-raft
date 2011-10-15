using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces;
using Chraft.Net;

namespace Chraft.Commands
{
    public class CmdGive : ClientCommand
    {

        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            ItemStack item;
            string itemName = string.Empty;
            short metaData = 0;
            uint amount = 0;
            List<Client> who = new List<Client>();

            if (tokens.Length < 1)
            {
                client.SendMessage("§cPlease specify a target and an item or just an item to give it to yourself.");
                return;
            }

            if (tokens[1].Contains(':'))
            {
                itemName = tokens[0].Split(':')[0].Trim();
                short.TryParse(tokens[0].Split(':')[0].Trim(), out metaData);
                item = client.Owner.Server.Items[itemName];
                item.Durability = metaData;
            }
            else
            {
                item = client.Owner.Server.Items[tokens[0]];
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
                    if (!ItemStack.IsVoid(item))
                        who.Add(client);
                    else
                    {
                        if (tokens[1].Contains(':'))
                        {
                            itemName = tokens[1].Split(':')[0].Trim();
                            short.TryParse(tokens[1].Split(':')[1].Trim(), out metaData);
                            item = client.Owner.Server.Items[itemName];
                            item.Durability = metaData;
                        }

                        else
                        {
                            item = client.Owner.Server.Items[tokens[1]];
                        }
                        who.AddRange(client.Owner.Server.GetClients(tokens[0]));
                    }

                }
                else
                {
                    // OR trying to give something to a player(s)
                    who.AddRange(client.Owner.Server.GetClients(tokens[0]));
                    if (tokens[2].Contains(':'))
                    {
                        itemName = tokens[1].Split(':')[0].Trim();
                        short.TryParse(tokens[1].Split(':')[1].Trim(), out metaData);
                        item = client.Owner.Server.Items[itemName];
                        item.Durability = metaData;
                    }
                    else
                    {
                        item = client.Owner.Server.Items[tokens[1]];
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
                        item = client.Owner.Server.Items[itemName];
                        item.Durability = metaData;
                    }
                    else
                    {
                        item = client.Owner.Server.Items[tokens[1]];
                    }
                }

            }
            
            if (ItemStack.IsVoid(item))
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
                item.Count = (sbyte)amount;
            
            foreach (Client c in who)
                c.Owner.Inventory.AddItem(item.Type, item.Count, item.Durability);
            client.SendMessage("§7Item given to " + who.Count + " player" + (who.Count > 1 ? "s":""));
        }

        public void Help(Client client)
        {
            client.SendMessage("/give <Player> <Item OR Block>[:MetaData] [Amount] - Gives <Player> [Amount] of <Item OR Block>.");
            client.SendMessage("/give <Item OR Block>[:MetaData] [Amount] - Gives you [Amount] of <Item OR Block>.");
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
    }
}
