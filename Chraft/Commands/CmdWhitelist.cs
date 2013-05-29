using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities.Config;
using Chraft.Utilities.Misc;

namespace Chraft.Commands
{
    class CmdWhitelist : IClientCommand, IServerCommand
    {
        public string Name { get { return "whitelist"; } }
        public string Shortcut { get { return "whitelist"; } }
        public CommandType Type { get { return CommandType.Mod; } }
        public string Permission { get { return "chraft.whitelist"; } }
        public IPlugin Iplugin { get; set; }
        public IClientCommandHandler ClientCommandHandler { get; set; }
        public void Use(IClient client, string commandName, string[] tokens)
        {
            if (tokens.Length < 1)
            {
                Help(client);
                return;
            }

            switch (tokens[0].ToLower())
            {
                
                case "on":
                    ChraftConfig.SetWhitelist(true);
                    foreach (var cl in client.GetServer().GetClients())
                    {
                        if( !cl.GetOwner().CanUseCommand("chraft.whitelist.exempt") &&
                            client.GetServer().GetBanSystem().IsOnWhiteList(cl.GetOwner().Name))
                        {
                            cl.Kick(ChraftConfig.WhiteListMesasge);
                        }
                    }
                    client.SendMessage("Whitelist enabled");
                    break;
                case "off":
                    ChraftConfig.SetWhitelist(false);
                    client.SendMessage("Whitelist disabled");
                    break;
                case "add":
                    if (tokens.Length < 2)
                    {
                        Help(client);
                        return;
                    }
                    client.GetServer().GetBanSystem().AddToWhiteList(tokens[1]);
                    client.SendMessage(tokens[1] + " added to Whitelist");
                    break;
                case "remove":
                    if (tokens.Length < 2)
                    {
                        Help(client);
                        return;
                    }
                    client.GetServer().GetBanSystem().RemoveFromWhiteList(tokens[1]);
                    client.SendMessage(tokens[1] + " removed from Whitelist");
                    break;
                case "list":
                    foreach (var play in client.GetServer().GetBanSystem().ListWhiteList())
                    {
                        client.SendMessage(play);
                    }
                    break;
                case "message":
                    if (tokens.Length < 2)
                    {
                        Help(client);
                        return;
                    }
                    ChraftConfig.SetWhitelistMessage(tokens[1]);
                    client.SendMessage("Whitelist message set");
                    break;

            }
        }

        public void Help(IClient client)
        {
            client.SendMessage("/whitelist [on|off|add|remove|list|message] <player>");
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

            switch (tokens[0].ToLower())
            {
                case "on":
                    ChraftConfig.SetWhitelist(true);
                    foreach (var cl in server.GetClients())
                    {
                        if (!cl.GetOwner().CanUseCommand("chraft.whitelist.exempt") &&
                            server.GetBanSystem().IsOnWhiteList(cl.GetOwner().Name))
                        {
                            cl.Kick(ChraftConfig.WhiteListMesasge);
                        }
                    }
                    server.GetLogger().Log(LogLevel.Info, "Whitelist enabled");
                    break;
                case "off":
                    ChraftConfig.SetWhitelist(false);
                    server.GetLogger().Log(LogLevel.Info, "Whitelist disabled");
                    break;
                case "add":
                    if (tokens.Length < 2)
                    {
                        Help(server);
                        return;
                    }
                    server.GetBanSystem().AddToWhiteList(tokens[1]);
                    server.GetLogger().Log(LogLevel.Info, tokens[1] + " added to Whitelist");
                    break;
                case "remove":
                    if (tokens.Length < 2)
                    {
                        Help(server);
                        return;
                    }
                    server.GetBanSystem().RemoveFromWhiteList(tokens[1]);
                    server.GetLogger().Log(LogLevel.Info, tokens[1] + " removed from Whitelist");
                    break;
                case "list":
                    foreach (var play in server.GetBanSystem().ListWhiteList())
                    {
                        server.GetLogger().Log(LogLevel.Info, play);
                    }
                    break;
                case "message":
                    if (tokens.Length < 2)
                    {
                        Help(server);
                        return;
                    }
                    ChraftConfig.SetWhitelistMessage(tokens[1]);
                    server.GetLogger().Log(LogLevel.Info, "Whitelist message set");
                    break;
            }
        }

        public void Help(IServer server)
        {
            server.GetLogger().Log(LogLevel.Info, "whitelist [on|off|add|remove|message] <player>");
        }
    }
}
