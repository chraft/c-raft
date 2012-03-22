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
    public class ClientEvent : IChraftEventHandler
    {
        public ClientEvent()
        {
            events.AddRange(new Event[]{Event.PlayerJoined, Event.PlayerLeft, Event.PlayerCommand,
                Event.PlayerPreCommand, Event.PlayerChat, Event.PlayerPreChat, Event.PlayerKicked,
                Event.PlayerMove, Event.PlayerDied});
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
                case PluginSystem.Event.Event.PlayerJoined:
                    OnPlayerJoined(e as ClientJoinedEventArgs);
                    break;
                case PluginSystem.Event.Event.PlayerLeft:
                    OnPlayerLeft(e as ClientLeftEventArgs);
                    break;
                case PluginSystem.Event.Event.PlayerCommand:
                    OnPlayerCommand(e as ClientCommandEventArgs);
                    break;
                case PluginSystem.Event.Event.PlayerPreCommand:
                    OnPlayerPreCommand(e as ClientCommandEventArgs);
                    break;
                case PluginSystem.Event.Event.PlayerChat:
                    OnPlayerChat(e as ClientChatEventArgs);
                    break;
                case PluginSystem.Event.Event.PlayerPreChat:
                    OnPlayerPreChat(e as ClientPreChatEventArgs);
                    break;
                case PluginSystem.Event.Event.PlayerKicked:
                    OnPlayerKicked(e as ClientKickedEventArgs);
                    break;
                case PluginSystem.Event.Event.PlayerMove:
                    OnPlayerMoved(e as ClientMoveEventArgs);
                    break;
                case PluginSystem.Event.Event.PlayerDied:
                    OnPlayerDeath(e as ClientDeathEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener listener)
        {
            plugins.Add(listener);
        }
        #region Local Hooks
        private void OnPlayerJoined(ClientJoinedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerJoined)
                    pl.OnPlayerJoined(e);
            }
        }
        private void OnPlayerLeft(ClientLeftEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerLeft)
                    pl.OnPlayerLeft(e);
            }
        }
        private void OnPlayerCommand(ClientCommandEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerCommand)
                    pl.OnPlayerCommand(e);
            }
        }
        private void OnPlayerPreCommand(ClientCommandEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerPreCommand)
                    pl.OnPlayerPreCommand(e);
            }
        }
        private void OnPlayerChat(ClientChatEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerChat)
                    pl.OnPlayerChat(e);
            }
        }
        private void OnPlayerPreChat(ClientPreChatEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerPreChat)
                    pl.OnPlayerPreChat(e);
            }
        }
        private void OnPlayerKicked(ClientKickedEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)el.Listener;
                if (el.Event == Event.PlayerKicked)
                    pl.OnPlayerKicked(e);
            }
        }
        private void OnPlayerMoved(ClientMoveEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)el.Listener;
                if (el.Event == Event.PlayerMove)
                    pl.OnPlayerMoved(e);
            }
        }
        private void OnPlayerDeath(ClientDeathEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                IPlayerListener pl = (IPlayerListener)el.Listener;
                if (el.Event == Event.PlayerDied)
                    pl.OnPlayerDeath(e);
            }
        }
        #endregion
    }
}
