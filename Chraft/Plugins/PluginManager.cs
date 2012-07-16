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
using System.IO;
using System.Reflection;
using Chraft.Commands;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Listener;
using Chraft.PluginSystem.Server;
using Chraft.Plugins.Events;

namespace Chraft.Plugins
{
    public class PluginManager : IPluginManager
    {
        /// <summary>
        /// The collection of plugins for the server. 
        /// </summary>
        private readonly List<IPlugin> _plugins = new List<IPlugin>();

        /// <summary>
        /// The server instance that is running. 
        /// </summary>
        private readonly Server _server;

        /// <summary>
        /// An EventList holding all of EventHandlers.
        /// </summary>
        private readonly EventList _pluginHooks = new EventList();

        /// <summary>
        /// A Dictionary of all plugin commands.
        /// </summary>
        internal static Dictionary<ICommand, IPlugin> PluginCommands = new Dictionary<ICommand, IPlugin>();
       
        /// <summary>
        /// The folder searched at runtime for available plugins.
        /// </summary>
        public string Folder { get; private set; }

        /// <summary>
        /// Invoked when a plugin is loaded.
        /// </summary>
        public event EventHandler<PluginEventArgs> PluginLoaded;

        /// <summary>
        /// Initializes a new PluginManager with the given plugin folder.
        /// </summary>
        /// <param name="folder">The folder to be used by LoadDefaultAssemblies.</param>
        internal PluginManager(Server server, string folder)
        {
            // Creates the plugin folder on first run if empty. 
            //TODO: Impliment permission checks. 
            if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }
            
            Folder = folder;
            _server = server;
            _pluginHooks.Add(new ClientEvent());
            _pluginHooks.Add(new PluginEvent());
            _pluginHooks.Add(new PacketEvent());
            _pluginHooks.Add(new ServerEvent());
            _pluginHooks.Add(new WorldEvent());
            _pluginHooks.Add(new EntityEvent());
            _pluginHooks.Add(new BlockEvent());
        }

        /// <summary>
        /// Gets a thread-safe array of loaded plugins.  Note that while this array is safe for enumeration, it
        /// does not necessarily ensure the thread-safety of the underlying plugins.
        /// </summary>
        /// <returns>A shallow-thread-safe array of plugins.</returns>
        public IPlugin[] GetPlugins()
        {
            return _plugins.ToArray();
        }

        /// <summary>
        /// Loads all the assemblies in the plugin folder and the current assembly.
        /// </summary>
        internal void LoadDefaultAssemblies()
        {
            // Load the internal plugins first. 
            LoadAssembly(Assembly.GetExecutingAssembly());

            // If the plugin folder is not there return. 
            if (!Directory.Exists(Folder)) return;

            // Load up the plugins in the plugin directory.
            foreach (string f in Directory.EnumerateFiles(Folder, "*.dll"))
                LoadAssembly(f);
        }

        /// <summary>
        /// Load all plugins in the given file.
        /// </summary>
        /// <param name="file">The path to the assembly containing the plugin(s).</param>
        public void LoadAssembly(string file)
        {
            LoadAssembly(Assembly.LoadFile(Path.GetFullPath(file)));
        }

        /// <summary>
        /// Load all plugins in the given assembly.
        /// </summary>
        /// <param name="asm">The assembly containing the plugin(s).</param>
        public void LoadAssembly(Assembly asm)
        {
            foreach (Type t in from t in asm.GetTypes()
                               where t.GetInterfaces().Contains(typeof(IPlugin))
                               && t.GetCustomAttributes(typeof(PluginAttribute), false).Length > 0
                               select t)
                LoadPlugin(Ctor(t));
        }

        /// <summary>
        /// Loads a plugin into the PluginManager so that it can be managed by the server.
        /// </summary>
        /// <param name="plugin">The plugin to be loaded.</param>
        public void LoadPlugin(IPlugin plugin)
        {
            //Event
            PluginEnabledEventArgs e = new PluginEnabledEventArgs(plugin);
            CallEvent(Event.PluginEnabled, e);
            if (e.EventCanceled) return;
            //End Event

            lock (_plugins)
                _plugins.Add(plugin);
            try
            {
                OnPluginLoaded(plugin);
                plugin.Associate(_server, this);
                plugin.Initialize();
                plugin.OnEnabled();
            }
            catch (Exception ex)
            {
                _server.Logger.Log(LogLevel.Fatal, plugin.Name + " cannot be loaded.  It may be out of date.");
                _server.Logger.Log(ex);
            }
        }

        private void OnPluginLoaded(IPlugin plugin)
        {
            if (PluginLoaded != null)
                PluginLoaded.Invoke(this, new PluginEventArgs(plugin));
        }

        /// <summary>
        /// Called to disable a plugin.
        /// 
        /// NOTE: this will NOT unload the assembly OR unlock the plugin DLL.
        /// </summary>
        /// <param name="plugin">The plugin to disable.</param>
        public void DisablePlugin(IPlugin plugin)
        {
            //Event
            PluginDisabledEventArgs args = new PluginDisabledEventArgs(plugin);
            CallEvent(Event.PluginDisabled, args);
            if (args.EventCanceled) return;
            //End Event

            try
            {
                //We call the plugins Dispose sub first incase it has to do 
                //Its own cleanup stuff
                plugin.OnDisabled();
                //Just in case of lazy plugin devs.
                UnregisterEvents(GetRegisterdEvents(plugin));
            }
            catch (Exception e)
            {
                _server.Logger.Log(e);
            }
            lock (_plugins)
                _plugins.Remove(plugin);
        }

        /// <summary>
        /// Disables all enabled plugins.
        /// </summary>
        public void ClosePlugins()
        {
            foreach (IPlugin p in _plugins)
            {
                DisablePlugin(p);
            }
        }

        /// <summary>
        /// Gets an array of EventListeners registerd to a plugin.
        /// </summary>
        public EventListener[] GetRegisterdEvents(IPlugin plugin)
        {
            //Use a list because of having to set a limit on an array at init.
            return (from e in _pluginHooks from el in e.Plugins where el.Plugin == plugin select el).ToArray();
        }

        /// <summary>
        /// Invokes a plugin's ".ctor" method and returns the resulting IPlugin.
        /// </summary>
        /// <param name="t">The IPlugin type to be constructed.</param>
        /// <returns>A new plugin from the type's constructor.</returns>
        public IPlugin Ctor(Type t)
        {
            return (IPlugin)t.GetConstructor(Type.EmptyTypes).Invoke(null);
        }

        /// <summary>
        /// Calls an event.
        /// </summary>
        /// <param name="Event">The Event to be called</param>
        /// <param name="args">The Event Args.</param>
        public void CallEvent(Event Event, ChraftEventArgs args)
        {
            _pluginHooks.Find(Event).CallEvent(Event, args);
        }

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="Event">The name of the event to listen for.</param>
        /// <param name="listener">The listener to notify.</param>
        /// <param name="plugin">The plugin to associate the listener with.</param>
        /// <returns>The resaulting EventListener</returns>
        public EventListener RegisterEvent(Event Event, IChraftListener listener, IPlugin plugin)
        {
            EventListener el = new EventListener(listener, plugin, Event);
            RegisterEvent(el);
            return el;
        }

        /// <summary>
        /// Subscribes to an event.
        /// </summary>
        /// <param name="Listener">The EventListener to associate the event calls with.</param>
        public void RegisterEvent(EventListener Listener)
        {
            try
            {
                _pluginHooks.Find(Listener.Event).RegisterEvent(Listener);
            }
            catch (EventNotFoundException e)
            {
                //Use the plugin's Chraft.Server to log the error.
                Listener.Plugin.Server.GetLogger().Log(e);
            }
        }

        /// <summary>
        /// Registers a List of EventListener.
        /// </summary>
        /// <param name="Listeners">The List of EventListener.</param>
        public void RegisterEvents(List<EventListener> Listeners)
        {
            RegisterEvents(Listeners.ToArray());
        }

        /// <summary>
        /// Registers an Array of EventListener.
        /// </summary>
        /// <param name="Listeners">The Array of EventListener.</param>
        public void RegisterEvents(EventListener[] Listeners)
        {
            foreach (EventListener el in Listeners)
            {
                RegisterEvent(el);
            }
        }

        /// <summary>
        /// Unsubscribes from an event.
        /// </summary>
        /// <param name="Event">The name of the event.</param>
        /// <param name="listener">The listener.</param>
        /// <param name="plugin">The plugin associated with the listener.</param>
        public void UnregisterEvent(Event Event, IChraftListener listener, IPlugin plugin)
        {
            foreach (EventListener el in _pluginHooks.Find(Event).Plugins)
            {
                if (el.Event == Event && el.Listener == listener && el.Plugin == plugin)
                {
                    UnregisterEvent(el);
                }
            }
        }

        /// <summary>
        /// Unsubscribes from an event.
        /// </summary>
        /// <param name="Listener">The EventListener to associate the event calls with.</param>
        public void UnregisterEvent(EventListener Listener)
        {
            try
            {
                _pluginHooks.Find(Listener.Event).Plugins.Remove(Listener);
            }
            catch (EventNotFoundException e)
            {
                //Use the plugin's Chraft.Server to log the error.
                Listener.Plugin.Server.GetLogger().Log(e);
            }
            catch (Exception) { }
        }

        /// <summary>
        /// Unregisters a List of EventListener.
        /// </summary>
        /// <param name="Listeners">The List of EventListener.</param>
        public void UnregisterEvents(List<EventListener> Listeners)
        {
            UnregisterEvents(Listeners.ToArray());
        }

        /// <summary>
        /// Unregisters an Array of EventListener.
        /// </summary>
        /// <param name="Listeners">The Array of EventListener.</param>
        public void UnregisterEvents(EventListener[] Listeners)
        {
            foreach (EventListener el in Listeners)
            {
                UnregisterEvent(el);
            }
        }

        /// <summary>
        /// Registers a command with the server.
        /// </summary>
        /// <param name="cmd">The command to register.  ClientCommand or ServerCommand.</param>
        /// <param name="plugin">The plugin to associate the command with.</param>
        public void RegisterCommand(ICommand cmd, IPlugin plugin)
        {
            Server server = plugin.Server as Server;
            if (cmd is IClientCommand)
            {
                try
                {
                    //Event
                    CommandAddedEventArgs e = new CommandAddedEventArgs(plugin, cmd);
                    CallEvent(Event.CommandAdded, e);
                    if (e.EventCanceled) return;
                    //End Event

                    PluginCommands.Add(cmd,plugin);
                    server.ClientCommandHandler.RegisterCommand(cmd);
                }
                catch (CommandAlreadyExistsException e)
                {
                    server.Logger.Log(e);
                }
            }
            else if (cmd is IServerCommand)
            {
                try
                {
                    //Event
                    CommandAddedEventArgs e = new CommandAddedEventArgs(plugin, cmd);
                    CallEvent(Event.CommandAdded, e);
                    if (e.EventCanceled) return;
                    //End Event

                    PluginCommands.Add(cmd, plugin);
                    server.ServerCommandHandler.RegisterCommand(cmd);
                }
                catch (CommandAlreadyExistsException e)
                {
                    server.Logger.Log(e);
                }
            }
        }

        /// <summary>
        /// Registers a List of Commands.
        /// </summary>
        /// <param name="commands">The List of Command.  See RegisterCommand(Command, IPlugin) for more info.</param>
        /// <param name="plugin">The IPlugin to associate the commands with.</param>
        public void RegisterCommands(List<ICommand> commands, IPlugin plugin)
        {
            RegisterCommands(commands.ToArray(), plugin); //No use in copying the same code from RegisterCommands.
        }

        /// <summary>
        /// Registers an Array of Commands.
        /// </summary>
        /// <param name="commands">The Array of Command.  See RegisterCommand(Command, IPlugin) for more info.</param>
        /// <param name="plugin">The IPlugin to associate the commands with.</param>
        public void RegisterCommands(ICommand[] commands, IPlugin plugin)
        {
            foreach (ICommand c in commands)
            {
                RegisterCommand(c, plugin);
            }
        }

        /// <summary>
        /// Unregisters a command.
        /// </summary>
        /// <param name="cmd">The command to unregister.</param>
        /// <param name="plugin">The plugin that the command is associated with.</param>
        public void UnregisterCommand(ICommand cmd, IPlugin plugin)
        {
            Server server = plugin.Server as Server;
            //Event
            CommandRemovedEventArgs e = new CommandRemovedEventArgs(plugin, cmd);
            CallEvent(Event.CommandAdded, e);
            if (e.EventCanceled) return;
            //End Event

            if (PluginCommands.ContainsKey(cmd))
            {
                IPlugin pluginFound;
                PluginCommands.TryGetValue(cmd, out pluginFound);
                if (plugin != pluginFound)
                    return;
                PluginCommands.Remove(cmd);
            }
            try
            {
                if (cmd is IClientCommand) server.ClientCommandHandler.UnregisterCommand(cmd);
                else if (cmd is IServerCommand) server.ServerCommandHandler.UnregisterCommand(cmd);
            }
            catch (CommandNotFoundException ex) { server.Logger.Log(ex); }
        }

        /// <summary>
        /// Unregisters an Array of Commands.
        /// </summary>
        /// <param name="commands">The Array of commands to unregister.</param>
        /// <param name="plugin">The IPlugin that is accociated with the commands.</param>
        public void UnregisterCommands(ICommand[] commands, IPlugin plugin)
        {
            foreach (ICommand c in commands)
            {
                UnregisterCommand(c, plugin);
            }
        }

        /// <summary>
        /// Unregisters a List of Commands.
        /// </summary>
        /// <param name="commands">The List of commands to unregister.</param>
        /// <param name="plugin">The IPlugin that is accociated with the commands.</param>
        public void UnregisterCommands(List<ICommand> commands, IPlugin plugin)
        {
            UnregisterCommands(commands.ToArray(), plugin);
        }
    }
}
