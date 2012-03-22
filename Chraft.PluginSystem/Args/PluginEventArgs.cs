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

using Chraft.PluginSystem.Commands;

namespace Chraft.PluginSystem.Args
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
