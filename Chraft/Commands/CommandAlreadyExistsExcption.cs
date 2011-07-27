using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public class CommandAlreadyExistsException : Exception
    {
        public CommandAlreadyExistsException() : base() { }
        public CommandAlreadyExistsException(string Message) : base(Message) { }
    }
}
