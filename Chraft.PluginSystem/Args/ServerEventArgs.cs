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
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;

namespace Chraft.PluginSystem.Args
{
    /// <summary>
    /// The base EventArgs for a Server Event.
    /// </summary>
    public class ServerEventArgs : ChraftEventArgs
    {
        public virtual IServer Server { get; private set; }

        public ServerEventArgs(IServer server)
            : base()
        {
            Server = server;
        }
    }
    /// <summary>
    /// EventArgs for a Server Broadcast Event.
    /// </summary>
    public class ServerBroadcastEventArgs : ServerEventArgs
    {
        public string Message { get; set; }
        public IClient ExcludeClient { get; set; }

        public ServerBroadcastEventArgs(IServer server, string message, IClient excludeClient)
            : base(server)
        {
            Message = message;
            ExcludeClient = excludeClient;
        }
    }
    /// <summary>
    /// EventArgs for a Server Command Event.
    /// </summary>
    public class ServerCommandEventArgs : ServerEventArgs
    {
        public IServerCommand Command { get; set; }
        public string[] Tokens { get; set; }

        public ServerCommandEventArgs(IServer server, IServerCommand command, string[] tokens)
            : base(server)
        {
            Command = command;
            Tokens = tokens;
        }
    }
    /// <summary>
    /// EventArgs for a Server Chat Event.
    /// </summary>
    public class ServerChatEventArgs : ServerEventArgs
    {
        public string Message { get; set; }

        public ServerChatEventArgs(IServer server, string Message)
            : base(server)
        {
            this.Message = Message;
        }
    }
    /// <summary>
    /// EventArgs for a Logger Event.
    /// </summary>
    public class LoggerEventArgs : ChraftEventArgs
    {
        public ILogger Logger { get; private set; }
        public LogLevel LogLevel { get; set; }
        public string LogMessage { get; set; }
        public Exception Exception { get; private set; }

        public LoggerEventArgs(ILogger logger, LogLevel logLevel, string Message, Exception exception = null)
        {
            Logger = logger;
            LogLevel = logLevel;
            LogMessage = Message;
            if (exception != null) Exception = exception;
        }
    }
    /// <summary>
    /// EventArgs for a Server Accept Event.
    /// </summary>
    public class ClientAcceptedEventArgs : ServerEventArgs
    {
        public override bool EventCanceled { get { return false; } set { } }
        public IClient Client { get; private set; }

        public ClientAcceptedEventArgs(IServer server, IClient client)
            : base(server)
        {
            Client = client;
        }
    }
}
