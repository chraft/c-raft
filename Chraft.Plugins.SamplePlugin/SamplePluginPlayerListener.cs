using Chraft.Plugins.Listener;
using Chraft.Utils;

namespace Chraft.Plugins.SamplePlugin
{
    class SamplePluginPlayerListener : PlayerListener
    {
        private readonly IPlugin _plugin;
        public override void OnPlayerChat(Events.Args.ClientChatEventArgs e)
        {
            if (e.EventCanceled) return;
            e.Message = ChatColor.Blue + e.Message;
        }

        public override void OnPlayerDeath(Events.Args.ClientDeathEventArgs e)
        {
            if (e.EventCanceled) return;
            _plugin.Server.Broadcast(e.Client.Owner.DisplayName + " went splat");
        }

        public SamplePluginPlayerListener(IPlugin plugin)
        {
            _plugin = plugin;
        }
    }
}
