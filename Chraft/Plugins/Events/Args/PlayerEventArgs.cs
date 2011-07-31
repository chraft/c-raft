using System;
using Chraft.Commands;
using Chraft.Entity;

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
            return Client.DisplayName;
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
        public virtual ClientCommand Command { get; private set; }
        public virtual string[] Tokens{get; set;}
        public ClientCommandEventArgs(Client Client, ClientCommand cmd, string[] tokens)
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
            BrodcastMessage = ChatColor.Yellow + c.DisplayName + " has joined the game."; //Like the Notchian server.
        }
    }
    public class ClientLeftEventArgs : ClientEventArgs
    {
        public virtual string BrodcastMessage { get; set; }
        public ClientLeftEventArgs(Client c)
            : base(c)
        {
            BrodcastMessage = ChatColor.Yellow + c.DisplayName + " has left the game"; //Like the Notchian server.
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
