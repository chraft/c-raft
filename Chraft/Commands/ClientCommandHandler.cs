using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

namespace Chraft.Commands
{
    public class ClientCommandHandler : ICommandHandler
    {
        private List<IClientCommand> commands;
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
        /// <param name="Command">The name of the command to find.</param>
        /// <returns>A command with the given name.</returns>
        public ICommand Find(string Command)
        {
            foreach (IClientCommand cmd in commands.Where(cmd => cmd.Name.ToLower() == Command.ToLower()))
            {
                return cmd;
            }
            IClientCommand Cmd;
            try
            {
                Cmd = FindShort(Command) as IClientCommand;
            }
            catch (CommandNotFoundException e){ throw e; }
            if (Cmd == null) throw new CommandNotFoundException("The specified command was not found!");
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
            foreach (IClientCommand cmd in commands)
            {
                if (cmd.Shortcut.ToLower() == Shortcut.ToLower())
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
        /// <param name="command">The <see cref="IClientCommand">Command</see> to register.</param>
        public void RegisterCommand(ICommand command)
        {
            if (command is IClientCommand)
            {
                foreach (IClientCommand cmd in commands)
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
                IClientCommand Cmd = command as IClientCommand;
                Cmd.ClientCommandHandler = this;
                commands.Add(Cmd);
            }
            
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
