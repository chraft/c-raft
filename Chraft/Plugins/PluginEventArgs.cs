using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Plugins
{
	public class PluginEventArgs : EventArgs
	{
		/// <summary>
		/// The plugin associated with the event.
		/// </summary>
		public IPlugin Plugin { get; private set; }

		internal PluginEventArgs(IPlugin plugin)
		{
			Plugin = plugin;
		}
	}
}
