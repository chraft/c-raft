using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public class CmdPlayers : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            client.SendMessage("Online Players: " + client.Server.Clients.Count);
            foreach (Client c in client.Server.Clients.Values)
                client.SendMessage(c.EntityId + " : " + c.DisplayName);
        }

        public void Help(Client client)
        {
            client.SendMessage("/players - Shows a list of online players.");
        }

        public string Name
        {
            get { return "players"; }
        }

        public string Shortcut
        {
            get { return "who"; }
        }

        public CommandType Type
        {
            get { return CommandType.Other; }
        }

        public string Permission
        {
            get { return "chraft.players"; }
        }
    }
}
