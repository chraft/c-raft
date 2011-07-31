using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using Chraft.Net.Packets;

namespace Chraft.Plugins.Events.Args
{
    public class PacketEventArgs : ChraftEventArgs
    {
        public virtual Packet Packet { get; set; }
        public virtual Socket Socket { get; set; }
        public virtual Client Client { get; set; }

        public PacketEventArgs(Packet Packet, Socket Socket, Client Client)
        {
            this.Socket = Socket;
            this.Client = Client;
            this.Packet = Packet;
        }
    }
    public class PacketSentEventArgs : PacketEventArgs
    {
        public PacketSentEventArgs(Packet Packet, Socket Socket, Client Client) : base(Packet, Socket, Client) { }
    }
    public class PacketRecevedEventArgs : PacketEventArgs
    {
        public PacketRecevedEventArgs(Packet Packet, Socket Socket, Client Client) : base(Packet, Socket, Client) { }
    }
}
