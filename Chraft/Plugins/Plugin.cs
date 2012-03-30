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

using System;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Server;

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
		public IServer Server { get; private set; }

		/// <summary>
		/// Indicates whether the plugin is currently loaded.
		/// </summary>
		public bool IsPluginEnabled { get; private set; }

        /// <summary>
        /// The PluginManager associated with the plugin. 
        /// </summary>
        public IPluginManager PluginManager { get; private set; }

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
		public void Associate(IServer server, IPluginManager pluginManager)
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
