using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins;
using Chraft.Plugins.Listener;
using Chraft.Commands;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public class PluginEvent : ChraftEventHandler
    {
        public PluginEvent()
        {
            Events.Add(Event.PLUGIN_ENABLED);
            Events.Add(Event.PLUGIN_DISABLED);
            Events.Add(Event.COMMAND_ADDED);
            Events.Add(Event.COMMAND_REMOVED);
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
                case Event.PLUGIN_ENABLED:
                    OnPluginEnabled(e as PluginEnabledEventArgs);
                    break;
                case Event.PLUGIN_DISABLED:
                    OnPluginDisabled(e as PluginDisabledEventArgs);
                    break;
                case Event.COMMAND_ADDED:
                    OnPluginCommandAdded(e as CommandAddedEventArgs);
                    break;
                case Event.COMMAND_REMOVED:
                    OnPluginCommandRemoved(e as CommandRemovedEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener Listener)
        {
            plugins.Add(Listener);
        }
        #region Local Hooks
        private void OnPluginEnabled(PluginEnabledEventArgs e)
        {
            foreach (EventListener v in Plugins)
            {
                PluginListener pl = (PluginListener)v.Listener;
                if (v.Event == Event.PLUGIN_ENABLED)
                    pl.OnPluginEnabled(e);
            }
        }
        private void OnPluginDisabled(PluginDisabledEventArgs e)
        {
            foreach (EventListener v in Plugins)
            {
                PluginListener pl = (PluginListener)v.Listener;
                if (v.Event == Event.PLUGIN_DISABLED)
                    pl.OnPluginDisabled(e);
            }
        }
        private void OnPluginCommandAdded(CommandAddedEventArgs e)
        {
            foreach (EventListener v in Plugins)
            {
                PluginListener pl = (PluginListener)v.Listener;
                if (v.Event == Event.COMMAND_ADDED)
                    pl.OnPluginCommandAdded(e);
            }
        }
        private void OnPluginCommandRemoved(CommandRemovedEventArgs e)
        {
            foreach (EventListener v in Plugins)
            {
                PluginListener pl = (PluginListener)v.Listener;
                if (v.Event == Event.COMMAND_REMOVED)
                    pl.OnPluginCommandRemoved(e);
            }
        }
        #endregion
    }
}
