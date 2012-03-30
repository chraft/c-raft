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
                case PluginSystem.Event.Event.PacketReceived:
                    OnPacketReceived(e as PacketRecevedEventArgs);
                    break;
                case PluginSystem.Event.Event.PacketSent:
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
                IPacketListener pl = (IPacketListener)el.Listener;
                if (el.Event == Event.PacketReceived)
                    pl.OnPacketReceived(e);
            }
        }
        private void OnPacketSent(PacketSentEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                IPacketListener pl = (IPacketListener)el.Listener;
                if (el.Event == Event.PacketSent)
                    pl.OnPacketSent(e);
            }
        }
        #endregion
    }

}
