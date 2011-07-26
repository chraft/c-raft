using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Commands
{
    public class CmdSetHealth : ClientCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }

        public void Use(Client client, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                client.SetHealth(20);
                return;
           }
            client.SetHealth(short.Parse(tokens[1]));
        }

        public void Help(Client client)
        {
            client.SendMessage("/sethealth <Health> - Sets your health to <Health>");
        }

        public string Name
        {
            get { return "sethealth"; }
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
