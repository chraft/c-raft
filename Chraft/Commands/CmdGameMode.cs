using System;
using System.Linq;
using Chraft.Net;
using Chraft.Net.Packets;

namespace Chraft.Commands
{
    public class CmdGameMode : ClientCommand
    {

        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            switch (tokens.Length)
            {
                case 0:
                    ChangeGameMode(client, client.Owner.GameMode == 0 ? 1 : 0);
                    break;
                case 2:
                    if (Int32.Parse(tokens[1]) != 0 || Int32.Parse(tokens[1]) != 1)
                    {
                        Help(client);
                        break;
                    }
                    Client c = client.Owner.Server.GetClients(tokens[0]).FirstOrDefault();
                    if (c != null)
                    {
                        if (c.Owner.GameMode == Convert.ToByte(tokens[1]))
                        {
                            client.SendMessage("§Player is already in that mode");
                            return;
                        }
                        ChangeGameMode(client, Int32.Parse(tokens[1]));
                    }
                    client.SendMessage(string.Format("§cPlayer {0} not found", tokens[0]));
                    break;
                default:
                    Help(client);
                    break;
            }
        }

        private static void ChangeGameMode(Client client, int mode)
        {
            client.SendPacket(new NewInvalidStatePacket
            {
                GameMode = client.Owner.GameMode = Convert.ToByte(mode),
                Reason = NewInvalidStatePacket.NewInvalidReason.ChangeGameMode
            });
        }

        public void Help(Client client)
        {
            client.SendMessage("/gamemode <player> [0|1]");
        }

        public string Name
        {
            get { return "gamemode"; }
        }

        public string Shortcut
        {
            get { return "gm"; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public string Permission
        {
            get { return "chraft.gamemode"; }
        }
    }
}
