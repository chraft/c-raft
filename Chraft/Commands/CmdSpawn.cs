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
using Chraft.Net;
using Chraft.Plugins;
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

        public IPlugin Iplugin { get; set; }
    }
}
