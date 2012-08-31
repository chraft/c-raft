using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Xml.Serialization;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Server;
using Chraft.Utilities.Config;

namespace Chraft.Plugins.IrcPlugin
{
    [Plugin]
    public class IrcPlugin : IPlugin 
    {
        // SEND Broacast message.
        //Irc.WriteLine("PRIVMSG {0} :{1}", ChraftConfig.IrcChannel, message.Replace('§', '\x3'));
        #region Implementation of IPlugin

        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public string Name
        {
            get { return "IrcPlugin"; }
        }

        /// <summary>
        /// The author(s) of the plugin.
        /// </summary>
        public string Author
        {
            get { return "sytone"; }
        }

        /// <summary>
        /// A description of the plugin.
        /// </summary>
        public string Description
        {
            get { return "This plugin allows intergration with a IRC server."; }
        }

        /// <summary>
        /// A website for information regarding the plugin.
        /// </summary>
        public string Website
        {
            get { return "https://github.com/chraft"; }
        }

        /// <summary>
        /// The version of the plugin.
        /// </summary>
        public Version Version { get { return Assembly.GetExecutingAssembly().GetName().Version; } }

        /// <summary>
        /// The Server associated with the plugin.
        /// </summary>
        public IServer Server { get; set; }

        /// <summary>
        /// The PluginManager associated with the plugin.
        /// </summary>
        public IPluginManager PluginManager { get; set; }

        /// <summary>
        /// A value indicating whether the plugin is currently enabled.
        /// </summary>
        public bool IsPluginEnabled { get; set; }



        /// <summary>
        /// Called after all default plugins are loaded, at which point it is safe to assume that any dependencies are loaded.
        /// </summary>
        public void Initialize()
        {
            _logger = Server.GetPluginLogger();
            _logger.Log(LogLevel.Info, Name, "Initialize ircplugin Version {0}.", Version);

            _logger.Log(LogLevel.Debug, Name, "Checking folder {0}", ConfigurationDirectory);
            if (!Directory.Exists(ConfigurationDirectory)) { Directory.CreateDirectory(ConfigurationDirectory); }

            _logger.Log(LogLevel.Debug, Name, "Checking configfile {0}", ConfigurationFilename);
            if (!File.Exists(ConfigurationFilename))
            {
                GenerateNewConfigurationFile();
            }

            LoadConfigurationFile();

            _serverListener = new IrcPluginServerListener(this);
        }

        /// <summary>
        /// Associates a Server and a PluginManager with the plugin.
        /// </summary>
        /// <param name="server">The Server object to be associated with the plugin.</param>
        /// <param name="pluginManager">The PluginManager to be associated with the plugin.</param>
        public void Associate(IServer server, IPluginManager pluginManager)
        {
            Server = server;
            PluginManager = pluginManager;
        }

        /// <summary>
        /// Called when the parent PluginManager enables the plugin.
        /// </summary>
        public void OnEnabled()
        {
            if (RunningConfiguration.Enabled)
            {
                StartIrcClient();

                IsPluginEnabled = true;
                PluginManager.RegisterEvent(Event.ServerBroadcast, _serverListener, this);
                _logger.Log(LogLevel.Debug, Name, "Registered for ServerBroadcast.");

                _logger.Log(LogLevel.Debug, Name, "Enabled.");
            }
            else
            {
                _logger.Log(LogLevel.Debug, Name, "Disabled in file.");
            }
        }

        /// <summary>
        /// Called when the parent PluginManager disables the plugin.
        /// </summary>
        public void OnDisabled()
        {
            IsPluginEnabled = false;
            PluginManager.UnregisterEvent(Event.ServerBroadcast, _serverListener, this);
            _logger.Log(LogLevel.Debug, Name, "Unregistered for ServerBroadcast.");

            _logger.Log(LogLevel.Debug, Name, "Disabled.");
        }

        #endregion

        private string _configDirectory = "ircplugin";
        private string _configFilename = "ircplugin.xml";
        private IPluginLogger _logger;
        private IrcPluginServerListener _serverListener;

        /// <summary>
        /// Gets the IRC client, if it has been initialized.
        /// </summary>
        internal IrcClient Irc { get; private set; }

        /// <summary>
        /// Gets the running configuration.
        /// </summary>
        internal IrcPluginConfiguration RunningConfiguration { get; private set; }

        /// <summary>
        /// Gets the configuration directory.
        /// </summary>
        private string ConfigurationDirectory
        {
            get { return Path.Combine(PluginManager.Folder, _configDirectory); }
        }

        /// <summary>
        /// Gets the configuration filename.
        /// </summary>
        private string ConfigurationFilename
        {
            get { return Path.Combine(PluginManager.Folder, _configDirectory, _configFilename); }
        }

        /// <summary>
        /// Starts the irc client.
        /// </summary>
        private void StartIrcClient()
        {
            IPEndPoint ircServerEndPoint = new IPEndPoint(Dns.GetHostEntry(RunningConfiguration.Server).AddressList[0],
                                           RunningConfiguration.Port);
            Irc = new IrcClient(ircServerEndPoint, RunningConfiguration.Nickname, _logger, this);
            Irc.Received += new IrcEventHandler(Irc_Received);
        }

        private void Irc_Received(object sender, IrcEventArgs e)
        {
            if (e.Handled)
                return;

            switch (e.Command)
            {
                case "PRIVMSG": OnIrcPrivMsg(sender, e); break;
                case "NOTICE": OnIrcNotice(sender, e); break;
                case "001": OnIrcWelcome(sender, e); break;
            }
        }

        private void OnIrcPrivMsg(object sender, IrcEventArgs e)
        {
            for (int i = 0; i < e.Args[1].Length; i++)
                if (!ChraftConfig.AllowedChatChars.Contains(e.Args[1][i]))
                    return;
            Server.Broadcast("§7[IRC] " + e.Prefix.Nickname + ":§f " + e.Args[1]);
            e.Handled = true;
        }

        private void OnIrcNotice(object sender, IrcEventArgs e)
        {
            for (int i = 0; i < e.Args[1].Length; i++)
                if (!ChraftConfig.AllowedChatChars.Contains(e.Args[1][i]))
                    return;

            Server.Broadcast("§c[IRC] " + e.Prefix.Nickname + ":§f " + e.Args[1]);
            e.Handled = true;
        }

        private void OnIrcWelcome(object sender, IrcEventArgs e)
        {
            Irc.Join(RunningConfiguration.Channel);
        }

        private void LoadConfigurationFile()
        {
            XmlSerializer deserializer = new XmlSerializer(typeof(IrcPluginConfiguration));
            TextReader textReader = new StreamReader(ConfigurationFilename);
            RunningConfiguration = (IrcPluginConfiguration)deserializer.Deserialize(textReader);
            textReader.Close();

            _logger.Log(LogLevel.Debug, Name, "IrcEnabled: {0}", RunningConfiguration.Enabled);
            _logger.Log(LogLevel.Debug, Name, "IrcServer: {0}", RunningConfiguration.Server);
            _logger.Log(LogLevel.Debug, Name, "IrcChannel: {0}", RunningConfiguration.Channel);
            _logger.Log(LogLevel.Debug, Name, "IrcNickName: {0}", RunningConfiguration.Nickname);
            _logger.Log(LogLevel.Debug, Name, "IrcPort: {0}", RunningConfiguration.Port);
        }

        private void GenerateNewConfigurationFile()
        {
            _logger.Log(LogLevel.Debug, Name, "Creating a new configuration file");
            IrcPluginConfiguration config = new IrcPluginConfiguration();
            config.Enabled = false;
            config.Server = "irc.esper.net";
            config.Channel = "#C#raft";
            config.Nickname = "ChraftBot";
            config.Port = 6667;

            XmlSerializer serializer = new XmlSerializer(typeof(IrcPluginConfiguration));
            TextWriter textWriter = new StreamWriter(ConfigurationFilename);
            serializer.Serialize(textWriter, config);
            textWriter.Close();
        }


    }
}
