using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public class CmdSpawn : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            client.TeleportTo(client.World.Spawn.X, client.World.Spawn.Y + 1, client.World.Spawn.Z);
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
    }
}
