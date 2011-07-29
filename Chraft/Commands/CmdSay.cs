using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Commands
{
    public class CmdSay : ClientCommand, ServerCommand
    {

        public ClientCommandHandler ClientCommandHandler { get; set; }
        public ServerCommandHandler ServerCommandHandler { get; set; }
        public void Use(Client client, string[] tokens)
        {
            string message = "";
            for (int i = 1; i < tokens.Length; i++)
            {
                message += tokens[i] + " ";
            }
            client.Server.Broadcast(message);
        }
        public void Help(Client client)
        {
            client.SendMessage("/say <Message> - broadcasts a message to the server.");
        }

        public string Name
        {
            get { return "say"; }
        }

        public string Shortcut
        {
            get { return ""; }
        }

        public CommandType Type
        {
            get { return CommandType.Mod; }
        }

        public void Use(Server server, string[] tokens)
        {
            string message = "";
            //for loop that starts at one so that we do not include "say".
            for (int i = 1; i < tokens.Length; i++)
            {
                message += tokens[i] + " ";
            }

            //Event
            ServerChatEventArgs e = new ServerChatEventArgs(server, message);
            server.PluginManager.CallEvent(Plugins.Events.Event.SERVER_CHAT, e);
            if (e.EventCanceled) return;
            message = e.Message;
            //End Event

            server.Broadcast(message);
        }

        public void Help(Server server)
        {
            server.Logger.Log(Logger.LogLevel.Info, "/say <message> - broadcasts a message to the server.");
        }
    }
}
