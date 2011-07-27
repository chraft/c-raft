using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public class CmdMute : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            if (tokens.Length < 2)
            {
                client.SendMessage("You must specify a player to mute");
                return;
            }

            Client[] muteClient = client.Server.GetClients(tokens[1]).ToArray();
            if (muteClient.Length < 1)
            {
                client.SendMessage("Unknown Player");
                return;
            }
            bool clientMuted = muteClient[0].IsMuted;
            muteClient[0].IsMuted = !clientMuted;
            muteClient[0].SendMessage(clientMuted ? "You have been unmuted" : "You have been muted");
            client.SendMessage(clientMuted ? tokens[1] + " has been unmuted" : tokens[1] + " has been muted");
        }

        public void Help(Client client)
        {
            client.SendMessage("/mute <Target> - Mutes or unmutes <Target>.");
        }

        public string Name
        {
            get { return "mute"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }
    }
}
