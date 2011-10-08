using System;
using Chraft.Entity;
using Chraft.Plugins.Events;
using Chraft.World;
using Chraft.Utils;
using Chraft.Commands;
using Chraft.Plugins.Events.Args;

namespace Chraft.Net
{
    public partial class Client : IDisposable
    {
        /// <summary>
        /// Invoked whenever the user sends a command.
        /// </summary>
        public event EventHandler<CommandEventArgs> Command;

        /// <summary>
        /// Invoked prior to a chat message transmission.
        /// </summary>
        public event EventHandler<ChatEventArgs> ChatMessage;

        /// <summary>
        /// Gets or sets the first point of the cuboid selection.
        /// </summary>
        public UniversalCoords? Point1 { get; set; }

        /// <summary>
        /// Gets or sets the second point of the cuboid selection.
        /// </summary>
        public UniversalCoords? Point2 { get; set; }

        /// <summary>
        /// Gets or sets the start of the cuboid selection.
        /// </summary>
        public UniversalCoords? SelectionStart
        {
            get
            {
                if (Point1 == null || Point2 == null)
                    return null;
                return UniversalCoords.FromWorld(Point1.Value.WorldX < Point2.Value.WorldX ? Point1.Value.WorldX : Point2.Value.WorldX,
                    Point1.Value.WorldY < Point2.Value.WorldY ? Point1.Value.WorldY : Point2.Value.WorldY,
                    Point1.Value.WorldZ < Point2.Value.WorldZ ? Point1.Value.WorldZ : Point2.Value.WorldZ);
            }
        }

        /// <summary>
        /// Gets or sets the end of the cuboid selection.
        /// </summary>
        public UniversalCoords? SelectionEnd
        {
            get
            {
                if (Point1 == null || Point2 == null)
                    return null;
                return UniversalCoords.FromWorld(Point1.Value.WorldX > Point2.Value.WorldX ? Point1.Value.WorldX : Point2.Value.WorldX,
                    Point1.Value.WorldY > Point2.Value.WorldY ? Point1.Value.WorldY : Point2.Value.WorldY,
                    Point1.Value.WorldZ > Point2.Value.WorldZ ? Point1.Value.WorldZ : Point2.Value.WorldZ);
            }
        }

        /// <summary>
        /// Send a chat message from the user.
        /// </summary>
        /// <param name="clean">The pre-cleaned message to be sent.</param>
        public void ExecuteChat(string clean)
        {
            //Event
            ClientPreChatEventArgs e1 = new ClientPreChatEventArgs(this, clean);
            _Player.Server.PluginManager.CallEvent(Event.PLAYER_PRE_CHAT, e1);
            if (e1.EventCanceled) return;
            clean = e1.Message;
            //End Event

            if (_Player.IsMuted)
            {
                SendMessage("You have been muted");
                return;
            }

            if ((clean = OnChat(clean)) != null)
            {
                //Event
                ClientChatEventArgs e2 = new ClientChatEventArgs(this, clean);
                _Player.Server.PluginManager.CallEvent(Event.PLAYER_CHAT, e2);
                if (e2.EventCanceled) return;
                clean = e2.Message;
                //End Event

                _Player.Server.Broadcast(Chat.Format(_Player.DisplayName, clean));
                Logger.Log(Logger.LogLevel.Info, "{0}: {1}", _Player.DisplayName, clean);
            }
        }

        private string OnChat(string message)
        {
            ChatEventArgs e = new ChatEventArgs
            {
                Cancelled = false,
                Message = message
            };
            if (ChatMessage != null)
                ChatMessage.Invoke(this, e);
            return e.Cancelled ? null : e.Message;
        }

        /// <summary>
        /// Execute a command in the context of the user.
        /// </summary>
        /// <param name="command">The command text, with the slash removed.</param>
        public void ExecuteCommand(string command)
        {
            //Event
            ClientPreCommandEventArgs e = new ClientPreCommandEventArgs(this, command);
            _Player.Server.PluginManager.CallEvent(Event.PLAYER_PRE_COMMAND, e);
            if (e.EventCanceled) return;
            command = e.Command;
            //End Event

            int argsPos = command.IndexOf(" ");
            string baseCommand = command;
            if (argsPos != -1)
                baseCommand = command.Substring(0, command.IndexOf(" "));
            if (!_Player.CanUseCommand(baseCommand))
            {
                SendMessage("You do not have permission to use that command");
                return;
            }
            Logger.Log(Logger.LogLevel.Info, _Player.DisplayName + " issued server command: " + command);
            _Player.Server.Broadcast(_Player.DisplayName + " executed command " + command, this);
            CommandProc(command, Chat.Tokenize(command));
        }

        private void CommandProc(string raw, string[] tokens)
        {
            ClientCommand cmd;
            try
            {
                cmd = _Player.Server.ClientCommandHandler.Find(tokens[0]) as ClientCommand;
            }
            catch (CommandNotFoundException e)
            {
                SendMessage(ChatColor.Red + e.Message);
                return;
            }
            try
            {
                //Event
                ClientCommandEventArgs e = new ClientCommandEventArgs(this, cmd, tokens);
                _Player.Server.PluginManager.CallEvent(Event.PLAYER_COMMAND, e);
                if (e.EventCanceled) return;
                tokens = e.Tokens;
                //End Event

                cmd.Use(this, tokens);
            }
            catch (Exception e)
            {
                SendMessage("There was an error while executing the command.");
                _Player.Server.Logger.Log(e);
            }
        }

        public void SetHealth(string[] tokens)
        {
            if (tokens.Length < 1)
            {
                _Player.SetHealth(20);
                return;
            }
            _Player.SetHealth(short.Parse(tokens[1]));
        }
    }
}
