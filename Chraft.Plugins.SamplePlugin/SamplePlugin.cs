using System;
using Chraft.Plugins.Events;

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

        public Version Version { get; private set; }

        public Server Server { get; set; }
        public PluginManager PluginManager { get; set; }

        public bool IsPluginEnabled { get; set; }


        public void Initialize()
        {
            _playerListener = new SamplePluginPlayerListener(this);
            _entitiyListener = new SamplePluginEntitiyListener(this);
        }

        public void Associate(Server server, PluginManager pluginManager)
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
            Console.WriteLine(Name + " " + Version + " Enabled");
        }

        public void OnDisabled()
        {
            IsPluginEnabled = false;
            PluginManager.UnregisterEvent(Event.PlayerChat, _playerListener, this);
            PluginManager.UnregisterEvent(Event.PlayerDied, _playerListener, this);
            PluginManager.UnregisterEvent(Event.EntitySpawn, _entitiyListener, this);
            Console.WriteLine(Name + " " + Version + " Disabled");
        }
    }
}
