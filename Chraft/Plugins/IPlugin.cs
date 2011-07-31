using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events;

namespace Chraft.Plugins
{
	public interface IPlugin
	{
		/// <summary>
		/// The name of the plugin.
		/// </summary>
		string Name { get; }

		/// <summary>
		/// The author(s) of the plugin.
		/// </summary>
		string Author { get; }

		/// <summary>
		/// A description of the plugin.
		/// </summary>
		string Description { get; }

		/// <summary>
		/// A website for information regarding the plugin.
		/// </summary>
		string Website { get; }

		/// <summary>
		/// The version of the plugin.
		/// </summary>
		Version Version { get; }

		/// <summary>
		/// The Server associated with the plugin.
		/// </summary>
		Server Server { get; }

        /// <summary>
        /// The PluginManager associated with the plugin.
        /// </summary>
        PluginManager PluginManager { get; }

		/// <summary>
		/// A value indicating whether the plugin is currently enabled.
		/// </summary>
		bool IsPluginEnabled { get; }

		/// <summary>
		/// Called after all default plugins are loaded, at which point it is safe to assume that any dependencies are loaded.
		/// </summary>
		void Initialize();

        /// <summary>
        /// Associates a Server and a PluginManager with the plugin.
        /// </summary>
        /// <param name="server">The Server object to be associated with the plugin.</param>
        /// <param name="pluginManager">The PluginManager to be associated with the plugin.</param>
        void Associate(Server server, PluginManager pluginManager);

		/// <summary>
		/// Called when the parent PluginManager enables the plugin.
		/// </summary>
		void OnEnabled();

		/// <summary>
		/// Called when the parent PluginManager disables the plugin.
		/// </summary>
		void OnDisabled();
	}
}
