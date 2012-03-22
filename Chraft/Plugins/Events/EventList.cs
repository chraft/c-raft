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
using System.Collections;
using Chraft.PluginSystem.Event;

namespace Chraft.Plugins.Events
{
    public class EventList : IEnumerable<IChraftEventHandler>
    {
        private List<IChraftEventHandler> Events = new List<IChraftEventHandler>();
        private Dictionary<Event, IChraftEventHandler> Mappings = new Dictionary<Event, IChraftEventHandler>();
        
        public EventList() { }
        /// <summary>
        /// Gets an instance of the Event Handler holding the given event.
        /// </summary>
        /// <param name="Event">The name of the event(e.g. PLUGIN_ENABLED).</param>
        /// <returns>Event Handler</returns>
        public IChraftEventHandler Find(Event Event)
        {
            foreach (IChraftEventHandler e in Events)
            {
                if (e.Events.Contains(Event))
                {
                    return e;
                }
            }
            throw new EventNotFoundException("\"" + Event + "\" was not found.  Please ask the developer(s) to fix this error.");
            //Insted of returning null we throw a EventNotFoundException.  
            //Yep... we will not be getting a NullRefrenceException here.
            //return null;
        }
        /// <summary>
        /// Adds an event handler.
        /// </summary>
        /// <param name="e">The Event handler to add.</param>
        public void Add(IChraftEventHandler e)
        {
            Events.Add(e);
            foreach (Event Event in e.Events)
            {
                Mappings.Add(Event, e);
            }
        }
        /// <summary>
        /// Removes a single event from the mappings.  Any further calles to this event will return a EventNotFoundException.
        /// </summary>
        /// <param name="Event">The name of the event to remove.</param>
        public void Remove(Event Event)
        {
            Mappings.Remove(Event);
        }
        /// <summary>
        /// Removes an event handler.
        /// 
        /// This removes the event handler and ALL Mappings linked to this handler.
        /// </summary>
        /// <param name="e"></param>
        public void RemoveEventHandler(IChraftEventHandler e)
        {
            foreach(KeyValuePair<Event, IChraftEventHandler> ed in from ed in Mappings 
                    where Mappings.ContainsValue(e) select ed)
            {
                Mappings.Remove(ed.Key);
            }
            Events.Remove(e);
        }

        public IEnumerator<IChraftEventHandler> GetEnumerator()
        {
            return Events.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
           return GetEnumerator();
        }
    }
    /// <summary>
    /// A custom Exception to handle non-existant events.
    /// </summary>
    public class EventNotFoundException : Exception
    {
        public EventNotFoundException() { }
        public EventNotFoundException(string message) : base(message) { }
        public EventNotFoundException(string message, Exception inner) : base(message, inner) { }
    }
}
