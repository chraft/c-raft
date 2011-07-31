using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Plugins.Events.Args
{
    public class EventArgsException : Exception
    {
        public EventArgsException() : base() { }
        public EventArgsException(string Message) : base(Message) { }
    }
}
