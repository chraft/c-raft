using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities.Misc;

namespace Chraft.Commands
{
    class CmdUnban : IClientCommand, IServerCommand
    {
        public string Name { get { return "unban"; } }

        public string Shortcut { get { return "unban"; } }
        public CommandType Type { get { return CommandType.Mod; } }
        public string Permission { get { return "chraft.unban"; } }
        public IPlugin Iplugin { get; set; }
        public IClientCommandHandler ClientCommandHandler { get; set; }
        public void Use(IClient client, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                Help(client);
                return;
            }
            client.GetServer().GetBanSystem().RemoveFromBanList(tokens[0]);
            client.SendMessage(string.Format("{0} has been unbanned", tokens[0]));
        }

        public void Help(IClient client)
        {
            client.SendMessage("/unban [player]");
        }

        public string AutoComplete(IClient client, string sourceStr)
        {
            return "";
        }


        public IServerCommandHandler ServerCommandHandler { get; set; }
        public void Use(IServer server, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                Help(server);
                return;
            }
            server.GetBanSystem().RemoveFromBanList(tokens[0]);
            server.GetLogger().Log(LogLevel.Info, string.Format("{0} has been unbanned", tokens[0]));
        }

        public void Help(IServer server)
        {
            server.GetLogger().Log(LogLevel.Info, "unban [player]");
        }
    }
}
