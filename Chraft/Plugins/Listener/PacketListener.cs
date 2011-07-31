using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Listener
{
    class PacketListener : ChraftListener
    {
        public virtual void OnPacketReceived(PacketRecevedEventArgs e) { }
        public virtual void OnPacketSent(PacketSentEventArgs e) { }
    }
}
