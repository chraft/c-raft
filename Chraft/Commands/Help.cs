#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Plugins;
using Chraft.Utilities;
using Chraft.Utilities.Misc;
using Chraft.Utils;

namespace Chraft.Commands
{
    public class CmdHelp : IClientCommand, IServerCommand
    {
        public IClientCommandHandler ClientCommandHandler { get; set; }
        public IServerCommandHandler ServerCommandHandler { get; set; }
        public string Name { get { return "help"; } }
        public string Shortcut { get { return ""; } }
        public CommandType Type { get { return CommandType.Information; } }
        public string Permission { get { return "chraft.help"; } }

        public IPlugin Iplugin { get; set; }

        public void Use(IClient iClient, string commandName, string[] tokens)
        {
            Client client = iClient as Client;
            if (tokens.Length == 0)
            {
                client.SendMessage("Use " + ChatColor.Teal + "/help build" + ChatColor.White + " for a list of building commands.");
                client.SendMessage("Use " + ChatColor.Teal + "/help mod" + ChatColor.White + " for a list of moderation commands.");
                client.SendMessage("Use " + ChatColor.Teal + "/help information" + ChatColor.White + " for a list of information commands.");
                client.SendMessage("Use " + ChatColor.Teal + "/help other" + ChatColor.White + " for a list of other commands.");
                client.SendMessage("Use " + ChatColor.Teal + "/help short" + ChatColor.White + " for a list of shortcuts.");
            }
            else if (tokens.Length > 0)
            {
                string message;
                switch (tokens[0].ToLower())
                {
                    case "build":
                        message = (from IClientCommand c in ClientCommandHandler.GetCommands() where c.Type == CommandType.Build && client.Owner.CanUseCommand(c) select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    case "mod":
                        message = (from IClientCommand c in ClientCommandHandler.GetCommands() where c.Type == CommandType.Mod && client.Owner.CanUseCommand(c) select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    case "information":
                    case "info":
                        message = (from IClientCommand c in ClientCommandHandler.GetCommands() where c.Type == CommandType.Information && client.Owner.CanUseCommand(c) select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    case "other":
                        message = (from IClientCommand c in ClientCommandHandler.GetCommands() where c.Type == CommandType.Other && client.Owner.CanUseCommand(c) select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    case "short":
                        message = (from IClientCommand c in ClientCommandHandler.GetCommands() where client.Owner.CanUseCommand(c) && !string.IsNullOrEmpty(c.Shortcut) select c).Aggregate("", (current, c) => current + (", " + c.Shortcut));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    default:
                        IClientCommand cmd;
                        try
                        {
                            cmd = ClientCommandHandler.Find(tokens[0]) as IClientCommand;
                        }
                        catch (MultipleCommandsMatchException e)
                        {
                            client.SendMessage(ChatColor.Red + "Multiple commands has been found:");
                            foreach (var s in e.Commands)
                                client.SendMessage(string.Format(" {0}{1}", ChatColor.Red, s));
                            
                            return;
                        }
                        catch (CommandNotFoundException e) { client.SendMessage(e.Message); return; }
                        try
                        {
                            cmd.Help(client);
                            client.SendMessage("Type: " + cmd.Type.ToString());
                            if (client.Owner.CanUseCommand(cmd))
                            {
                                client.SendMessage(ChatColor.BrightGreen + "You can use this command.");
                            }
                            else
                            {
                                client.SendMessage(ChatColor.Red + "You cannot use this command.");
                            }
                        }
                        catch (Exception e)
                        {
                            client.SendMessage("There was an error while accessing the help for this command.");
                            client.Owner.Server.Logger.Log(e);
                        }
                        break;
                }
            }
        }

        public void Help(IClient client)
        {
            client.SendMessage("helps");
        }

        public void Use(IServer iServer, string commandName, string[] tokens)
        {
            Server server = iServer as Server;
            if (tokens.Length == 0)
            {

                server.Logger.Log(LogLevel.Info, "Use /help build for a list of building commands.");
                server.Logger.Log(LogLevel.Info, "Use /help mod for a list of moderation commands.");
                server.Logger.Log(LogLevel.Info, "Use /help information for a list of information commands.");
                server.Logger.Log(LogLevel.Info, "Use /help other for a list of other commands.");
                server.Logger.Log(LogLevel.Info, "Use /help short for a list of shortcuts.");
            }
            else if (tokens.Length > 0)
            {
                string message;
                switch (tokens[0].ToLower())
                {
                    case "build":
                        message = (from IServerCommand c in ServerCommandHandler.GetCommands() where c.Type == CommandType.Build select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            server.Logger.Log(LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(LogLevel.Info, message);
                        break;
                    case "mod":
                        message = (from IServerCommand c in ServerCommandHandler.GetCommands() where c.Type == CommandType.Build select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            server.Logger.Log(LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(LogLevel.Info, message);
                        break;
                    case "information":
                    case "info":
                        message = (from IServerCommand c in ServerCommandHandler.GetCommands() where c.Type == CommandType.Build select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            server.Logger.Log(LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(LogLevel.Info, message);
                        break;
                    case "other":
                        message = (from IServerCommand c in ServerCommandHandler.GetCommands() where c.Type == CommandType.Build select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            server.Logger.Log(LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(LogLevel.Info, message);
                        break;
                    case "short":
                        message = (from IServerCommand c in ServerCommandHandler.GetCommands() where !string.IsNullOrEmpty(c.Shortcut) select c).Aggregate("", (current, c) => current + (", " + c.Shortcut));
                        if (message == "")
                        {
                            server.Logger.Log(LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(LogLevel.Info, message);
                        break;
                    default:
                        IServerCommand cmd;
                        try
                        {
                            cmd = ServerCommandHandler.Find(tokens[0]) as IServerCommand;
                        }
                        catch (CommandNotFoundException e) { server.Logger.Log(LogLevel.Info, e.Message); return; }
                        try
                        {
                            cmd.Help(server);
                            server.Logger.Log(LogLevel.Info, "Type: " + cmd.Type.ToString());
                        }
                        catch (Exception e)
                        {
                            server.Logger.Log(LogLevel.Info, "There was an error while accessing the help for this command.");
                            server.Logger.Log(e);
                        }
                        break;
                }
            }
        }

        public string AutoComplete(IClient client, string s)
        {
            var args = new string[] { "build", "mode", "information", "other", "short" };
            if (string.IsNullOrEmpty(s.Trim()))
                return string.Join("\0", args);

            if (s.TrimStart().IndexOf(' ') != -1)
                return string.Empty;

            var sb = new StringBuilder();

            foreach (var a in args)
                if (a.StartsWith(s.Trim(), StringComparison.OrdinalIgnoreCase))
                    sb.Append(a).Append('\0');
            return sb.ToString();
        }

        public void Help(IServer server)
        {
            server.GetLogger().Log(LogLevel.Info, "helps");
        }
    }
}
