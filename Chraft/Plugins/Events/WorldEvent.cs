using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public class WorldEvent : ChraftEventHandler
    {
        public WorldEvent()
        {
            Events.Add(Event.WORLD_LOAD);
            Events.Add(Event.WORLD_UNLOAD);
            Events.Add(Event.WORLD_JOIN);
            Events.Add(Event.WORLD_LEAVE);
            Events.Add(Event.WORLD_CREATE);
            Events.Add(Event.WORLD_DELETE);
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
                case Event.WORLD_LOAD:
                    OnWorldLoaded(e as WorldLoadEventArgs);
                    break;
                case Event.WORLD_UNLOAD:
                    OnLeveUnloaded(e as WorldUnloadEventArgs);
                    break;
                case Event.WORLD_JOIN:
                    OnWorldJoined(e as WorldJoinedEventArgs);
                    break;
                case Event.WORLD_LEAVE:
                    OnWorldLeft(e as WorldLeftEventArgs);
                    break;
                case Event.WORLD_CREATE:
                    OnWorldCreated(e as WorldCreatedEventArgs);
                    break;
                case Event.WORLD_DELETE:
                    OnWorldDeleted(e as WorldDeletedEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener Listener)
        {
            plugins.Add(Listener);
        }
        #region Local Hooks
        private void OnWorldLoaded(WorldLoadEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                WorldListener ll = (WorldListener)bl.Listener;
                if (bl.Event == Event.WORLD_LOAD)
                    ll.OnWorldLoaded(e);
            }
        }
        private void OnLeveUnloaded(WorldUnloadEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                WorldListener ll = (WorldListener)bl.Listener;
                if (bl.Event == Event.WORLD_UNLOAD)
                    ll.OnWorldUnloaded(e);
            }
        }
        private void OnWorldJoined(WorldJoinedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                WorldListener ll = (WorldListener)bl.Listener;
                if (bl.Event == Event.WORLD_JOIN)
                    ll.OnWorldJoined(e);
            }
        }
        private void OnWorldLeft(WorldLeftEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                WorldListener ll = (WorldListener)bl.Listener;
                if (bl.Event == Event.WORLD_LEAVE)
                    ll.OnWorldLeft(e);
            }
        }
        private void OnWorldCreated(WorldCreatedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                WorldListener ll = (WorldListener)bl.Listener;
                if (bl.Event == Event.WORLD_CREATE)
                    ll.OnWorldCreated(e);
            }
        }
        private void OnWorldDeleted(WorldDeletedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                WorldListener ll = (WorldListener)bl.Listener;
                if (bl.Event == Event.WORLD_DELETE)
                    ll.OnWorldDeleted(e);
            }
        }
        #endregion
    }
}
