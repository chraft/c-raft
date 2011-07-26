using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public interface ClientCommand : Command
    {
        ClientCommandHandler ClientCommandHandler { get; set; }
        void Use(Client client, string[] tokens);
        void Help(Client client);
    }
}
