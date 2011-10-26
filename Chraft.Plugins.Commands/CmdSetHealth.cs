using Chraft.Commands;
using Chraft.Net;
using Chraft.Net.Packets;

namespace Chraft.Plugins.Commands
{
    public class CmdSetHealth : IClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            short newHealth = 20;
            if (tokens.Length > 1)
            {
                if (!short.TryParse(tokens[0], out newHealth))
                    newHealth = 20;
            }
            client.Owner.SetHealth(newHealth);
        }

        public void Help(Client client)
        {
            client.SendMessage("/sethealth <Health> - Sets your health to <Health>");
        }

        public string Name
        {
            get { return "sethealth"; }
        }

        public string Shortcut
        {
            get { return "heal"; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.sethealth"; }
        }
    }
}
