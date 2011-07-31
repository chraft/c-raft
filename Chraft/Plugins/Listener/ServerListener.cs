using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Listener
{
    public class ServerListener : ChraftListener
    {
        public virtual void OnBroadcast(ServerBroadcastEventArgs e) { }
        public virtual void OnLog(LoggerEventArgs e) { }
        public virtual void OnAccept(ClientAcceptedEventArgs e) { }
        public virtual void OnCommand(ServerCommandEventArgs e) { }
        public virtual void OnChat(ServerChatEventArgs e) { }
    }
}
