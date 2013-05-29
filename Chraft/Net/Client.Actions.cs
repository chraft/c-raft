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
using System.Linq;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Server;
using Chraft.Commands;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;

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
        internal void ExecuteChat(string clean)
        {
            //Event
            ClientPreChatEventArgs e1 = new ClientPreChatEventArgs(this, clean);
            _player.Server.PluginManager.CallEvent(Event.PlayerPreChat, e1);
            if (e1.EventCanceled) return;
            clean = e1.Message;
            //End Event

            if (_player.IsMuted)
            {
                SendMessage("You have been muted");
                return;
            }

            if ((clean = OnChat(clean)) != null)
            {
                //Event
                ClientChatEventArgs e2 = new ClientChatEventArgs(this, clean);
                _player.Server.PluginManager.CallEvent(Event.PlayerChat, e2);
                if (e2.EventCanceled) return;
                clean = e2.Message;
                //End Event

                _player.Server.Broadcast(Chat.Format(_player.DisplayName, clean));
                Logger.Log(LogLevel.Info, "{0}: {1}", _player.DisplayName, clean);
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
        internal void ExecuteCommand(string command)
        {
            //Event
            ClientPreCommandEventArgs e = new ClientPreCommandEventArgs(this, command);
            _player.Server.PluginManager.CallEvent(Event.PlayerPreCommand, e);
            if (e.EventCanceled) return;
            command = e.Command;
            //End Event

            int argsPos = command.IndexOf(" ");
            string baseCommand = command;
            if (argsPos != -1)
                baseCommand = command.Substring(0, command.IndexOf(" "));
            if (!_player.CanUseCommand(baseCommand))
            {
                SendMessage("You do not have permission to use that command");
                return;
            }
            Logger.Log(LogLevel.Info, _player.DisplayName + " issued server command: " + command);
            _player.Server.Broadcast(_player.DisplayName + " executed command " + command, this);
            CommandProc(baseCommand, command, Chat.Tokenize(command));
        }

        private void CommandProc(string commandName, string raw, string[] tokens)
        {
            var cleanedTokens = tokens.Skip(1).ToArray();
            IClientCommand cmd;
            try
            {
                cmd = _player.Server.ClientCommandHandler.Find(commandName) as IClientCommand;
            }
            catch (MultipleCommandsMatchException e)
            {
                SendMessage(ChatColor.Red + "Multiple commands has been found:");
                foreach (var s in e.Commands)
                    SendMessage(string.Format(" {0}{1}", ChatColor.Red, s));

                return;
            }
            catch (CommandNotFoundException e)
            {
                SendMessage(ChatColor.Red + e.Message);
                return;
            }
            try
            {
                //Event
                ClientCommandEventArgs e = new ClientCommandEventArgs(this, cmd, cleanedTokens);
                _player.Server.PluginManager.CallEvent(Event.PlayerCommand, e);
                if (e.EventCanceled) return;
                cleanedTokens = e.Tokens;
                //End Event

                cmd.Use(this, commandName, cleanedTokens);
            }
            catch (Exception e)
            {
                SendMessage("There was an error while executing the command.");
                _player.Server.Logger.Log(e);
            }
        }
    }
}
