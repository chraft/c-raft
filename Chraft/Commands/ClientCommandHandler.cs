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
using Chraft.Plugins;

namespace Chraft.Commands
{
    public class ClientCommandHandler : IClientCommandHandler
    {
        private List<IClientCommand> commands;
        private List<IClientCommand> fallBackCommands;
        public ClientCommandHandler()
        {
            commands = new List<IClientCommand>();
            fallBackCommands = new List<IClientCommand>();
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
            var commandSplit = command.Split(':');
            if (commandSplit.Length == 2)
            {
                var test1 = commandSplit[0];
                string test2 = commandSplit[1];
                foreach (var s in PluginManager.PluginCommands.Where(c => c.Key.Name.ToLower() == test2.ToLower() && c.Value.Name.ToLower() == test1.ToLower()))
                {
                    return s.Key;
                }
            }
            foreach (IClientCommand cmd in commands.Where(cmd => cmd.Name.ToLower() == command.ToLower()))
            {
                return cmd;
            }
            IClientCommand Cmd;
            try
            {
                Cmd = FindShort(command) as IClientCommand;
            }
            catch (CommandNotFoundException e)
            {
                throw e;
            }
            if (Cmd == null)
            {
                throw new CommandNotFoundException("The specified command was not found!");
            }
            return Cmd;
        }
        /// <summary>
        /// Finds a command and returns it for use.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="Shortcut">The shortcut of the command to find.</param>
        /// <returns>A command with the given shortcut.</returns>
        public ICommand FindShort(string Shortcut)
        {
            foreach (IClientCommand cmd in commands.Where(cmd => cmd.Shortcut.ToLower() == Shortcut.ToLower()))
            {
                return cmd;
            }
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
                {
                    commands.Remove(command as IClientCommand);
                }
                else if (fallBackCommands.Contains(command))
                {
                    fallBackCommands.Remove(command as IClientCommand);
                }
                else
                {
                    throw new CommandNotFoundException("The given command was not found!");
                }
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
    }
}
