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
using Chraft.Plugins;
using Chraft.Plugins.Events;
using Chraft.Commands;
using Chraft.Plugins.Listener;


namespace Chraft.Plugins.Commands
{
    [Plugin]
    public class ChraftCommands : IPlugin
    {
        private List<ICommand> _commands;
        public string Name
        {
            get { return "ChraftCommands"; }
        }

        public string Author
        {
            get { return "C#raft Team"; }
        }

        public string Description
        {
            get { return "Collection of base commands"; }
        }

        public string Website
        {
            get { return "http://www.c-raft.com"; }
        }

        public Version Version { get; private set; }

        public Server Server { get; set; }

        public PluginManager PluginManager { get; set; }

        public bool IsPluginEnabled { get; set; }

        public void Initialize()
        {
            _commands = new List<ICommand>
                            {
                                new CmdSet(this),
                                new CmdTime(this),
                                new CmdPos1(this),
                                new CmdPos2(this),
                                new CmdSpawnMob(this),
                                new CmdMute(this),
                                new CmdSetHealth(this)
                            };
        }

        public void Associate(Server server, PluginManager pluginManager)
        {
            Server = server;
            PluginManager = pluginManager;
        }

        public void OnEnabled()
        {
            IsPluginEnabled = true;
            PluginManager.RegisterCommands(_commands, this);
            Console.WriteLine("ChraftCommands Enabled");

        }

        public void OnDisabled()
        {
            IsPluginEnabled = false;
            PluginManager.UnregisterCommands(_commands, this);
            Console.WriteLine("ChraftCommands Disabled");
        }

        public override string ToString()
        {
            return "ChraftCommands " + Version;
        }
    }
}
