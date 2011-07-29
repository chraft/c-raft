using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public class PacketEvent : ChraftEventHandler
    {
        public PacketEvent()
        {
            events.AddRange(new Event[] { Event.PACKET_RECEIVED, Event.PACKET_SENT });
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
                case Event.PACKET_RECEIVED:
                    OnPacketReceived(e as PacketRecevedEventArgs);
                    break;
                case Event.PACKET_SENT:
                    OnPacketSent(e as PacketSentEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener Listener)
        {
            plugins.Add(Listener);
        }
        #region Local Hooks
        private void OnPacketReceived(PacketRecevedEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PacketListener pl = (PacketListener)el.Listener;
                if (el.Event == Event.PACKET_RECEIVED)
                    pl.OnPacketReceived(e);
            }
        }
        private void OnPacketSent(PacketSentEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PacketListener pl = (PacketListener)el.Listener;
                if (el.Event == Event.PACKET_SENT)
                    pl.OnPacketSent(e);
            }
        }
        #endregion
    }

}
