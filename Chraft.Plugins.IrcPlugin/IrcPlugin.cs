using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Server;

namespace Chraft.Plugins.IrcPlugin
{
    public class IrcPlugin : IPlugin 
    {
        #region Implementation of IPlugin

        /// <summary>
        /// The name of the plugin.
        /// </summary>
        public string Name
        {
            get { return "IrcPLugin"; }
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
            
            //if (!Directory.Exists(folder)) { Directory.CreateDirectory(folder); }
            //throw new NotImplementedException();
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
            //throw new NotImplementedException();
        }

        /// <summary>
        /// Called when the parent PluginManager disables the plugin.
        /// </summary>
        public void OnDisabled()
        {
            //throw new NotImplementedException();
        }

        #endregion
    }
}
