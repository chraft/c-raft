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
using Chraft.Utilities.Misc;

namespace Chraft.Plugins.Commands
{
    public class CmdTime : IClientCommand
    {
        public CmdTime(IPlugin plugin)
        {
            Iplugin = plugin;
        }
        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            int newTime = -1;
            if (tokens.Length < 1)
            {
                client.SendMessage("You must specify a time value between 0 and 24000 or <sunrise|day|sunset|night>");
                return;
            }
            if (int.TryParse(tokens[0], out newTime) && newTime >= 0 && newTime <= 24000)
            {
                client.GetOwner().GetWorld().Time = newTime;
            }
            else if (tokens[0].ToLower() == "sunrise")
            {
                client.GetOwner().GetWorld().Time = 0;
            }
            else if (tokens[0].ToLower() == "day")
            {
                client.GetOwner().GetWorld().Time = 6000;
            }
            else if (tokens[0].ToLower() == "sunset")
            {
                client.GetOwner().GetWorld().Time = 12000;
            }

            else if (tokens[0].ToLower() == "night")
            {
                client.GetOwner().GetWorld().Time = 18000;
            }
            else
            {
                client.SendMessage("You must specify a time value between 0 and 24000 or <sunrise|day|sunset|night>");
                return;
            }

            client.GetServer().BroadcastTimeUpdate(client.GetOwner().GetWorld());
        }

        public void Help(IClient client)
        {
            client.SendMessage("/time <Sunrise | Day | Sunset | Night | Raw> - Sets the time.");
        }

        public string Name
        {
            get { return "time"; }
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
            get { return "chraft.time"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
