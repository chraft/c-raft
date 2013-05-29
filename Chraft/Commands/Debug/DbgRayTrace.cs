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
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utilities.Misc;
using Chraft.Utils;
using Chraft.World;

namespace Chraft.Commands.Debug
{
    public class DbgRayTrace : IClientCommand
    {
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient iClient, string commandName, string[] tokens)
        {
            Client client = iClient as Client;
            Vector3 facing = new Vector3(client.Owner.Yaw, client.Owner.Pitch);

            Vector3 start = new Vector3(client.Owner.Position.X, client.Owner.Position.Y + client.Owner.EyeHeight, client.Owner.Position.Z);
            Vector3 end = facing * 100 + start;
            if (end.Y < 0)
            {
                end = end * (Math.Abs(end.Y) / start.Y);
                end.Y = 0;
            }

            if (tokens.Length == 0)
            {
                // Ray trace out along client facing direction
                RayTraceHitBlock hit = client.Owner.World.RayTraceBlocks(new AbsWorldCoords(start), new AbsWorldCoords(end));

                if (hit == null)
                    client.SendMessage(String.Format("No block targetted within {0} metres", start.Distance(end)));
                else
                {
                    client.SendMessage(String.Format("{0} metres to {1}", start.Distance(hit.Hit), hit.ToString()));
                }
            }
            else if (tokens[0] == "destroy") // destroy the targetted block
            {
                RayTraceHitBlock hit = client.Owner.World.RayTraceBlocks(new AbsWorldCoords(start), new AbsWorldCoords(end));

                if (hit != null)
                {
                    client.Owner.World.SetBlockAndData(hit.TargetBlock, 0, 0);
                }
            }
            else if (tokens[0] == "pink") // make the targetted block pink wool
            {
                RayTraceHitBlock hit = client.Owner.World.RayTraceBlocks(new AbsWorldCoords(start), new AbsWorldCoords(end));

                if (hit != null)
                {
                    client.Owner.World.SetBlockAndData(hit.TargetBlock, 35, 6);
                }
            }
            else if (tokens[0] == "perf") // performance check
            {
                DateTime startTime = DateTime.Now;
                RayTraceHitBlock hit = null;
                for (int i = 0; i < 1000; i++)
                {
                    hit = client.Owner.World.RayTraceBlocks(new AbsWorldCoords(start), new AbsWorldCoords(end));
                }

                DateTime endTime = DateTime.Now;
                if (hit != null)
                {
                    client.SendMessage(String.Format("Time to ray trace {0} metres (with hit):", start.Distance(hit.Hit)));
                }
                else
                {
                    client.SendMessage(String.Format("Time to ray trace {0} metres:", start.Distance(end)));
                }
                client.SendMessage(((endTime - startTime).TotalMilliseconds / 1000.0).ToString() + " ms");
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
            get { return "dbgraytrace"; }
        }

        public string Shortcut
        {
            get { return "dbgray"; }
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

