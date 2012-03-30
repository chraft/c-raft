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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.PluginSystem.Server;

namespace Chraft.PluginSystem
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
		IServer Server { get; }

        /// <summary>
        /// The PluginManager associated with the plugin.
        /// </summary>
        IPluginManager PluginManager { get; }

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
        void Associate(IServer server, IPluginManager pluginManager);

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
