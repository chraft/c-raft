using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public Command[] GetCommands()
        {
            return Commands.ToArray();
        }
        private void Init()
        {
            RegisterCommand(new CmdHelp());
            RegisterCommand(new CmdStop());
        }
    }
}
