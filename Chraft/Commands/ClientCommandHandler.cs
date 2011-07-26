using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public class ClientCommandHandler : CommandHandler
    {
        private List<ClientCommand> commands;
        public ClientCommandHandler()
        {
            commands = new List<ClientCommand>();
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
            foreach (ClientCommand cmd in commands)
            {
                if (cmd.Name == Command)
                {
                    return cmd;
                }
            }
            ClientCommand Cmd;
            try
            {
                Cmd = FindShort(Command) as ClientCommand;
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
        public Command FindShort(string Shortcut)
        {
            foreach (ClientCommand cmd in commands)
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
        /// <param name="command">The <see cref="ClientCommand">Command</see> to register.</param>
        public void RegisterCommand(Command command)
        {
            if (command is ClientCommand)
            {
                foreach (ClientCommand cmd in commands)
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
                ClientCommand Cmd = command as ClientCommand;
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
        /// <param name="command">The <see cref="ClientCommand">Command</see> to remove.</param>
        public void UnregisterCommand(Command command)
        {
            if (command is ClientCommand)
            {
                if (commands.Contains(command))
                {
                    commands.Remove(command as ClientCommand);
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
        /// <returns>Array of <see cref="ClientCommand"/></returns>
        public Command[] GetCommands()
        {
            return commands.ToArray();
        }
        private void Init()
        {
            RegisterCommand(new CmdHelp());
            RegisterCommand(new CmdStop());
            RegisterCommand(new CmdSet());
            RegisterCommand(new CmdPos1());
            RegisterCommand(new CmdPos2());
            RegisterCommand(new CmdPlayers());
            RegisterCommand(new CmdSpawn());
            RegisterCommand(new CmdGive());
            RegisterCommand(new CmdSummon());
            RegisterCommand(new CmdMute());
            RegisterCommand(new CmdTime());
            RegisterCommand(new CmdTp());
            RegisterCommand(new CmdSetHealth());
        }
    }
}
