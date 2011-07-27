using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;
using Chraft.Interfaces;

namespace Chraft.Commands
{
    public class CmdSet : ClientCommand
    {

        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            if (client.Point2 == null || client.Point1 == null)
            {
                client.SendMessage("§cPlease select a cuboid first.");
                return;
            }

            PointI start = client.SelectionStart.Value;
            PointI end = client.SelectionEnd.Value;

            ItemStack item = client.Server.Items[tokens[2]];
            if (ItemStack.IsVoid(item))
            {
                client.SendMessage("§cUnknown item.");
                return;
            }

            if (item.Type > 255)
            {
                client.SendMessage("§cInvalid item.");
            }

            for (int x = start.X; x <= end.X; x++)
            {
                for (int y = start.Y; y <= end.Y; y++)
                {
                    for (int z = start.Z; z <= end.Z; z++)
                    {
                        client.World.SetBlockAndData(x, y, z, (byte)item.Type, (byte)item.Durability);
                    }
                }
            }
        }

        public void Help(Client client)
        {
            client.SendMessage("/set <Block> - Sets the selected cuboid to <Block>");
        }

        public string Name
        {
            get { return "set"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Build; }
        }
    }
}
