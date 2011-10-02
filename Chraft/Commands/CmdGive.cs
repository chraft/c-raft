using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces;

namespace Chraft.Commands
{
    public class CmdGive : ClientCommand
    {

        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            ItemStack item;
            uint amount = 0;
            List<Client> who = new List<Client>();

            if (tokens.Length < 2)
            {
                client.SendMessage("§cPlease specify a target and an item or just an item to give it to yourself.");
                return;
            }
            item = client.Server.Items[tokens[1]];

            if (tokens.Length == 2)
            {
                // Trying to give something to yourself
                who.Add(client);
            }
            else if (tokens.Length == 3)
            {
                // Trying to give yourself an item with amount specified
                if (uint.TryParse(tokens[2], out amount))
                {
                    if (!ItemStack.IsVoid(item))
                        who.Add(client);
                    else
                    {
                        item = client.Server.Items[tokens[2]];
                        who.AddRange(client.Server.GetClients(tokens[1]));
                    }

                }
                else
                {
                    // OR trying to give something to a player(s)
                    who.AddRange(client.Server.GetClients(tokens[1]));
                    item = client.Server.Items[tokens[2]];
                }  
            }
            else
            {
                // Trying to give item to other player with amount specified
                if (uint.TryParse(tokens[3], out amount))
                {
                    who.AddRange(client.Server.GetClients(tokens[1]));
                    item = client.Server.Items[tokens[2]];
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
                c.Inventory.AddItem(item.Type, item.Count, item.Durability);
            client.SendMessage("§7Item given to " + who.Count + " player" + (who.Count > 1 ? "s":""));
        }

        public void Help(Client client)
        {
            client.SendMessage("/give <Player> <Item OR Block> [Amount] - Gives <Player> [Amount] of <Item OR Block>.");
            client.SendMessage("/give <Item OR Block> [Amount] - Gives you [Amount] of <Item OR Block>.");
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
