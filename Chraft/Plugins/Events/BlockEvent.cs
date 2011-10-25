using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;
using Chraft.Plugins.Listener;

namespace Chraft.Plugins.Events
{
    /// <summary>
    /// This class contains all the possible block events
    /// </summary>
    public class BlockEvent : IChraftEventHandler
    {
        public BlockEvent()
        {
            events.AddRange(new Event[]{Event.BlockDestroy, Event.BlockPlace,
                Event.BlockTouch });
        }
        public EventType Type
        {
            get { return EventType.Block; }
        }
        public List<Event> Events { get { return events; } }
        public List<EventListener> Plugins { get { return plugins; } }

        private List<Event> events = new List<Event>();
        private List<EventListener> plugins = new List<EventListener>();

        public void CallEvent(Event Event, Args.ChraftEventArgs e)
        {
            switch (Event)
            {
                case Event.BlockDestroy:
                    OnDestroy(e as BlockDestroyEventArgs);
                    break;
                case Event.BlockPlace:
                    OnPlace(e as BlockPlaceEventArgs);
                    break;
                case Event.BlockTouch:
                    OnTouch(e as BlockTouchEventArgs);
                    break;
            }
        }

        public void RegisterEvent(EventListener listener)
        {
            plugins.Add(listener);
        }

        #region LocalHooks
        private void OnDestroy(BlockDestroyEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.BlockDestroy)
                {
                    BlockListener l = el.Listener as BlockListener;
                    l.OnDestroy(e);
                }
            }
        }
        private void OnPlace(BlockPlaceEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.BlockPlace)
                {
                    BlockListener l = el.Listener as BlockListener;
                    l.OnPlace(e);
                }
            }
        }
        private void OnTouch(BlockTouchEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.BlockTouch)
                {
                    BlockListener l = el.Listener as BlockListener;
                    l.OnTouch(e);
                }
            }
        }
        #endregion
    }
}
