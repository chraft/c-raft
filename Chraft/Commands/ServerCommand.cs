using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public interface ServerCommand : Command
    {
        ServerCommandHandler ServerCommandHandler { get; set; }
        void Use(Server server, string[] tokens);
        void Help(Server server);
    }
}
