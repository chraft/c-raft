using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Listener;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins.Events
{
    public class ClientEvent : IChraftEventHandler
    {
        public ClientEvent()
        {
            events.AddRange(new Event[]{Event.PlayerJoined, Event.PlayerLeft, Event.PlayerCommand,
                Event.PlayerPreCommand, Event.PlayerChat, Event.PlayerPreChat, Event.PlayerKicked,
                Event.PlayerMove, Event.PlayerDied});
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
                case Event.PlayerJoined:
                    OnPlayerJoined(e as ClientJoinedEventArgs);
                    break;
                case Event.PlayerLeft:
                    OnPlayerLeft(e as ClientLeftEventArgs);
                    break;
                case Event.PlayerCommand:
                    OnPlayerCommand(e as ClientCommandEventArgs);
                    break;
                case Event.PlayerPreCommand:
                    OnPlayerPreCommand(e as ClientCommandEventArgs);
                    break;
                case Event.PlayerChat:
                    OnPlayerChat(e as ClientChatEventArgs);
                    break;
                case Event.PlayerPreChat:
                    OnPlayerPreChat(e as ClientPreChatEventArgs);
                    break;
                case Event.PlayerKicked:
                    OnPlayerKicked(e as ClientKickedEventArgs);
                    break;
                case Event.PlayerMove:
                    OnPlayerMoved(e as ClientMoveEventArgs);
                    break;
                case Event.PlayerDied:
                    OnPlayerDeath(e as ClientDeathEventArgs);
                    break;
            }
        }
        public void RegisterEvent(EventListener listener)
        {
            plugins.Add(listener);
        }
        #region Local Hooks
        private void OnPlayerJoined(ClientJoinedEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerJoined)
                    pl.OnPlayerJoined(e);
            }
        }
        private void OnPlayerLeft(ClientLeftEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerLeft)
                    pl.OnPlayerLeft(e);
            }
        }
        private void OnPlayerCommand(ClientCommandEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerCommand)
                    pl.OnPlayerCommand(e);
            }
        }
        private void OnPlayerPreCommand(ClientCommandEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerPreCommand)
                    pl.OnPlayerPreCommand(e);
            }
        }
        private void OnPlayerChat(ClientChatEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerChat)
                    pl.OnPlayerChat(e);
            }
        }
        private void OnPlayerPreChat(ClientPreChatEventArgs e)
        {
            foreach (EventListener bl in Plugins)
            {
                PlayerListener pl = (PlayerListener)bl.Listener;
                if (bl.Event == Event.PlayerPreChat)
                    pl.OnPlayerPreChat(e);
            }
        }
        private void OnPlayerKicked(ClientKickedEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PlayerListener pl = (PlayerListener)el.Listener;
                if (el.Event == Event.PlayerKicked)
                    pl.OnPlayerKicked(e);
            }
        }
        private void OnPlayerMoved(ClientMoveEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PlayerListener pl = (PlayerListener)el.Listener;
                if (el.Event == Event.PlayerMove)
                    pl.OnPlayerMoved(e);
            }
        }
        private void OnPlayerDeath(ClientDeathEventArgs e)
        {
            foreach (EventListener el in Plugins)
            {
                PlayerListener pl = (PlayerListener)el.Listener;
                if (el.Event == Event.PlayerDied)
                    pl.OnPlayerDeath(e);
            }
        }
        #endregion
    }
}
