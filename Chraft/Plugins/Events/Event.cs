using System.Collections.Generic;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public interface IChraftEventHandler
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
        /// <param name="listener">An EventListener instance.</param>
        void RegisterEvent(EventListener listener);
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
        Block,
        Other
    }
    public enum Event
    {
        EntityDeath,
        EntitySpawn,
        EntityMove,
        EntityDamage,
        EntityAttack,
        PlayerJoined,
        PlayerLeft,
        PlayerCommand,
        PlayerPreCommand,
        PlayerChat,
        PlayerPreChat,
        PlayerKicked,
        PlayerMove,
        PlayerDied,
        IrcJoined,
        IrcLeft,
        IrcMessage,
        PacketReceived,
        PacketSent,
        PluginEnabled,
        PluginDisabled,
        CommandAdded,
        CommandRemoved,
        WorldLoad,
        WorldUnload,
        WorldJoin,
        WorldLeave,
        WorldCreate,
        WorldDelete,
        ServerCommand,
        ServerChat,
        ServerBroadcast,
        ServerAccept,
        LoggerLog,
        BlockPlace,
        BlockDestroy,
        BlockTouch
    }
    public struct EventListener
    {
        /// <summary>
        /// initializes a new instance of the EventListener struct.
        /// </summary>
        /// <param name="listener">A valid Listener.  
        /// All Listeners are in the Chraft.Plugins.Listener namespace.</param>
        /// <param name="plugin">The IPlugin that the listener is attached to.</param>
        /// <param name="Event">The name of the event to listen for.</param>
        public EventListener(IChraftListener listener, IPlugin plugin, Event Event)
        {
            this.Listener = listener;
            this.Event = Event;
            this.Plugin = plugin;
        }
        public IChraftListener Listener;
        public IPlugin Plugin;
        public Event Event;
    }
}
