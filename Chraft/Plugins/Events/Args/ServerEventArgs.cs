using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Commands;
using Chraft.Net;

namespace Chraft.Plugins.Events.Args
{
    /// <summary>
    /// The base EventArgs for a Server Event.
    /// </summary>
    public class ServerEventArgs : ChraftEventArgs
    {
        public virtual Server Server { get; private set; }

        public ServerEventArgs(Server server)
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
        public Client ExcludeClient { get; set; }

        public ServerBroadcastEventArgs(Server server, string message, Client excludeClient)
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
        public ServerCommand Command { get; set; }
        public string[] Tokens { get; set; }

        public ServerCommandEventArgs(Server server, ServerCommand command, string[] tokens)
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

        public ServerChatEventArgs(Server server, string Message)
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
        public Logger Logger { get; private set; }
        public Logger.LogLevel LogLevel { get; set; }
        public string LogMessage { get; set; }
        public Exception Exception { get; private set; }

        public LoggerEventArgs(Logger logger, Logger.LogLevel logLevel, string Message, Exception exception = null)
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
        public Client Client { get; private set; }

        public ClientAcceptedEventArgs(Server server, Client client)
            : base(server)
        {
            Client = client;
        }
    }
}
