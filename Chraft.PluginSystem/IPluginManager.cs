
using System.Collections.Generic;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Listener;

namespace Chraft.PluginSystem
{
    public interface IPluginManager
    {
        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="Event">The name of the event to listen for.</param>
        /// <param name="listener">The listener to notify.</param>
        /// <param name="plugin">The plugin to associate the listener with.</param>
        /// <returns>The resulting EventListener</returns>
        EventListener RegisterEvent(Event.Event Event, IChraftListener listener, IPlugin plugin);

        /// <summary>
        /// Unsubscribes from an event.
        /// </summary>
        /// <param name="Event">The name of the event.</param>
        /// <param name="listener">The listener.</param>
        /// <param name="plugin">The plugin associated with the listener.</param>
        void UnregisterEvent(Event.Event Event, IChraftListener listener, IPlugin plugin);

        /// <summary>
        /// Registers a List of Commands.
        /// </summary>
        /// <param name="commands">The List of Command.  See RegisterCommand(Command, IPlugin) for more info.</param>
        /// <param name="plugin">The IPlugin to associate the commands with.</param>
        void RegisterCommands(List<ICommand> commands, IPlugin plugin);

        /// <summary>
        /// Registers an Array of Commands.
        /// </summary>
        /// <param name="commands">The Array of Command.  See RegisterCommand(Command, IPlugin) for more info.</param>
        /// <param name="plugin">The IPlugin to associate the commands with.</param>
        void RegisterCommands(ICommand[] commands, IPlugin plugin);

        /// <summary>
        /// Registers a command with the server.
        /// </summary>
        /// <param name="cmd">The command to register.  ClientCommand or ServerCommand.</param>
        /// <param name="plugin">The plugin to associate the command with.</param>
        void RegisterCommand(ICommand cmd, IPlugin plugin);

        /// <summary>
        /// Unregisters a command.
        /// </summary>
        /// <param name="cmd">The command to unregister.</param>
        /// <param name="plugin">The plugin that the command is associated with.</param>
        void UnregisterCommand(ICommand cmd, IPlugin plugin);

        /// <summary>
        /// Unregisters an Array of Commands.
        /// </summary>
        /// <param name="commands">The Array of commands to unregister.</param>
        /// <param name="plugin">The IPlugin that is accociated with the commands.</param>
        void UnregisterCommands(ICommand[] commands, IPlugin plugin);

        /// <summary>
        /// Unregisters a List of Commands.
        /// </summary>
        /// <param name="commands">The List of commands to unregister.</param>
        /// <param name="plugin">The IPlugin that is accociated with the commands.</param>
        void UnregisterCommands(List<ICommand> commands, IPlugin plugin);

        /// <summary>
        /// Calls an event.
        /// </summary>
        /// <param name="Event">The Event to be called</param>
        /// <param name="args">The Event Args.</param>
        void CallEvent(Event.Event Event, ChraftEventArgs args);

        /// <summary>
        /// The folder searched at runtime for available plugins. Can be used by plugins to make configuration directories
        /// off this as a base. 
        /// </summary>
        string Folder { get; }
    }
}