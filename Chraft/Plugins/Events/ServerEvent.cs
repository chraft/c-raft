using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public class ServerEvent : ChraftEventHandler
    {
        public ServerEvent()
        {
            events.AddRange(new Event[] {Event.SERVER_BROADCAST, Event.SERVER_CHAT, Event.SERVER_COMMAND,
            Event.SERVER_ACCEPT, Event.LOGGER_LOG});
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
                case Event.LOGGER_LOG:
                    OnLog(e as LoggerEventArgs);
                    break;
                case Event.SERVER_ACCEPT:
                    OnAccept(e as ClientAcceptedEventArgs);
                    break;
                case Event.SERVER_BROADCAST:
                    OnBroadcast(e as ServerBroadcastEventArgs);
                    break;
                case Event.SERVER_CHAT:
                    OnChat(e as ServerChatEventArgs);
                    break;
                case Event.SERVER_COMMAND:
                    OnCommand(e as ServerCommandEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener Listener)
        {
            plugins.Add(Listener);
        }
        #region Local Hooks
        private void OnBroadcast(ServerBroadcastEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.SERVER_BROADCAST)
                    sl.OnBroadcast(e);
            }
        }
        private void OnLog(LoggerEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.LOGGER_LOG)
                    sl.OnLog(e);
            }
        }
        private void OnAccept(ClientAcceptedEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.SERVER_ACCEPT)
                    sl.OnAccept(e);
            }
        }
        private void OnCommand(ServerCommandEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.SERVER_COMMAND)
                    sl.OnCommand(e);
            }
        }
        private void OnChat(ServerChatEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                ServerListener sl = (ServerListener)el.Listener;
                if (el.Event == Event.SERVER_CHAT)
                    sl.OnChat(e);
            }
        }
        #endregion
    }
}
