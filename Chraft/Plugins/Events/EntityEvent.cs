using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;
using Chraft.Plugins.Listener;

namespace Chraft.Plugins.Events
{
    public class EntityEvent : ChraftEventHandler
    {
        public EntityEvent()
        {
            events.AddRange(new Event[]{Event.ENTITY_ATTACK, Event.ENTITY_DAMAGE, Event.ENTITY_DEATH,
                Event.ENTITY_MOVE, Event.ENTITY_SPAWN});
        }
        public EventType Type
        {
            get { return EventType.Entity; }
        }
        public List<Event> Events { get { return events; } }
        public List<EventListener> Plugins { get { return plugins; } }

        private List<Event> events = new List<Event>();
        private List<EventListener> plugins = new List<EventListener>();

        public void CallEvent(Event Event, Args.ChraftEventArgs e)
        {
            switch (Event)
            {
                case Event.ENTITY_ATTACK:
                    OnAttack(e as EntityAttackEventArgs);
                    break;
                case Event.ENTITY_DAMAGE:
                    OnDamaged(e as EntityDamageEventArgs);
                    break;
                case Event.ENTITY_DEATH:
                    OnDeath(e as EntityDeathEventArgs);
                    break;
                case Event.ENTITY_MOVE:
                    OnMove(e as EntityMoveEventArgs);
                    break;
                case Event.ENTITY_SPAWN:
                    OnSpawn(e as EntitySpawnEventArgs);
                    break;
            }
        }

        public void RegisterEvent(EventListener Listener)
        {
            plugins.Add(Listener);
        }

        #region LocalHooks
        private void OnDeath(EntityDeathEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.ENTITY_DEATH)
                {
                    EntityListener l = el.Listener as EntityListener;
                    l.OnDeath(e);
                }
            }
        }
        private void OnSpawn(EntitySpawnEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.ENTITY_SPAWN)
                {
                    EntityListener l = el.Listener as EntityListener;
                    l.OnSpawn(e);
                }
            }
        }
        private void OnMove(EntityMoveEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.ENTITY_MOVE)
                {
                    EntityListener l = el.Listener as EntityListener;
                    l.OnMove(e);
                }
            }
        }
        private void OnDamaged(EntityDamageEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.ENTITY_DAMAGE)
                {
                    EntityListener l = el.Listener as EntityListener;
                    l.OnDamaged(e);
                }
            }
        }
        private void OnAttack(EntityAttackEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                if (el.Event == Event.ENTITY_ATTACK)
                {
                    EntityListener l = el.Listener as EntityListener;
                    l.OnAttack(e);
                }
            }
        }
        #endregion
    }
}
