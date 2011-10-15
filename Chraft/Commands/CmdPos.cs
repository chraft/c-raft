using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.World;

namespace Chraft.Commands
{
    public class CmdPos1 : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            client.Point2 = UniversalCoords.FromAbsWorld(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z);
            client.SendMessage("§7First position set.");
        }

        public void Help(Client client)
        {
            client.SendMessage("/pos1 - Sets the first cuboid position to your current location.");
        }

        public string Name
        {
            get { return "pos1"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Build; }
        }

        public string Permission
        {
            get { return "chraft.pos1"; }
        }
    }
    public class CmdPos2 : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            client.Point1 = UniversalCoords.FromAbsWorld(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z);
            client.SendMessage("§7Second position set.");
        }

        public void Help(Client client)
        {
            client.SendMessage("/pos2 - Sets the first cuboid position to your current location.");
        }

        public string Name
        {
            get { return "pos2"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Build; }
        }

        public string Permission
        {
            get { return "chraft.pos2"; }
        }
    }
}
