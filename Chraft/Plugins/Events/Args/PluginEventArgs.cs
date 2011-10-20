using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Commands;

namespace Chraft.Plugins.Events.Args
{
    /// <summary>
    /// The base EventArgs for a PluginEvent.
    /// </summary>
    public class PluginEventArgs : ChraftEventArgs
    {
        public virtual IPlugin Plugin { get; protected set; }

        public PluginEventArgs(IPlugin p)
        {
            Plugin = p;
            EventCanceled = false;
        }
        public override string ToString()
        {
            return Plugin.Name;
        }
    }
    /// <summary>
    /// The Base EventArgs for a PluginEvent for commands.
    /// </summary>
    public class PluginCommandEventArgs : PluginEventArgs
    {
        public virtual ICommand Command { get; protected set; }

        public PluginCommandEventArgs(IPlugin p, ICommand c)
            : base(p)
        {
            Command = c;
        }
    }

    /// <summary>
    /// EventArgs for when a plugin is enabled.
    /// </summary>
    public class PluginDisabledEventArgs : PluginEventArgs
    {
        public PluginDisabledEventArgs(IPlugin p) : base(p) { }        
    }
    /// <summary>
    /// EventArgs for when a  plugin is disabled.
    /// </summary>
    public class PluginEnabledEventArgs : PluginEventArgs
    {
        public PluginEnabledEventArgs(IPlugin p) : base(p) { }
    }
    /// <summary>
    /// EventArgs for when a command is added.
    /// </summary>
    public class CommandAddedEventArgs : PluginCommandEventArgs
    {

        public CommandAddedEventArgs(IPlugin p, ICommand c) : base(p, c) { }
    }
    /// <summary>
    /// EventArgs for when a command is removed.
    /// </summary>
    public class CommandRemovedEventArgs : PluginCommandEventArgs
    {
        public CommandRemovedEventArgs(IPlugin p, ICommand c) : base(p, c) { }
    }
}
