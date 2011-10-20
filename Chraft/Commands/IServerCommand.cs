using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public interface IServerCommand : ICommand
    {
        ServerCommandHandler ServerCommandHandler { get; set; }
        void Use(Server server, string commandName, string[] tokens);
        void Help(Server server);
    }
}
