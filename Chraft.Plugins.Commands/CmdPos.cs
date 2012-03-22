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
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;

namespace Chraft.Plugins.Commands
{
    public class CmdPos1 : IClientCommand
    {
        public CmdPos1(IPlugin plugin)
        {
            Iplugin = plugin;
        }
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            client.Point2 = UniversalCoords.FromAbsWorld(client.GetOwner().Position);
            client.SendMessage("§7First position set.");
        }

        public void Help(IClient client)
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

        public IPlugin Iplugin { get; set; }
    }
    public class CmdPos2 : IClientCommand
    {
        public CmdPos2(IPlugin plugin)
        {
            Iplugin = plugin;
        }
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            client.Point1 = UniversalCoords.FromAbsWorld(client.GetOwner().Position);
            client.SendMessage("§7Second position set.");
        }

        public void Help(IClient client)
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

        public IPlugin Iplugin { get; set; }
    }
}
