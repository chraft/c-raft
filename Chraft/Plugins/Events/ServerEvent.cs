using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public class ServerEvent : IChraftEventHandler
    {
        public ServerEvent()
        {
            events.AddRange(new Event[] {Event.ServerBroadcast, Event.ServerChat, Event.ServerCommand,
            Event.ServerAccept, Event.LoggerLog});
        }
        public EventType Type { get { return EventType.Player; } }
        public List<Event> Events { get { return events; } }
        public List<EventListener> Plugins { get { return plugins; } }
        private List<Event> events = new List<Event>();
        private List<EventListener> plugins = new List<EventListener>();

        public void CallEvent(Event Event, ChraftEventArgs e)
        {
            switch (Event)
            {
                case Event.LoggerLog:
                    OnLog(e as LoggerEventArgs);
                    break;
                case Event.ServerAccept:
                    OnAccept(e as ClientAcceptedEventArgs);
                    break;
                case Event.ServerBroadcast:
                    OnBroadcast(e as ServerBroadcastEventArgs);
                    break;
                case Event.ServerChat:
                    OnChat(e as ServerChatEventArgs);
                    break;
                case Event.ServerCommand:
                    OnCommand(e as ServerCommandEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener listener)
        {
            plugins.Add(listener);
        }
        #region Local Hooks
        private void OnBroadcast(ServerBroadcastEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.ServerBroadcast)
                    sl.OnBroadcast(e);
            }
        }
        private void OnLog(LoggerEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.LoggerLog)
                    sl.OnLog(e);
            }
        }
        private void OnAccept(ClientAcceptedEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.ServerAccept)
                    sl.OnAccept(e);
            }
        }
        private void OnCommand(ServerCommandEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.ServerCommand)
                    sl.OnCommand(e);
            }
        }
        private void OnChat(ServerChatEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.ServerChat)
                    sl.OnChat(e);
            }
        }
        #endregion
    }
}
