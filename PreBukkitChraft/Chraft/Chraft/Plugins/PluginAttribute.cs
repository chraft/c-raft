using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Plugins
{
	/// <summary>
	/// Marks a class inheriting IPlugin as a plugin that should be loaded with the assembly.
	/// </summary>
	[AttributeUsage(AttributeTargets.Class)]
	public class PluginAttribute : Attribute
	{
	}
}
