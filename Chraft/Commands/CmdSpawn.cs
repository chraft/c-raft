using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;

namespace Chraft.Commands
{
    public class CmdSpawn : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            client.Owner.TeleportTo(UniversalCoords.ToAbsWorld(client.Owner.World.Spawn));
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
