using Chraft.Commands;
using Chraft.Net;
using Chraft.Net.Packets;

namespace Chraft.Plugins.Commands
{
    public class CmdTime : IClientCommand
    {
        public CmdTime(IPlugin plugin)
        {
            Iplugin = plugin;
        }
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            int newTime = -1;
            if (tokens.Length < 1)
            {
                client.SendMessage("You must specify a time value between 0 and 24000 or <sunrise|day|sunset|night>");
                return;
            }
            if (int.TryParse(tokens[0], out newTime) && newTime >= 0 && newTime <= 24000)
            {
                client.Owner.World.Time = newTime;
            }
            else if (tokens[0].ToLower() == "sunrise")
            {
                client.Owner.World.Time = 0;
            }
            else if (tokens[0].ToLower() == "day")
            {
                client.Owner.World.Time = 6000;
            }
            else if (tokens[0].ToLower() == "sunset")
            {
                client.Owner.World.Time = 12000;
            }

            else if (tokens[0].ToLower() == "night")
            {
                client.Owner.World.Time = 18000;
            }
            else
            {
                client.SendMessage("You must specify a time value between 0 and 24000 or <sunrise|day|sunset|night>");
                return;
            }
            client.Owner.Server.Broadcast(new TimeUpdatePacket { Time = client.Owner.World.Time });
        }

        public void Help(Client client)
        {
            client.SendMessage("/time <Sunrise | Day | Sunset | Night | Raw> - Sets the time.");
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

        public IPlugin Iplugin { get; set; }
    }
}
