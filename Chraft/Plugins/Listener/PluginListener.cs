using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Listener
{
    public class PluginListener : IChraftListener
    {
        public virtual void OnPluginEnabled(PluginEnabledEventArgs e) { }
        public virtual void OnPluginDisabled(PluginDisabledEventArgs e) { }
        public virtual void OnPluginCommandAdded(CommandAddedEventArgs e) { }
        public virtual void OnPluginCommandRemoved(CommandRemovedEventArgs e) { }
        public virtual void OnPluginListStatusChanged() { }
    }
}
