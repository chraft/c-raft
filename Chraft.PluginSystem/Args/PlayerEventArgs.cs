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

using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Net;
using Chraft.Utilities.Misc;

namespace Chraft.PluginSystem.Args
{
    public class ClientEventArgs : ChraftEventArgs
    {
        public virtual IClient Client { get; protected set; }

        public ClientEventArgs(IClient Client)
            : base()
        {
            this.Client = Client;
            EventCanceled = false;
        }

        public override string ToString()
        {
            return Client.GetOwner().DisplayName;
        }
    }
    public class ClientPreCommandEventArgs : ClientEventArgs
    {
        public virtual string Command { get; set; }
        public ClientPreCommandEventArgs(IClient c, string Command)
            : base(c)
        {
            this.Command = Command;
        }
        public override string ToString()
        {
            return Command;
        }
    }
    public class ClientCommandEventArgs : ClientEventArgs
    {
        public virtual IClientCommand Command { get; private set; }
        public virtual string[] Tokens{get; set;}
        public ClientCommandEventArgs(IClient Client, IClientCommand cmd, string[] tokens)
            : base(Client)
        {
            this.Command = cmd;
            this.Tokens = tokens;
        }
        public override string ToString()
        {
            return Command.Name;
        }
    }
    public class ClientMessageEventArgs : ClientEventArgs
    {
        public virtual string Message { get; set; }
        public ClientMessageEventArgs(IClient p, string Message)
            : base(p)
        {
            this.Message = Message;
        }
    }
    public class ClientDeathEventArgs : ClientEventArgs
    {
        public virtual IEntityBase KilledBy { get; set; }

        public ClientDeathEventArgs(IClient c, string Message, IEntityBase KilledBy)
            : base(c)
        {
            this.KilledBy = KilledBy;
        }
    }

    public class ClientJoinedEventArgs : ClientEventArgs
    {
        public virtual string BrodcastMessage { get; set; }
        public ClientJoinedEventArgs(IClient c) : base(c) 
        {
            BrodcastMessage = ChatColor.Yellow + c.GetOwner().DisplayName + " has joined the game."; //Like the Notchian server.
        }
    }
    public class ClientLeftEventArgs : ClientEventArgs
    {
        public virtual string BrodcastMessage { get; set; }
        public ClientLeftEventArgs(IClient c)
            : base(c)
        {
            BrodcastMessage = ChatColor.Yellow + c.GetOwner().DisplayName + " has left the game"; //Like the Notchian server.
        }
    }
    public class ClientChatEventArgs : ClientMessageEventArgs
    {
        public ClientChatEventArgs(IClient c, string Message) : base(c, Message) { }
    }
    public class ClientPreChatEventArgs : ClientMessageEventArgs
    {
        public ClientPreChatEventArgs(IClient c, string Message) : base(c, Message) { }
    }
    public class ClientKickedEventArgs : ClientMessageEventArgs
    {
        public ClientKickedEventArgs(IClient c, string Message) : base(c, Message) { }
    }
    public class ClientMoveEventArgs : ClientEventArgs
    {
        public ClientMoveEventArgs(IClient c) : base(c) { }
    }
}
