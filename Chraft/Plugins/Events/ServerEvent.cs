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

using System.Collections.Generic;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Listener;

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
                case PluginSystem.Event.Event.LoggerLog:
                    OnLog(e as LoggerEventArgs);
                    break;
                case PluginSystem.Event.Event.ServerAccept:
                    OnAccept(e as ClientAcceptedEventArgs);
                    break;
                case PluginSystem.Event.Event.ServerBroadcast:
                    OnBroadcast(e as ServerBroadcastEventArgs);
                    break;
                case PluginSystem.Event.Event.ServerChat:
                    OnChat(e as ServerChatEventArgs);
                    break;
                case PluginSystem.Event.Event.ServerCommand:
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
