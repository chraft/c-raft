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
    public class CmdSetHealth : IClientCommand
    {
        public CmdSetHealth(IPlugin plugin)
        {
            Iplugin = plugin;
        }

        public IClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(IClient client, string commandName, string[] tokens)
        {
            short newHealth = 20;
            if (tokens.Length > 0)
            {
                if (!short.TryParse(tokens[0], out newHealth))
                    newHealth = 20;
            }
            client.GetOwner().SetHealth(newHealth);
        }

        public void Help(IClient client)
        {
            client.SendMessage("/sethealth <Health> - Sets your health to <Health>");
        }

        public string Name
        {
            get { return "sethealth"; }
            set { }
        }

        public string Shortcut
        {
            get { return "heal"; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.sethealth"; }
        }

        public IPlugin Iplugin { get; set; }
    }
}
