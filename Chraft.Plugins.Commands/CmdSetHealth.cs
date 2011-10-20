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
            if (tokens.Length < 1)
            {
                SetHealth(client, 20);
                return;
            }
            SetHealth(client, short.Parse(tokens[0]));
        }
        private void SetHealth(Client client, short health)
        {
            if (health > 20)
            {
                health = 20;
            }
            client.SendPacket(new UpdateHealthPacket { Health = health });
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
