using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public interface ChraftEventHandler
    {
        /// <summary>
        /// The type of event this is.
        /// </summary>
        EventType Type { get; }
        /// <summary>
        /// The list of events this handler holds.
        /// Events should be added in the constructer.
        /// </summary>
        List<Event> Events { get; }
        /// <summary>
        /// The list of EventListeners that listen to this handler.
        /// </summary>
        List<EventListener> Plugins { get; }

        /// <summary>
        /// Calls an event in this event handler.
        /// This should only be called from the PluginManager.
        /// </summary>
        /// <param name="Event">The event to call</param>
        /// <param name="e">Attached EventArgs</param>
        void CallEvent(Event Event, ChraftEventArgs e);
        /// <summary>
        /// Registers an event listener with this event handler.
        /// This should only be called from the PluginManager.
        /// </summary>
        /// <param name="Listener">An EventListener instance.</param>
        void RegisterEvent(EventListener Listener);
    }
    /// <summary>
    /// A list of event types.
    /// </summary>
    public enum EventType
    {
        World,
        Player,
        Server,
        Plugin,
        IRC,
        Entity,
        Other
    }
    public enum Event
    {
        ENTITY_DEATH,
        ENTITY_SPAWN,
        ENTITY_MOVE,
        ENTITY_DAMAGE,
        ENTITY_ATTACK,
        PLAYER_JOINED,
        PLAYER_LEFT,
        PLAYER_COMMAND,
        PLAYER_PRE_COMMAND,
        PLAYER_CHAT,
        PLAYER_PRE_CHAT,
        PLAYER_KICKED,
        PLAYER_MOVE,
        PLAYER_DIED,
        IRC_JOINED,
        IRC_LEFT,
        IRC_MESSAGE,
        PACKET_RECEIVED,
        PACKET_SENT,
        PLUGIN_ENABLED,
        PLUGIN_DISABLED,
        COMMAND_ADDED,
        COMMAND_REMOVED,
        WORLD_LOAD,
        WORLD_UNLOAD,
        WORLD_JOIN,
        WORLD_LEAVE,
        WORLD_CREATE,
        WORLD_DELETE,
        SERVER_COMMAND,
        SERVER_CHAT,
        SERVER_BROADCAST,
        SERVER_ACCEPT,
        LOGGER_LOG
    }
    public struct EventListener
    {
        /// <summary>
        /// initializes a new instance of the EventListener struct.
        /// </summary>
        /// <param name="Listener">A valid Listener.  
        /// All Listeners are in the Chraft.Plugins.Listener namespace.</param>
        /// <param name="Plugin">The IPlugin that the listener is attached to.</param>
        /// <param name="Event">The name of the event to listen for.</param>
        public EventListener(ChraftListener Listener, IPlugin Plugin, Event Event)
        {
            this.Listener = Listener;
            this.Event = Event;
            this.Plugin = Plugin;
        }
        public ChraftListener Listener;
        public IPlugin Plugin;
        public Event Event;
    }
}
