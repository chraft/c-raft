using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Plugins.Events.Args;

namespace Chraft.Plugins
{
	public class PluginEventArgs : ChraftEventArgs
	{
		/// <summary>
		/// The plugin associated with the event.
		/// </summary>
		public IPlugin Plugin { get; private set; }

		internal PluginEventArgs(IPlugin plugin) : base()
		{
			Plugin = plugin;
		}
	}
}
