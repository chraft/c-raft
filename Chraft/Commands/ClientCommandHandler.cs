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
using System.Reflection;
using Chraft.PluginSystem.Commands;

namespace Chraft.Commands
{
    public class ClientCommandHandler : IClientCommandHandler
    {
        private List<IClientCommand> commands;
        private readonly string _chraftCoreNamespace = "Chraft";

        public ClientCommandHandler()
        {
            commands = new List<IClientCommand>();
            Init();
        }

        /// <summary>
        /// Finds a command and returns it for use.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="command">The name of the command to find.</param>
        /// <returns>A command with the given name.</returns>
        public ICommand Find(string command)
        {
            var parts = command.Split(':');

            // Ambiguous command
            if (parts.Length > 2 || (parts.Length == 2 && string.IsNullOrEmpty(parts[1])))
                throw new CommandNotFoundException("The specified command was not found!");

            // Looking exactly for the /PluginName:CommandName command
            if (parts.Length == 2)
                return FindByFullName(command);

            // Check for main command names collision
            var matchedCommands = commands.Where(c => c.Name.ToLower() == command.ToLower()).ToList();
            var matchedCount = matchedCommands.Count;

            if (matchedCount > 1)
            {
                var matchedStrings = new string[matchedCount];
                string pluginName;

                for (int i = 0; i < matchedCount; i++)
                {
                    pluginName = (matchedCommands[i].Iplugin == null) ? _chraftCoreNamespace : matchedCommands[i].Iplugin.Name;
                    matchedStrings[i] = string.Format("/{0}:{1}", pluginName, matchedCommands[i].Name);
                }
                throw new MultipleCommandsMatchException(matchedStrings);
            }

            if (matchedCount == 1)
                return matchedCommands[0];

            // We haven't found the command by its main name, checking the shortcuts
            IClientCommand cmd;
            try
            {
                cmd = FindShort(command) as IClientCommand;
            }
            catch (MultipleCommandsMatchException e)
            {
                throw e;
            }
            catch (CommandNotFoundException e)
            {
                throw e;
            }
            if (cmd == null)
            {
                throw new CommandNotFoundException("The specified command was not found!");
            }
            return cmd;
        }

        /// <summary>
        /// Finds a command by its full name (e.g. "PluginName:CommandName") and returns it for use.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="command">The name of the command to find.</param>
        /// <returns>A command with the given name.</returns>
        public ICommand FindByFullName(string command)
        {
            var commandSplit = command.Split(':');

            if (commandSplit.Length == 2)
            {
                var pluginName = commandSplit[0];
                var commandName = commandSplit[1];
                if (pluginName.ToLower() == _chraftCoreNamespace.ToLower())
                {
                    foreach (var s in commands.Where(c => c.Iplugin == null && (c.Name.ToLower() == commandName.ToLower()) || c.Shortcut.ToLower() == commandName.ToLower()))
                        return s;
                }
                else
                {
                    foreach (var s in commands.Where(c => c.Iplugin != null && c.Iplugin.Name.ToLower() == pluginName.ToLower() && (c.Name.ToLower() == commandName.ToLower() || c.Shortcut.ToLower() == commandName.ToLower())))
                        return s;
                }
            }
            throw new CommandNotFoundException("The specified command was not found!");
        }

        /// <summary>
        /// Finds a command and returns it for use.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="Shortcut">The shortcut of the command to find.</param>
        /// <returns>A command with the given shortcut.</returns>
        public ICommand FindShort(string shortcut)
        {
            // Check for commands shortcuts collision
            var matchedCommands = commands.Where(c => !string.IsNullOrEmpty(c.Shortcut) && c.Shortcut.ToLower() == shortcut.ToLower()).ToList();
            var commandsCount = matchedCommands.Count;

            if (commandsCount > 1)
            {
                var matchedStrings = new string[commandsCount];
                string pluginName;

                for (int i = 0; i < commandsCount; i++)
                {
                    pluginName = (matchedCommands[i].Iplugin == null) ? _chraftCoreNamespace : matchedCommands[i].Iplugin.Name;
                    matchedStrings[i] = string.Format("/{0}:{1}", pluginName, matchedCommands[i].Shortcut);
                }
                throw new MultipleCommandsMatchException(matchedStrings);
            }

            if (commandsCount == 1)
                return matchedCommands[0];

            throw new CommandNotFoundException("The specified command was not found!");
        }

        /// <summary>
        /// Registers a command with the server.
        /// Exceptions:
        /// <exception cref="CommandAlreadyExistsException">CommandAlreadyExistsException</exception>
        /// </summary>
        /// <param name="command">The <see cref="IClientCommand">Command</see> to register.</param>
        /// <param name="plugin"></param>
        public void RegisterCommand(ICommand command)
        {
            if (!(command is IClientCommand)) return;

            var cmd = command as IClientCommand;
            cmd.ClientCommandHandler = this;
            commands.Add(cmd);
        }
        /// <summary>
        /// Removes a command from the server.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="command">The <see cref="IClientCommand">Command</see> to remove.</param>
        public void UnregisterCommand(ICommand command)
        {
            if (command is IClientCommand)
            {
                if (commands.Contains(command))
                    commands.Remove(command as IClientCommand);
                else
                    throw new CommandNotFoundException("The given command was not found!");
            }
        }
        /// <summary>
        /// Gets an array of all of the commands registerd.
        /// </summary>
        /// <returns>Array of <see cref="IClientCommand"/></returns>
        public ICommand[] GetCommands()
        {
            return commands.ToArray();
        }
        private void Init()
        {
            foreach (Type t in from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.GetInterfaces().Contains(typeof(IClientCommand)) && !t.IsAbstract
                               select t)
            {
                RegisterCommand((IClientCommand)t.GetConstructor(Type.EmptyTypes).Invoke(null));
            }
        }

        public string AutoComplete(PluginSystem.Net.IClient client, string s)
        {
            var sb = new System.Text.StringBuilder(string.Empty);
            if (s.Length < 2 || !s.StartsWith("/"))
                return string.Empty;

            s = s.Substring(1);
            string plugin, commandName, commandStr;

            if (s.IndexOf(':') != -1)
            {
                var colonPos = s.IndexOf(':');
                var spacePos = s.IndexOf(' ');
                if (spacePos != -1 && spacePos < colonPos)
                    return string.Empty;

                plugin = s.Substring(0, colonPos);

                if (string.IsNullOrEmpty(plugin))
                    return string.Empty;

                commandStr = s.Substring(colonPos + 1);
            }
            else
            {
                plugin = string.Empty;
                commandStr = s;
            }

            if (commandStr.IndexOf(' ') != -1)
            {
                var spacePos = commandStr.IndexOf(' ');
                commandName = commandStr.Substring(0, spacePos);
                commandStr = commandStr.Substring(spacePos);
            }
            else
            {
                commandName = commandStr;
                commandStr = string.Empty;
            }

            var commandsByPlugin =
                commands.Where(
                    c =>
                    (!string.IsNullOrEmpty(plugin) &&
                     ((c.Iplugin == null && string.Compare(plugin, _chraftCoreNamespace, true) == 0) ||
                      (c.Iplugin != null && string.Compare(c.Iplugin.Name, plugin, true) == 0)))
                    || string.IsNullOrEmpty(plugin));

            var commandsByName = commandsByPlugin.Where(c => c.Name.StartsWith(commandName, StringComparison.OrdinalIgnoreCase)).ToList();
            var commandsByShortcut = commandsByPlugin.Where(c => c.Shortcut.StartsWith(commandName, StringComparison.OrdinalIgnoreCase)).ToList();

            if (commandsByName.Count == 0 && commandsByShortcut.Count == 0)
                return string.Empty;

            if (commandsByName.Count + commandsByShortcut.Count > 1)
            {
                if (commandStr.Length == 0)
                {
                    foreach (var c in commandsByName)
                        if (!string.IsNullOrEmpty(c.Name.Trim()))
                            sb.AppendFormat("/{0}{1}\0", (string.IsNullOrEmpty(plugin) ? "" : plugin + ":"), c.Name);
                    foreach (var c in commandsByShortcut)
                        if (!string.IsNullOrEmpty(c.Shortcut.Trim()))
                            sb.AppendFormat("/{0}{1}\0", (string.IsNullOrEmpty(plugin) ? "" : plugin + ":"), c.Shortcut);
                    return sb.ToString();
                }

                var nameExact = commandsByName.Where(c => string.Compare(c.Name, commandName, true) == 0).ToList();
                var shortcutExact = commandsByShortcut.Where(c => string.Compare(c.Shortcut, commandName, true) == 0).ToList();

                if (nameExact.Count > 0)
                    return sb.Append(nameExact[0].AutoComplete(client, commandStr)).ToString();
                if (shortcutExact.Count > 0)
                    return sb.Append(shortcutExact[0].AutoComplete(client, commandStr)).ToString();

                return string.Empty;
            }
            
            if (!string.IsNullOrEmpty(commandStr))
            {
                if (commandsByName.Count == 1)
                    return sb.Append(commandsByName[0].AutoComplete(client, commandStr)).ToString();

                return sb.Append(commandsByShortcut[0].AutoComplete(client, commandStr)).ToString();
            }

            if (commandsByName.Count == 1)
                return sb.AppendFormat("/{0}{1}\0", (string.IsNullOrEmpty(plugin) ? "" : plugin + ":"), commandsByName[0].Name).ToString();

            return sb.AppendFormat("/{0}{1}\0", (string.IsNullOrEmpty(plugin) ? "" : plugin + ":"), commandsByShortcut[0].Shortcut).ToString();
        }
    }
}
