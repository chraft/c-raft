#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion

using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;

namespace Chraft.Plugins.Commands
{
    public class CmdSet : IClientCommand
    {
        public CmdSet(IPlugin plugin)
        {
            Iplugin = plugin;
        }
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            if (client.Point2 == null || client.Point1 == null)
            {
                client.SendMessage("§cPlease select a cuboid first.");
                return;
            }

            UniversalCoords start = client.SelectionStart.Value;
            UniversalCoords end = client.SelectionEnd.Value;

            IItemStack item = client.GetOwner().GetServer().GetItemDb().GetItemStack(tokens[0]);
            if (item == null || item.IsVoid())
            {
                client.SendMessage("§cUnknown item.");
                return;
            }

            if (item.Type > 255)
            {
                client.SendMessage("§cInvalid item.");
            }

            for (int x = start.WorldX; x <= end.WorldX; x++)
            {
                for (int y = start.WorldY; y <= end.WorldY; y++)
                {
                    for (int z = start.WorldZ; z <= end.WorldZ; z++)
                    {
                        client.GetOwner().GetWorld().SetBlockAndData(UniversalCoords.FromWorld(x, y, z), (byte)item.Type, (byte)item.Durability);
                    }
                }
            }
        }

        public void Help(IClient client)
        {
            client.SendMessage("/set <Block> - Sets the selected cuboid to <Block>");
        }

        public string Name
        {
            get { return "set"; }
            set { }
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
            get { return "chraft.set"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
