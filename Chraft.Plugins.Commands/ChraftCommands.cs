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
using System.Reflection;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Server;

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

        public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        public IServer Server { get; set; }

        public IPluginManager PluginManager { get; set; }

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
                                new CmdSetHealth(this),
                                new CmdVersion(this),
                                new CmdGiveXP(this)
                            };
        }

        public void Associate(IServer server, IPluginManager pluginManager)
        {
            Server = server;
            PluginManager = pluginManager;
        }

        public void OnEnabled()
        {
            IsPluginEnabled = true;
            PluginManager.RegisterCommands(_commands, this);
            Server.GetLogger().Log(LogLevel.Info, "Plugin {0} v{1} Enabled", Name, Version);

        }

        public void OnDisabled()
        {
            IsPluginEnabled = false;
            PluginManager.UnregisterCommands(_commands, this);
            Server.GetLogger().Log(LogLevel.Info, "Plugin {0} v{1} Disabled", Name, Version);
        }

        public override string ToString()
        {
            return Name + " " + Version;
        }
    }
}
