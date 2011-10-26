using System.Linq;
using Chraft.Net;
using Chraft.Plugins;
using Chraft.World;

namespace Chraft.Commands
{
    internal class CmdTp : IClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                client.SendMessage("§cPlease specify a target.");
                return;
            }
            Client[] targets = client.Owner.Server.GetClients(tokens[0]).ToArray();
            if (targets.Length < 1)
            {
                client.SendMessage("§cUnknown player.");
                return;
            }

            Client target = targets[0];
            client.Owner.World = target.Owner.World;
            client.Owner.TeleportTo(new AbsWorldCoords(target.Owner.Position.X, target.Owner.Position.Y, target.Owner.Position.Z));
        }

        public void Help(Client client)
        {
            client.SendMessage("/tp <Target> - Teleports you to <Target>'s location.");
        }

        public string Name
        {
            get { return "tp"; }
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
            get { return "chraft.tp"; }
        }

        public IPlugin Iplugin { get; set; }
    }
    internal class CmdSummon : IClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                client.SendMessage("§cPlease specify a target.");
                return;
            }
            Client[] targets = client.Owner.Server.GetClients(tokens[0]).ToArray();
            if (targets.Length < 1)
            {
                client.SendMessage("§cUnknown payer.");
                return;
            }
            foreach (Client c in targets)
            {
                c.Owner.World = client.Owner.World;
                c.Owner.TeleportTo(new AbsWorldCoords(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z));
            }
        }

        public void Help(Client client)
        {
            client.SendMessage("/tphere <Player> - Teleports <Player> to you.");
        }

        public string Name
        {
            get { return "tphere"; }
        }

        public string Shortcut
        {
            get { return "s"; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.tphere"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
