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
using System.Reflection;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Server;

namespace Chraft.Plugins.SamplePlugin
{
    [Plugin]
    public class SamplePlugin : IPlugin
    {
        private SamplePluginPlayerListener _playerListener;
        private SamplePluginEntitiyListener _entitiyListener;

        public string Name
        {
            get { return "SamplePlugin"; }
        }

        public string Author
        {
            get { return "C#raft Team"; }
        }

        public string Description
        {
            get { return "Sample Plugin"; }
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
            _playerListener = new SamplePluginPlayerListener(this);
            _entitiyListener = new SamplePluginEntitiyListener(this);
        }

        public void Associate(IServer server, IPluginManager pluginManager)
        {
            Server = server;
            PluginManager = pluginManager;
        }

        public void OnEnabled()
        {
            IsPluginEnabled = true;
            PluginManager.RegisterEvent(Event.PlayerChat, _playerListener, this);
            PluginManager.RegisterEvent(Event.PlayerDied, _playerListener, this);
            PluginManager.RegisterEvent(Event.EntitySpawn, _entitiyListener, this);
            Server.GetLogger().Log(LogLevel.Info, "Plugin {0} v{1} Enabled", Name, Version);
        }

        public void OnDisabled()
        {
            IsPluginEnabled = false;
            PluginManager.UnregisterEvent(Event.PlayerChat, _playerListener, this);
            PluginManager.UnregisterEvent(Event.PlayerDied, _playerListener, this);
            PluginManager.UnregisterEvent(Event.EntitySpawn, _entitiyListener, this);
            Server.GetLogger().Log(LogLevel.Info, "Plugin {0} v{1} Disabled", Name, Version);
        }
    }
}
