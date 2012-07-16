using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Listener;

namespace Chraft.Plugins.IrcPlugin
{
    class IrcPluginServerListener : IServerListener 
    {
        private readonly IrcPlugin _plugin;

        public IrcPluginServerListener(IrcPlugin plugin)
        {
            _plugin = plugin;
        }

        #region Implementation of IServerListener

        public void OnBroadcast(ServerBroadcastEventArgs e)
        {
            _plugin.Irc.WriteLine("PRIVMSG {0} :{1}", _plugin.RunningConfiguration.Channel, e.Message.Replace('§', '\x3'));
        }

        public void OnLog(LoggerEventArgs e)
        {
            
        }

        public void OnAccept(ClientAcceptedEventArgs e)
        {
            
        }

        public void OnCommand(ServerCommandEventArgs e)
        {
            
        }

        public void OnChat(ServerChatEventArgs e)
        {
            
        }

        #endregion
    }
}
