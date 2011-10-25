using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public class PacketEvent : IChraftEventHandler
    {
        public PacketEvent()
        {
            events.AddRange(new Event[] { Event.PacketReceived, Event.PacketSent });
        }
        public EventType Type { get { return EventType.Other; } }
        public List<Event> Events { get { return events; } }
        public List<EventListener> Plugins { get { return plugins; } }
        private List<Event> events = new List<Event>();
        private List<EventListener> plugins = new List<EventListener>();

        public void CallEvent(Event Event, ChraftEventArgs e)
        {
            switch (Event)
            {
                case Event.PacketReceived:
                    OnPacketReceived(e as PacketRecevedEventArgs);
                    break;
                case Event.PacketSent:
                    OnPacketSent(e as PacketSentEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener listener)
        {
            plugins.Add(listener);
        }
        #region Local Hooks
        private void OnPacketReceived(PacketRecevedEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PacketListener pl = (PacketListener)el.Listener;
                if (el.Event == Event.PacketReceived)
                    pl.OnPacketReceived(e);
            }
        }
        private void OnPacketSent(PacketSentEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PacketListener pl = (PacketListener)el.Listener;
                if (el.Event == Event.PacketSent)
                    pl.OnPacketSent(e);
            }
        }
        #endregion
    }

}
