using Chraft.Net;
using Chraft.World;

namespace Chraft.Commands
{
    internal class CmdSpawn : IClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
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
