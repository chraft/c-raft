using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Reflection;

namespace Chraft.Plugins
{
	public class PluginManager
	{
		private List<IPlugin> Plugins = new List<IPlugin>();

		/// <summary>
		/// The folder searched at runtime for available plugins.
		/// </summary>
		public string Folder { get; private set; }

		/// <summary>
		/// Invoked when a plugin is loaded.
		/// </summary>
		public event EventHandler<PluginEventArgs> PluginLoaded;

		/// <summary>
		/// Initializes a new PluginManager with the given plugin folder.
		/// </summary>
		/// <param name="folder">The folder to be used by LoadDefaultAssemblies.</param>
		internal PluginManager(string folder)
		{
			Folder = folder;
		}

		/// <summary>
		/// Gets a thread-safe array of loaded plugins.  Note that while this array is safe for enumeration, it
		/// does not necessarily ensure the thread-safety of the underlying plugins.
		/// </summary>
		/// <returns>A shallow-thread-safe array of plugins.</returns>
		public IPlugin[] GetPlugins()
		{
			return Plugins.ToArray();
		}

		/// <summary>
		/// Loads all the assemblies in the plugin folder and the current assembly.
		/// </summary>
		internal void LoadDefaultAssemblies()
		{
			LoadAssembly(Assembly.GetExecutingAssembly());
			if (Directory.Exists(Folder))
			{
				foreach (string f in Directory.EnumerateFiles(Folder, "*.dll"))
					LoadAssembly(f);
			}
		}

		/// <summary>
		/// Load all plugins in the given file.
		/// </summary>
		/// <param name="file">The path to the assembly containing the plugin(s).</param>
		public void LoadAssembly(string file)
		{
			LoadAssembly(Assembly.LoadFile(file));
		}

		/// <summary>
		/// Load all plugins in the given assembly.
		/// </summary>
		/// <param name="asm">The assembly containing the plugin(s).</param>
		public void LoadAssembly(Assembly asm)
		{
			foreach (Type t in from t in asm.GetTypes()
							   where t.GetInterfaces().Contains(typeof(IPlugin))
							   && t.GetCustomAttributes(typeof(PluginAttribute), false).Length > 0
							   select t)
				LoadPlugin(Ctor(t));
		}

		/// <summary>
		/// Loads a plugin into the PluginManager so that it can be managed by the server.
		/// </summary>
		/// <param name="plugin">The plugin to be loaded.</param>
		public void LoadPlugin(IPlugin plugin)
		{
			lock (Plugins)
				Plugins.Add(plugin);
			OnPluginLoaded(plugin);
		}

		private void OnPluginLoaded(IPlugin plugin)
		{
			if (PluginLoaded != null)
				PluginLoaded.Invoke(this, new PluginEventArgs(plugin));
		}

		/// <summary>
		/// Invokes a plugin's ".ctor" method and returns the resulting IPlugin.
		/// </summary>
		/// <param name="t">The IPlugin type to be constructed.</param>
		/// <returns>A new plugin from the type's constructor.</returns>
		public IPlugin Ctor(Type t)
		{
			return (IPlugin)t.GetConstructor(Type.EmptyTypes).Invoke(null);
		}
	}
}
