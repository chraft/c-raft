using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;

namespace Chraft.Commands
{
    public interface IClientCommand : ICommand
    {
        ClientCommandHandler ClientCommandHandler { get; set; }
        void Use(Client client,string commandName, string[] tokens);
        void Help(Client client);
    }
}