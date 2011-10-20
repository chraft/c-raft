using Chraft.Plugins.Listener;

namespace Chraft.Plugins.SamplePlugin
{
    class SamplePluginEntitiyListener : EntityListener
    {

        private readonly IPlugin _plugin;

        public override void OnSpawn(Events.Args.EntitySpawnEventArgs e)
        {
            if (e.EventCanceled) return;
            _plugin.Server.Broadcast(e.Entity.GetType() + " Spawned");
        }

        public SamplePluginEntitiyListener(IPlugin plugin)
        {
            _plugin = plugin;
        }
    }
}
