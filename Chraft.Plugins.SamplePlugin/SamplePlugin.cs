using System;
using Chraft.Plugins.Events;

namespace Chraft.Plugins.SamplePlugin
{
    [Plugin]
    public class SamplePlugin : IPlugin
    {
        private SamplePluginPlayerListener playerListener;
        private SamplePluginEntitiyListener entitiyListener;

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
            playerListener = new SamplePluginPlayerListener(this);
        }

        public void Associate(Server server, PluginManager pluginManager)
        {
            Server = server;
            PluginManager = pluginManager;
        }

        public void OnEnabled()
        {
            PluginManager.RegisterEvent(Event.PLAYER_CHAT, playerListener, this);
            PluginManager.RegisterEvent(Event.PLAYER_DIED, playerListener, this);
            PluginManager.RegisterEvent(Event.ENTITY_SPAWN, entitiyListener, this);
            Console.WriteLine(Name + " " + Version + " Enabled");
        }

        public void OnDisabled()
        {
            PluginManager.UnregisterEvent(Event.PLAYER_CHAT, playerListener, this);
            PluginManager.UnregisterEvent(Event.PLAYER_DIED, playerListener, this);
            PluginManager.UnregisterEvent(Event.ENTITY_SPAWN, entitiyListener, this);
            Console.WriteLine(Name + " " + Version + " Disabled");
        }
    }
}
