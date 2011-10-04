using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;

namespace Chraft.Commands
{
    public class CmdTp : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            if (tokens.Length < 2)
            {
                client.SendMessage("§cPlease specify a target.");
                return;
            }
            Client[] targets = client.Owner.Server.GetClients(tokens[1]).ToArray();
            if (targets.Length < 1)
            {
                client.SendMessage("§cUnknown player.");
                return;
            }
            client.Owner.World = targets[0].Owner.World;
            client.Owner.TeleportTo(targets[0].Owner.Position.X, targets[0].Owner.Position.Y, targets[0].Owner.Position.Z);
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
    }
    public class CmdSummon : ClientCommand
    {

        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            if (tokens.Length < 2)
            {
                client.SendMessage("§cPlease specify a target.");
                return;
            }
            Client[] targets = client.Owner.Server.GetClients(tokens[1]).ToArray();
            if (targets.Length < 1)
            {
                client.SendMessage("§cUnknown payer.");
                return;
            }
            foreach (Client c in targets)
            {
                c.Owner.World = client.Owner.World;
                c.Owner.TeleportTo(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z);
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
    }
}
