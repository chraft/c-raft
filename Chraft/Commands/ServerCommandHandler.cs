using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Chraft.Commands
{
    public class ServerCommandHandler : CommandHandler
    {
        private List<ServerCommand> Commands;

        public ServerCommandHandler()
        {
            Commands = new List<ServerCommand>();
            Init();
        }
        /// <summary>
        /// Finds a command and returns it for use.
        /// 
        /// Exceptions:
        /// <exception cref="CommandNotFoundException">CommandNotFoundException</exception>
        /// </summary>
        /// <param name="Command">The name of the command to find.</param>
        /// <returns>A command with the given name.</returns>
        public Command Find(string Command)
        {
            foreach (ServerCommand cmd in Commands)
            {
                if (cmd.Name == Command)
                {
                    return cmd;
                }
            }
            ServerCommand Cmd;
            try
            {
                Cmd = FindShort(Command) as ServerCommand;
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
        public Command FindShort(string Shortcut)
        {
            foreach (ServerCommand cmd in Commands)
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
        /// <param name="command">The <see cref="ServerCommand">Command</see> to register.</param>
        public void RegisterCommand(Command command)
        {
            if (command is ServerCommand)
            {
                foreach (ServerCommand cmd in Commands)
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
                ServerCommand Cmd = command as ServerCommand;
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
        /// <param name="command">The <see cref="ServerCommand">Command</see> to remove.</param>
        public void UnregisterCommand(Command command)
        {
            if (command is ServerCommand)
            {
                if (Commands.Contains(command))
                {
                    Commands.Remove(command as ServerCommand);
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
        /// <returns>Array of <see cref="ServerCommand"/></returns>
        public Command[] GetCommands()
        {
            return Commands.ToArray();
        }
        private void Init()
        {
            foreach (Type t in from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.GetInterfaces().Contains(typeof(ServerCommand)) && !t.IsAbstract
                               select t)
            {
                RegisterCommand((ServerCommand)t.GetConstructor(Type.EmptyTypes).Invoke(null));
            }
        }
    }
}
