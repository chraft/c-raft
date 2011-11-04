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
using Chraft.Commands;
using Chraft.Entity;
using Chraft.Net;
using Chraft.Utils;

namespace Chraft.Plugins.Events.Args
{
    public class ClientEventArgs : ChraftEventArgs
    {
        public virtual Client Client { get; protected set; }

        public ClientEventArgs(Client Client)
            : base()
        {
            this.Client = Client;
            EventCanceled = false;
        }

        public override string ToString()
        {
            return Client.Owner.DisplayName;
        }
    }
    public class ClientPreCommandEventArgs : ClientEventArgs
    {
        public virtual string Command { get; set; }
        public ClientPreCommandEventArgs(Client c, string Command)
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
        public ClientCommandEventArgs(Client Client, IClientCommand cmd, string[] tokens)
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
        public ClientMessageEventArgs(Client p, string Message)
            : base(p)
        {
            this.Message = Message;
        }
    }
    public class ClientDeathEventArgs : ClientEventArgs
    {
        public virtual EntityBase KilledBy { get; set; }

        public ClientDeathEventArgs(Client c, string Message, EntityBase KilledBy)
            : base(c)
        {
            this.KilledBy = KilledBy;
        }
    }

    public class ClientJoinedEventArgs : ClientEventArgs
    {
        public virtual string BrodcastMessage { get; set; }
        public ClientJoinedEventArgs(Client c) : base(c) 
        {
            BrodcastMessage = ChatColor.Yellow + c.Owner.DisplayName + " has joined the game."; //Like the Notchian server.
        }
    }
    public class ClientLeftEventArgs : ClientEventArgs
    {
        public virtual string BrodcastMessage { get; set; }
        public ClientLeftEventArgs(Client c)
            : base(c)
        {
            BrodcastMessage = ChatColor.Yellow + c.Owner.DisplayName + " has left the game"; //Like the Notchian server.
        }
    }
    public class ClientChatEventArgs : ClientMessageEventArgs
    {
        public ClientChatEventArgs(Client c, string Message) : base(c, Message) { }
    }
    public class ClientPreChatEventArgs : ClientMessageEventArgs
    {
        public ClientPreChatEventArgs(Client c, string Message) : base(c, Message) { }
    }
    public class ClientKickedEventArgs : ClientMessageEventArgs
    {
        public ClientKickedEventArgs(Client c, string Message) : base(c, Message) { }
    }
    public class ClientMoveEventArgs : ClientEventArgs
    {
        public ClientMoveEventArgs(Client c) : base(c) { }
    }
}
