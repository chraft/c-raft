#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Listener
{
    public class PlayerListener : IChraftListener
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
