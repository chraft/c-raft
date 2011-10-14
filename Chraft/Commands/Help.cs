using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net;
using Chraft.Utils;

namespace Chraft.Commands
{
    public class CmdHelp : ClientCommand, ServerCommand
    {
        public ClientCommandHandler ClientCommandHandler { get; set; }
        public ServerCommandHandler ServerCommandHandler { get; set; }
        public string Name { get { return "help"; } }
        public string Shortcut { get { return ""; } }
        public CommandType Type { get { return CommandType.Information; } }
        public string Permission{get { return "chraft.help"; }}

        public void Use(Client client, string[] tokens)
        {
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
                        message = (from ClientCommand c in ClientCommandHandler.GetCommands() where c.Type == CommandType.Build && client.Owner.CanUseCommand(c) select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    case "mod":
                        message = (from ClientCommand c in ClientCommandHandler.GetCommands() where c.Type == CommandType.Mod && client.Owner.CanUseCommand(c) select c).Aggregate("", (current, c) => current + (", " + c.Name));
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
                        message = (from ClientCommand c in ClientCommandHandler.GetCommands() where c.Type == CommandType.Information && client.Owner.CanUseCommand(c) select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    case "other":
                        message = (from ClientCommand c in ClientCommandHandler.GetCommands() where c.Type == CommandType.Other && client.Owner.CanUseCommand(c) select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    case "short":
                        message = (from ClientCommand c in ClientCommandHandler.GetCommands() where client.Owner.CanUseCommand(c) && !string.IsNullOrEmpty(c.Shortcut) select c).Aggregate("", (current, c) => current + (", " + c.Shortcut));
                        if (message == "")
                        {
                            client.SendMessage(ChatColor.Red + "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        client.SendMessage(message);
                        break;
                    default:
                        ClientCommand cmd;
                        try
                        {
                            cmd = ClientCommandHandler.Find(tokens[0]) as ClientCommand;
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
                        catch(Exception e)
                        {
                            client.SendMessage("There was an error while accessing the help for this command.");
                            client.Owner.Server.Logger.Log(e);
                        }
                        break;
                }
            }
        }

        public void Help(Client client)
        {
            client.SendMessage("helps");
        }

        public void Use(Server server, string[] tokens)
        {
            if (tokens.Length == 0)
            {

                server.Logger.Log(Logger.LogLevel.Info, "Use /help build for a list of building commands.");
                server.Logger.Log(Logger.LogLevel.Info, "Use /help mod for a list of moderation commands.");
                server.Logger.Log(Logger.LogLevel.Info, "Use /help information for a list of information commands.");
                server.Logger.Log(Logger.LogLevel.Info, "Use /help other for a list of other commands.");
                server.Logger.Log(Logger.LogLevel.Info, "Use /help short for a list of shortcuts.");
            }
            else if (tokens.Length > 0)
            {
                string message;
                switch (tokens[0].ToLower())
                {
                    case "build":
                        message = (from ServerCommand c in ServerCommandHandler.GetCommands() where c.Type == CommandType.Build select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            server.Logger.Log(Logger.LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(Logger.LogLevel.Info, message);
                        break;
                    case "mod":
                        message = (from ServerCommand c in ServerCommandHandler.GetCommands() where c.Type == CommandType.Build select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            server.Logger.Log(Logger.LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(Logger.LogLevel.Info, message);
                        break;
                    case "information":
                    case "info":
                        message = (from ServerCommand c in ServerCommandHandler.GetCommands() where c.Type == CommandType.Build select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            server.Logger.Log(Logger.LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(Logger.LogLevel.Info, message);
                        break;
                    case "other":
                        message = (from ServerCommand c in ServerCommandHandler.GetCommands() where c.Type == CommandType.Build select c).Aggregate("", (current, c) => current + (", " + c.Name));
                        if (message == "")
                        {
                            server.Logger.Log(Logger.LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(Logger.LogLevel.Info, message);
                        break;
                    case "short":
                        message = (from ServerCommand c in ServerCommandHandler.GetCommands() where !string.IsNullOrEmpty(c.Shortcut) select c).Aggregate("", (current, c) => current + (", " + c.Shortcut));
                        if (message == "")
                        {
                            server.Logger.Log(Logger.LogLevel.Info, "There are no commands of this type that you can use.");
                            return;
                        }
                        message = message.Remove(0, 1);
                        server.Logger.Log(Logger.LogLevel.Info, message);
                        break;
                    default:
                        ServerCommand cmd;
                        try
                        {
                            cmd = ServerCommandHandler.Find(tokens[1]) as ServerCommand;
                        }
                        catch (CommandNotFoundException e) { server.Logger.Log(Logger.LogLevel.Info, e.Message); return; }
                        try
                        {
                            cmd.Help(server);
                            server.Logger.Log(Logger.LogLevel.Info, "Type: " + cmd.Type.ToString());
                        }
                        catch (Exception e)
                        {
                            server.Logger.Log(Logger.LogLevel.Info, "There was an error while accessing the help for this command.");
                            server.Logger.Log(e);
                        }
                        break;
                }
            }
        }

        public void Help(Server server)
        {
            server.Logger.Log(Logger.LogLevel.Info, "helps");
        }
    }
}
