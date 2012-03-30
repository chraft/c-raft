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
using Chraft.PluginSystem;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;

namespace Chraft.Plugins.Commands
{
    public class CmdSpawnMob : IClientCommand
    {
        public CmdSpawnMob(IPlugin plugin)
        {
            Iplugin = plugin;
        }
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            MobType type = MobType.Sheep;
            int amount = 1;
            bool validMob = false;

            if (tokens.Length > 1)
                Int32.TryParse(tokens[1], out amount);

            if (tokens.Length > 0)
            {
                int mobId;
                Int32.TryParse(tokens[0], out mobId);
                string mobName = Enum.GetName(typeof(MobType), mobId);
                if (mobId == 0)
                {
                    if (mobId.ToString() != tokens[0])
                    {
                        Enum.TryParse(tokens[0], true, out type);
                        validMob = true;
                    }
                }
                else if (!string.IsNullOrEmpty(mobName))
                {
                    type = (MobType)Enum.Parse(typeof(MobType), mobName);
                    validMob = true;
                }
            }
            else
                validMob = true;

            if (amount < 1 || !validMob)
            {
                Help(client);
                return;
            }

            IServer server = client.GetServer();
            AbsWorldCoords position = client.GetOwner().Position;
            IMobFactory mobFactory = server.GetMobFactory();
            for (int i = 0; i < amount; i++)
            {                
                var mob = mobFactory.CreateMob(client.GetOwner().GetWorld(), server, type, null);
                mob.Position = position;
                
                //Event
                EntitySpawnEventArgs e = new EntitySpawnEventArgs(mob, mob.Position);
                server.GetPluginManager().CallEvent(Event.EntitySpawn, e);
                if (e.EventCanceled)
                    continue;
                mob.Position = e.Location;
                //End Event
                
                server.AddEntity(mob);
            }
        }

        public void Help(IClient client)
        {
            client.SendMessage("/spawnmob <Mob> [Amount] - Spawns a mob at your position.");
        }

        public string Name
        {
            get { return "spawnmob"; }
            set { }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.spawnmob"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}

