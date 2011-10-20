using Chraft.Net;

namespace Chraft.Commands
{
    internal class CmdPlayers : IClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            client.SendMessage("Online Players: " + client.Owner.Server.Clients.Count);
            foreach (Client c in client.Owner.Server.GetAuthenticatedClients())
                client.SendMessage(c.Owner.EntityId + " : " + c.Owner.DisplayName);
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
