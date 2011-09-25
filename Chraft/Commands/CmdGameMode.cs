using System;
using System.Linq;
using Chraft.Net.Packets;

namespace Chraft.Commands
{
    public class CmdGameMode : ClientCommand
    {

        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            if (tokens.Length < 2)
            {
                client.SendMessage("§cUsage <player> <mode>");
                return;
            }
            Client c = client.Server.GetClients(tokens[1]).FirstOrDefault();
            if (c != null)
            {
                if (c.GameMode == Convert.ToByte(tokens[2]))
                {
                    client.SendMessage("§7You are already in that mode");
                    return;
                }
                c.SendPacket(new NewInvalidStatePacket
                                      {
                                          GameMode = c.GameMode = Convert.ToByte(tokens[2]),
                                          Reason = NewInvalidStatePacket.NewInvalidReason.ChangeGameMode
                                      });
            }
            else
            {
                client.SendMessage(string.Format("§cPlayer {0} not found", tokens[1]));
            }
        }

        public void Help(Client client)
        {
            client.SendMessage("/gamemode <player> <mode>");
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
