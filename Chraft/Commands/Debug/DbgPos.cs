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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.Plugins;
using Chraft.Utilities;
using Chraft.Utilities.Math;
using Chraft.Utilities.Misc;
using Chraft.Utils;

namespace Chraft.Commands.Debug
{
    public class DbgPos : IClientCommand
    {
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient iClient, string commandName, string[] tokens)
        {
            Client client = iClient as Client;
            if (tokens.Length == 0)
            {
                client.SendMessage(String.Format("§7Your position: X={0:0.00},Y={1:0.00},Z={2:0.00}, Yaw={3:0.00}, Pitch={4:0.00}", client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z, client.Owner.Yaw, client.Owner.Pitch));
            }
            else if (tokens[0] == "yaw")
            {
                Vector3 z1 = client.Owner.Position.ToVector() + Vector3.ZAxis;
                Vector3 posToZ1 = (client.Owner.Position.ToVector() - z1);

                client.SendMessage(String.Format("§7Player.Position.Yaw {0:0.00}, vector computed yaw (SignedAngle) {1:0.00}", client.Owner.Yaw % 360, Vector3.ZAxis.SignedAngle(Vector3.ZAxis.Yaw(client.Owner.Yaw.ToRadians()), Vector3.ZAxis.Yaw(client.Owner.Yaw.ToRadians()).Yaw(90.0.ToRadians())).ToDegrees()));
                client.SendMessage(String.Format("§7Normalised facing Yaw: " + new Vector3(client.Owner.Position.X, client.Owner.Position.Y, client.Owner.Position.Z).Normalize().Yaw(client.Owner.Yaw % 360).ToString()));
            }
        }

        public void Help(IClient client)
        {

        }

        public string AutoComplete(IClient client, string s)
        {
            return string.Empty;
        }

        public string Name
        {
            get { return "dbgpos"; }
        }

        public string Shortcut
        {
            get { return String.Empty; }
        }

        public CommandType Type
        {
            get { return CommandType.Information; }
        }

        public string Permission
        {
            get { return "chraft.debug"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
