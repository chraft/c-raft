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
    class CmdIpBan : IClientCommand, IServerCommand
    {
        public string Name { get { return "ipban"; } }

        public string Shortcut { get { return "ipban"; } }
        public CommandType Type { get { return CommandType.Mod; } }
        public string Permission { get { return "chraft.ipban"; } }
        public IPlugin Iplugin { get; set; }
        public IClientCommandHandler ClientCommandHandler { get; set; }
        public void Use(IClient client, string commandName, string[] tokens)
        {
            if (tokens.Length < 2)
            {
                Help(client);
                return;
            }
            try
            {
                client.GetServer().GetBanSystem().AddToIpBans(tokens[0], tokens[1], tokens.Length > 2 ? tokens : null);
            }
            catch (FormatException ex)
            {
                client.SendMessage(ex.Message);
                return;
            }

            foreach (var nClient in client.GetServer().GetClients(tokens[0]).ToList())
            {
                nClient.Kick(tokens[1]);
            }

            client.SendMessage(string.Format("{0} has been banned", tokens[0]));
        }

        public void Help(IClient client)
        {
            client.SendMessage("/banip [ip] [reason] <duration>");
            client.SendMessage("e.g. /banip 127.0.0.1 hax d:30 h:5 m:5 s:5");
            client.SendMessage("banip 127.0.0.1 for 30 days, 5 hours, 5 minutes and 5 seconds");
        }

        public string AutoComplete(IClient client, string sourceStr)
        {
            return "";
        }


        public IServerCommandHandler ServerCommandHandler { get; set; }
        public void Use(IServer server, string commandName, string[] tokens)
        {
            if (tokens.Length < 2)
            {
                Help(server);
                return;
            }
            try
            {
                server.GetBanSystem().AddToBanList(tokens[0], tokens[1], tokens.Length > 2 ? tokens : null);
            }
            catch (FormatException ex)
            {
                server.GetLogger().Log(LogLevel.Info, ex.Message);
                return;
            }
            foreach (var nClient in server.GetClients(tokens[0]).ToList())
            {
                nClient.Kick(tokens[1]);
            }
            server.GetLogger().Log(LogLevel.Info, string.Format("{0} has been banned", tokens[0]));
        }

        public void Help(IServer server)
        {
            server.GetLogger().Log(LogLevel.Info, "banip [ip] [reason] <duration>");
            server.GetLogger().Log(LogLevel.Info, "e.g. /banip 127.0.0.1 hax d:30 h:5 m:5 s:5");
            server.GetLogger().Log(LogLevel.Info, "banip 127.0.0.1 for 30 days, 5 hours, 5 minutes and 5 seconds");
        }
    }
}
