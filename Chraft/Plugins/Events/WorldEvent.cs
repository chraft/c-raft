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
using Chraft.PluginSystem.Events.Args;
using Chraft.PluginSystem.Listener;


namespace Chraft.PluginSystem.Events
{
    public class WorldEvent : IChraftEventHandler
    {
        public WorldEvent()
        {
            Events.Add(PluginSystem.Events.Event.WorldLoad);
            Events.Add(Event.WorldUnload);
            Events.Add(Event.WorldJoin);
            Events.Add(Event.WorldLeave);
            Events.Add(Event.WorldCreate);
            Events.Add(Event.WorldDelete);
        }
        public EventType Type { get { return EventType.World; } }
        public List<Event> Events { get { return events; } }
        public List<EventListener> Plugins { get { return plugins; } }
        private List<Event> events = new List<Event>();
        private List<EventListener> plugins = new List<EventListener>();

        public void CallEvent(Event Event, ChraftEventArgs e)
        {
            switch (Event)
            {
                case Event.WorldLoad:
                    OnWorldLoaded(e as WorldLoadEventArgs);
                    break;
                case Event.WorldUnload:
                    OnLeveUnloaded(e as WorldUnloadEventArgs);
                    break;
                case Event.WorldJoin:
                    OnWorldJoined(e as WorldJoinedEventArgs);
                    break;
                case Event.WorldLeave:
                    OnWorldLeft(e as WorldLeftEventArgs);
                    break;
                case Event.WorldCreate:
                    OnWorldCreated(e as WorldCreatedEventArgs);
                    break;
                case Event.WorldDelete:
                    OnWorldDeleted(e as WorldDeletedEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener listener)
        {
            plugins.Add(listener);
        }
        #region Local Hooks
        private void OnWorldLoaded(WorldLoadEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IWorldListener ll = (IWorldListener)bl.Listener;
                if (bl.Event == Event.WorldLoad)
                    ll.OnWorldLoaded(e);
            }
        }
        private void OnLeveUnloaded(WorldUnloadEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IWorldListener ll = (IWorldListener)bl.Listener;
                if (bl.Event == Event.WorldUnload)
                    ll.OnWorldUnloaded(e);
            }
        }
        private void OnWorldJoined(WorldJoinedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IWorldListener ll = (IWorldListener)bl.Listener;
                if (bl.Event == Event.WorldJoin)
                    ll.OnWorldJoined(e);
            }
        }
        private void OnWorldLeft(WorldLeftEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IWorldListener ll = (IWorldListener)bl.Listener;
                if (bl.Event == Event.WorldLeave)
                    ll.OnWorldLeft(e);
            }
        }
        private void OnWorldCreated(WorldCreatedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IWorldListener ll = (IWorldListener)bl.Listener;
                if (bl.Event == Event.WorldCreate)
                    ll.OnWorldCreated(e);
            }
        }
        private void OnWorldDeleted(WorldDeletedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                IWorldListener ll = (IWorldListener)bl.Listener;
                if (bl.Event == Event.WorldDelete)
                    ll.OnWorldDeleted(e);
            }
        }
        #endregion
    }
}
