using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public interface CommandHandler
    {
        Command Find(string Command);
        Command FindShort(string Shortcut);
        void RegisterCommand(Command command);
        void UnregisterCommand(Command command);
        Command[] GetCommands();
    }
}
