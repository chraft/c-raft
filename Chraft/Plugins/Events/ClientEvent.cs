using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public class ClientEvent : ChraftEventHandler
    {
        public ClientEvent()
        {
            events.AddRange(new Event[]{Event.PLAYER_JOINED, Event.PLAYER_LEFT, Event.PLAYER_COMMAND,
                Event.PLAYER_PRE_COMMAND, Event.PLAYER_CHAT, Event.PLAYER_PRE_CHAT, Event.PLAYER_KICKED,
                Event.PLAYER_MOVE, Event.PLAYER_DIED});
        }
        public EventType Type { get { return EventType.Player; } }
        public List<Event> Events { get { return events; } }
        public List<EventListener> Plugins { get { return plugins; } }
        private List<Event> events = new List<Event>();
        private List<EventListener> plugins = new List<EventListener>();

        public void CallEvent(Event Event, ChraftEventArgs e)
        {
            switch (Event)
            {
                case Event.PLAYER_JOINED:
                    OnPlayerJoined(e as ClientJoinedEventArgs);
                    break;
                case Event.PLAYER_LEFT:
                    OnPlayerLeft(e as ClientLeftEventArgs);
                    break;
                case Event.PLAYER_COMMAND:
                    OnPlayerCommand(e as ClientCommandEventArgs);
                    break;
                case Event.PLAYER_PRE_COMMAND:
                    OnPlayerPreCommand(e as ClientCommandEventArgs);
                    break;
                case Event.PLAYER_CHAT:
                    OnPlayerChat(e as ClientChatEventArgs);
                    break;
                case Event.PLAYER_PRE_CHAT:
                    OnPlayerPreChat(e as ClientPreChatEventArgs);
                    break;
                case Event.PLAYER_KICKED:
                    OnPlayerKicked(e as ClientKickedEventArgs);
                    break;
                case Event.PLAYER_MOVE:
                    OnPlayerMoved(e as ClientMoveEventArgs);
                    break;
                case Event.PLAYER_DIED:
                    OnPlayerDeath(e as ClientDeathEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener Listener)
        {
            plugins.Add(Listener);
        }
        #region Local Hooks
        private void OnPlayerJoined(ClientJoinedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PLAYER_JOINED)
                    pl.OnPlayerJoined(e);
            }
        }
        private void OnPlayerLeft(ClientLeftEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PLAYER_LEFT)
                    pl.OnPlayerLeft(e);
            }
        }
        private void OnPlayerCommand(ClientCommandEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PLAYER_COMMAND)
                    pl.OnPlayerCommand(e);
            }
        }
        private void OnPlayerPreCommand(ClientCommandEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PLAYER_PRE_COMMAND)
                    pl.OnPlayerPreCommand(e);
            }
        }
        private void OnPlayerChat(ClientChatEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PLAYER_CHAT)
                    pl.OnPlayerChat(e);
            }
        }
        private void OnPlayerPreChat(ClientPreChatEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PLAYER_PRE_CHAT)
                    pl.OnPlayerPreChat(e);
            }
        }
        private void OnPlayerKicked(ClientKickedEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PlayerListener pl = (PlayerListener)el.Listener;
                if (el.Event == Event.PLAYER_KICKED)
                    pl.OnPlayerKicked(e);
            }
        }
        private void OnPlayerMoved(ClientMoveEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PlayerListener pl = (PlayerListener)el.Listener;
                if (el.Event == Event.PLAYER_MOVE)
                    pl.OnPlayerMoved(e);
            }
        }
        private void OnPlayerDeath(ClientDeathEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PlayerListener pl = (PlayerListener)el.Listener;
                if (el.Event == Event.PLAYER_DIED)
                    pl.OnPlayerDeath(e);
            }
        }
        #endregion
    }
}
