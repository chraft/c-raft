using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public interface ICommandHandler
    {
        ICommand Find(string Command);
        ICommand FindShort(string Shortcut);
        void RegisterCommand(ICommand command);
        void UnregisterCommand(ICommand command);
        ICommand[] GetCommands();
    }
}
