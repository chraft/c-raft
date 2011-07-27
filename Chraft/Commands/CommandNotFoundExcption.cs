using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public class CommandNotFoundException : Exception
    {
        public CommandNotFoundException() : base() { }
        public CommandNotFoundException(string Message) : base(Message) { } 
    }
}
