using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Plugins.Events.Args
{
    public abstract class ChraftEventArgs : EventArgs
    {
        public virtual bool EventCanceled { get; set; }
        public ChraftEventArgs()
        {
            EventCanceled = false;
        }
    }
}
