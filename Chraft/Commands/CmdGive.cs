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
            if (tokens.Length < 3)
            {
                client.SendMessage("§cPlease specify an item and target.");
                return;
            }

            ItemStack item = client.Server.Items[tokens[2]];
            if (ItemStack.IsVoid(item))
            {
                client.SendMessage("§cUnknown item.");
                return;
            }

            sbyte count = -1;
            if (tokens.Length > 3)
                sbyte.TryParse(tokens[3], out count);

            foreach (Client c in client.Server.GetClients(tokens[1]))
                c.Inventory.AddItem(item.Type, count < 0 ? item.Count : count, item.Durability);
            client.SendMessage("§7Item given.");
        }

        public void Help(Client client)
        {
            client.SendMessage("/give <Item OR Block> <Player> [Amount] - Gives <Player> [Amount] of <Item OR Block>.");
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
    }
}
