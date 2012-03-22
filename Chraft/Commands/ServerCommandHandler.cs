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
    public class ServerCommandHandler : IServerCommandHandler
    {
        private List<IServerCommand> Commands;

        public ServerCommandHandler()
        {
            Commands = new List<IServerCommand>();
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
            foreach (IServerCommand cmd in Commands)
            {
                if (cmd.Name == command)
                {
                    return cmd;
                }
            }
            IServerCommand Cmd;
            try
            {
                Cmd = FindShort(command) as IServerCommand;
                return Cmd;
            }
            catch { }
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
        public ICommand FindShort(string Shortcut)
        {
            foreach (IServerCommand cmd in Commands)
            {
                if (cmd.Shortcut == Shortcut)
                {
                    return cmd;
                }
            }
            throw new CommandNotFoundException("The specified command was not found!");
        }
        /// <summary>
        /// Registers a command with the server.
        /// Exceptions:
        /// <exception cref="CommandAlreadyExistsException">CommandAlreadyExistsException</exception>
        /// </summary>
        /// <param name="command">The <see cref="IServerCommand">Command</see> to register.</param>
        public void RegisterCommand(ICommand command)
        {
            if (command is IServerCommand)
            {
                foreach (IServerCommand cmd in Commands)
                {
                    if (cmd.Name == command.Name)
                    {
                        throw new CommandAlreadyExistsException("A command with the same name already exists!");
                    }
                    else if (cmd.Shortcut == command.Shortcut && !string.IsNullOrEmpty(cmd.Shortcut))
                    {
                        throw new CommandAlreadyExistsException("A command with the same shortcut already exists!");
                    }
                }
                IServerCommand Cmd = command as IServerCommand;
                Cmd.ServerCommandHandler = this;
                Commands.Add(Cmd);
            }
        }
        /// <summary>
        /// Removes a command from the server.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="command">The <see cref="IServerCommand">Command</see> to remove.</param>
        public void UnregisterCommand(ICommand command)
        {
            if (command is IServerCommand)
            {
                if (Commands.Contains(command))
                {
                    Commands.Remove(command as IServerCommand);
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
        /// <returns>Array of <see cref="IServerCommand"/></returns>
        public ICommand[] GetCommands()
        {
            return Commands.ToArray();
        }
        private void Init()
        {
            foreach (Type t in from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.GetInterfaces().Contains(typeof(IServerCommand)) && !t.IsAbstract
                               select t)
            {
                RegisterCommand((IServerCommand)t.GetConstructor(Type.EmptyTypes).Invoke(null));
            }
        }
    }
}
