using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins;

namespace Chraft.Commands
{
    public interface ICommandHandler
    {
        ICommand Find(string command);
        ICommand FindShort(string Shortcut);
        void RegisterCommand(ICommand command);
        void UnregisterCommand(ICommand command);
        ICommand[] GetCommands();
    }
}
