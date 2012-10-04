using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities.Misc;

namespace Chraft.Commands
{
    class CmdKick : IClientCommand, IServerCommand
    {
        public string Name { get { return "kick"; } }

        public string Shortcut { get { return "kick"; } }

        public CommandType Type { get { return CommandType.Mod; } }
        public string Permission { get { return "chraft.kick"; } }
        public IPlugin Iplugin { get; set; }
        public IClientCommandHandler ClientCommandHandler { get; set; }
        public void Use(IClient client, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                Help(client);
                return;
            }

            var toKick = client.GetServer().GetClients();


            if (toKick.Any() && tokens[0].ToLower() != "all")
            {
                foreach (var client1 in toKick.Where(client1 => !client1.GetOwner().CanUseCommand("chraft.kick.exempt")))
                {
                    client1.Kick(tokens.Length > 1 ? tokens[1] : "Kicked");
                    client.SendMessage("Kicked " + client1.GetOwner().Name);
                }
            }
            else
            {
                foreach (IClient t in toKick.Where(t => t.GetOwner().Name.ToLower() == tokens[0].ToLower()).Where(t => !t.GetOwner().CanUseCommand("chraft.kick.exempt")))
                {
                    t.Kick(tokens.Length > 1 ? tokens[1] : "Kicked");
                    client.SendMessage("Kicked " + t.GetOwner().Name);
                }
            }
        }

        public void Help(IClient client)
        {
            client.SendMessage("/kick [player|all] <reason>");
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

            var toKick = server.GetClients();


            if (toKick.Any() && tokens[0].ToLower() != "all")
            {
                foreach (var client1 in toKick.Where(client1 => !client1.GetOwner().CanUseCommand("chraft.kick.exempt")))
                {
                    client1.Kick(tokens.Length > 1 ? tokens[1] : "Kicked");
                    server.GetLogger().Log(LogLevel.Info, "Kicked " + client1.GetOwner().Name);
                }
            }
            else
            {
                foreach (IClient t in toKick.Where(t => t.GetOwner().Name.ToLower() == tokens[0].ToLower()).Where(t => !t.GetOwner().CanUseCommand("chraft.kick.exempt")))
                {
                    t.Kick(tokens.Length > 1 ? tokens[1] : "Kicked");
                    server.GetLogger().Log(LogLevel.Info, "Kicked " + t.GetOwner().Name);
                }
            }
        }

        public void Help(IServer server)
        {
            server.GetLogger().Log(LogLevel.Info, "/kick [player|all] <reason>");
        }
    }
}
