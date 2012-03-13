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
using Chraft.Plugins;
using Chraft.Commands;
using Chraft.Plugins.Events.Args;

namespace Chraft.PluginSystem.Events
{
    public class PluginEvent : IChraftEventHandler
    {
        public PluginEvent()
        {
            Events.Add(Event.PluginEnabled);
            Events.Add(Event.PluginDisabled);
            Events.Add(Event.CommandAdded);
            Events.Add(Event.CommandRemoved);
        }
        public EventType Type { get { return EventType.Plugin; } }
        public List<Event> Events { get { return events; } }
        private List<Event> events = new List<Event>();
        public List<EventListener> Plugins { get { return plugins; } }
        private List<EventListener> plugins = new List<EventListener>();

        public void CallEvent(Event Event, ChraftEventArgs e)
        {
            switch (Event)
            {
                case Event.PluginEnabled:
                    OnPluginEnabled(e as PluginEnabledEventArgs);
                    break;
                case Event.PluginDisabled:
                    OnPluginDisabled(e as PluginDisabledEventArgs);
                    break;
                case Event.CommandAdded:
                    OnPluginCommandAdded(e as CommandAddedEventArgs);
                    break;
                case Event.CommandRemoved:
                    OnPluginCommandRemoved(e as CommandRemovedEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener listener)
        {
            plugins.Add(listener);
        }
        #region Local Hooks
        private void OnPluginEnabled(PluginEnabledEventArgs e)
        {
            foreach (EventListener v in Plugins)
            {
                PluginListener pl = (PluginListener)v.Listener;
                if (v.Event == Event.PluginEnabled)
                    pl.OnPluginEnabled(e);
            }
        }
        private void OnPluginDisabled(PluginDisabledEventArgs e)
        {
            foreach (EventListener v in Plugins)
            {
                PluginListener pl = (PluginListener)v.Listener;
                if (v.Event == Event.PluginDisabled)
                    pl.OnPluginDisabled(e);
            }
        }
        private void OnPluginCommandAdded(CommandAddedEventArgs e)
        {
            foreach (EventListener v in Plugins)
            {
                PluginListener pl = (PluginListener)v.Listener;
                if (v.Event == Event.CommandAdded)
                    pl.OnPluginCommandAdded(e);
            }
        }
        private void OnPluginCommandRemoved(CommandRemovedEventArgs e)
        {
            foreach (EventListener v in Plugins)
            {
                PluginListener pl = (PluginListener)v.Listener;
                if (v.Event == Event.CommandRemoved)
                    pl.OnPluginCommandRemoved(e);
            }
        }
        #endregion
    }
}
