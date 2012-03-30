
using System.Collections.Generic;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Listener;

namespace Chraft.PluginSystem
{
    public interface IPluginManager
    {
        EventListener RegisterEvent(Event.Event Event, IChraftListener Listener, IPlugin Plugin);
        void UnregisterEvent(Event.Event Event, IChraftListener Listener, IPlugin Plugin);
        void RegisterCommands(List<ICommand> commands, IPlugin Plugin);
        void RegisterCommands(ICommand[] commands, IPlugin Plugin);
        void RegisterCommand(ICommand cmd, IPlugin plugin);
        void UnregisterCommand(ICommand cmd, IPlugin plugin);
        void UnregisterCommands(ICommand[] commands, IPlugin Plugin);
        void UnregisterCommands(List<ICommand> commands, IPlugin Plugin);
        void CallEvent(Event.Event Event, ChraftEventArgs args);
    }
}