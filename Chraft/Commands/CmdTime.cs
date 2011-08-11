using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;

namespace Chraft.Commands
{
    public class CmdTime : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            int newTime = -1;
            if (tokens.Length < 2)
            {
                client.SendMessage("You must specify an explicit time, day, or night.");
                return;
            }
            if (int.TryParse(tokens[1], out newTime) && newTime >= 0 && newTime <= 24000)
            {
                client.World.Time = newTime;
            }
            else if (tokens[1].ToLower() == "day")
            {
                client.World.Time = 0;
            }
            else if (tokens[1].ToLower() == "night")
            {
                client.World.Time = 12000;
            }
            else
            {
                client.SendMessage("You must specify a time value between 0 and 24000");
                return;
            }
            client.Server.Broadcast(new TimeUpdatePacket { Time = client.World.Time });
        }

        public void Help(Client client)
        {
            client.SendMessage("/time <Day | Night | Raw> - Sets the time.");
        }

        public string Name
        {
            get { return "time"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.time"; }
        }
    }
}
