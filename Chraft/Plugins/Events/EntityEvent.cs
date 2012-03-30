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
    public class EntityEvent : IChraftEventHandler
    {
        public EntityEvent()
        {
            events.AddRange(new Event[]{Event.EntityAttack, Event.EntityDamage, Event.EntityDeath,
                Event.EntityMove, Event.EntitySpawn});
        }
        public EventType Type
        {
            get { return EventType.Entity; }
        }
        public List<Event> Events { get { return events; } }
        public List<EventListener> Plugins { get { return plugins; } }

        private List<Event> events = new List<Event>();
        private List<EventListener> plugins = new List<EventListener>();

        public void CallEvent(Event Event, ChraftEventArgs e)
        {
            switch (Event)
            {
                case PluginSystem.Event.Event.EntityAttack:
                    OnAttack(e as EntityAttackEventArgs);
                    break;
                case PluginSystem.Event.Event.EntityDamage:
                    OnDamaged(e as EntityDamageEventArgs);
                    break;
                case PluginSystem.Event.Event.EntityDeath:
                    OnDeath(e as EntityDeathEventArgs);
                    break;
                case PluginSystem.Event.Event.EntityMove:
                    OnMove(e as EntityMoveEventArgs);
                    break;
                case PluginSystem.Event.Event.EntitySpawn:
                    OnSpawn(e as EntitySpawnEventArgs);
                    break;
            }
        }

        public void RegisterEvent(EventListener listener)
        {
            plugins.Add(listener);
        }

        #region LocalHooks
        private void OnDeath(EntityDeathEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.EntityDeath)
                {
                    IEntityListener l = el.Listener as IEntityListener;
                    l.OnDeath(e);
                }
            }
        }
        private void OnSpawn(EntitySpawnEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.EntitySpawn)
                {
                    IEntityListener l = el.Listener as IEntityListener;
                    l.OnSpawn(e);
                }
            }
        }
        private void OnMove(EntityMoveEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.EntityMove)
                {
                    IEntityListener l = el.Listener as IEntityListener;
                    l.OnMove(e);
                }
            }
        }
        private void OnDamaged(EntityDamageEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.EntityDamage)
                {
                    IEntityListener l = el.Listener as IEntityListener;
                    l.OnDamaged(e);
                }
            }
        }
        private void OnAttack(EntityAttackEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.EntityAttack)
                {
                    IEntityListener l = el.Listener as IEntityListener;
                    l.OnAttack(e);
                }
            }
        }
        #endregion
    }
}
