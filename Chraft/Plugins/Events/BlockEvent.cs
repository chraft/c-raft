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

        public void CallEvent(Event Event, ChraftEventArgs e)
        {
            switch (Event)
            {
                case PluginSystem.Event.Event.BlockDestroy:
                    OnDestroy(e as BlockDestroyEventArgs);
                    break;
                case PluginSystem.Event.Event.BlockPlace:
                    OnPlace(e as BlockPlaceEventArgs);
                    break;
                case PluginSystem.Event.Event.BlockTouch:
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
                    IBlockListener l = el.Listener as IBlockListener;
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
                    IBlockListener l = el.Listener as IBlockListener;
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
                    IBlockListener l = el.Listener as IBlockListener;
                    l.OnTouch(e);
                }
            }
        }
        #endregion
    }
}
