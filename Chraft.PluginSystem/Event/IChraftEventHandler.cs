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
using Chraft.PluginSystem.Listener;

namespace Chraft.PluginSystem.Event
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