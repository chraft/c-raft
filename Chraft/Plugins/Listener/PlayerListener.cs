using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Listener
{
    public class PlayerListener : ChraftListener
    {
        public virtual void OnPlayerJoined(ClientJoinedEventArgs e) { }
        public virtual void OnPlayerLeft(ClientLeftEventArgs e) { }
        public virtual void OnPlayerCommand(ClientCommandEventArgs e) { }
        public virtual void OnPlayerPreCommand(ClientCommandEventArgs e) { }
        public virtual void OnPlayerChat(ClientChatEventArgs e) { }
        public virtual void OnPlayerPreChat(ClientPreChatEventArgs e) { }
        public virtual void OnPlayerKicked(ClientKickedEventArgs e) { }
        public virtual void OnPlayerMoved(ClientMoveEventArgs e) { }
        public virtual void OnPlayerDeath(ClientDeathEventArgs e) { }
    }
}
