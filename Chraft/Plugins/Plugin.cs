using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Plugins
{
	public abstract class Plugin : IPlugin
	{
		/// <summary>
		/// Invoked when the parent PluginManager enables the plugin.
		/// </summary>
		public event EventHandler Enabled;

		/// <summary>
		/// Invoked when the parent PluginManager disables the plugin.
		/// </summary>
		public event EventHandler Disabled;

		/// <summary>
		/// The name of the plugin.
		/// </summary>
		public abstract string Name { get; }

		/// <summary>
		/// The author(s) of the plugin.
		/// </summary>
		public abstract string Author { get; }

		/// <summary>
		/// A descriptive description (:P) of the plugin.
		/// </summary>
		public abstract string Description { get; }

		/// <summary>
		/// A website with more information about the plugin.
		/// </summary>
		public abstract string Website { get; }

		/// <summary>
		/// The installed version of the plugin.
		/// </summary>
		public abstract Version Version { get; }

		/// <summary>
		/// The Server associated with the plugin.
		/// </summary>
		public Server Server { get; private set; }

		/// <summary>
		/// Indicates whether the plugin is currently loaded.
		/// </summary>
		public bool IsPluginEnabled { get; private set; }

        /// <summary>
        /// The PluginManager associated with the plugin. 
        /// </summary>
        public PluginManager PluginManager { get; private set; }

		/// <summary>
		/// Instantiate a new plugin via .ctor
		/// </summary>
		public Plugin()
		{
		}

		/// <summary>
		/// Initializes the plugin.
		/// </summary>
		public virtual void Initialize()
		{
		}

		/// <summary>
		/// Associates a Server and a PluginManager with the plugin.
		/// </summary>
		/// <param name="server">The Server object to be associated with the plugin.</param>
        /// <param name="pluginManager">The PluginManager to be associated with the plugin.</param>
		public void Associate(Server server, PluginManager pluginManager)
		{
			Server = server;
            PluginManager = pluginManager;
		}

		/// <summary>
		/// Invokes the Enabled event.
		/// </summary>
		public virtual void OnEnabled()
		{
			IsPluginEnabled = true;
			if (Enabled != null)
				Enabled.Invoke(this, new EventArgs());
		}

		/// <summary>
		/// Invokes the Disabled event
		/// </summary>
		public virtual void OnDisabled()
		{
			IsPluginEnabled = false;
			if (Disabled != null)
				Disabled.Invoke(this, new EventArgs());
		}
	}
}
