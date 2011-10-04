using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;

namespace Chraft.Commands
{
    public class CmdSpawn : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            client.Owner.TeleportTo(client.Owner.World.Spawn.X, client.Owner.World.Spawn.Y, client.Owner.World.Spawn.Z);
        }

        public void Help(Client client)
        {
            client.SendMessage("/spawn - Teleports you to the spawn.");
        }

        public string Name
        {
            get { return "spawn"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Other; }
        }

        public string Permission
        {
            get { return "chraft.spawn"; }
        }
    }
}
