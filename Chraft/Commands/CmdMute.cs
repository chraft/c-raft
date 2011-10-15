using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;

namespace Chraft.Commands
{
    public class CmdMute : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                client.SendMessage("You must specify a player to mute");
                return;
            }

            Client[] matchedClients = client.Owner.Server.GetClients(tokens[0]).ToArray();
            Client clientToMute = null;
            if (matchedClients.Length < 1)
            {
                client.SendMessage("Unknown Player");
                return;
            }
            else if (matchedClients.Length == 1)
            {
                clientToMute = matchedClients[0];
            }
            else if (matchedClients.Length > 1)
            {
                // We've got more than 1 client. I.e. "Test" and "Test123" for the "test" pattern.
                // Looking for exact name match.
                int exactMatchClient = -1;
                for (int i = 0; i < matchedClients.Length; i++)
                {
                    if (matchedClients[i].Owner.DisplayName.ToLower() == tokens[0].ToLower())
                        exactMatchClient = i;
                }

                // If we found the player with the exactly same name - he is our target
                if (exactMatchClient != -1)
                {
                    clientToMute = matchedClients[exactMatchClient];
                } else
                {
                    // We do not found a proper target and aren't going to randomly punish anyone
                    client.SendMessage("More than one player found. Provide the exact name.");
                    return;
                }
            }
            bool clientMuted = clientToMute.Owner.IsMuted;
            clientToMute.Owner.IsMuted = !clientMuted;
            clientToMute.SendMessage(clientMuted ? "You have been unmuted" : "You have been muted");
            client.SendMessage(clientMuted ? clientToMute.Owner.DisplayName + " has been unmuted" : clientToMute.Owner.DisplayName + " has been muted");
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

        public string Permission
        {
            get { return "chraft.mute"; }
        }
    }
}
