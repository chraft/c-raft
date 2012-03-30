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

using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.World;

namespace Chraft.PluginSystem.Args
{
    public class WorldEventArgs : ChraftEventArgs
    {
        public virtual IWorldManager World { get; private set; }

        public WorldEventArgs(IWorldManager World)
        {
            this.World = World;
            this.EventCanceled = false;
        }
    }
    public class PlayerWorldEventArgs : WorldEventArgs
    {
        public virtual IClient Client { get; set; }
        public PlayerWorldEventArgs(IClient Client, IWorldManager World)
            : base(World)
        {
            this.Client = Client;
        }
    }
    public class WorldLoadEventArgs : WorldEventArgs
    {
        public WorldLoadEventArgs(IWorldManager World) : base(World) { }
    }
    public class WorldUnloadEventArgs : WorldEventArgs
    {
        public WorldUnloadEventArgs(IWorldManager World) : base(World) { }
    }
    public class WorldCreatedEventArgs : WorldEventArgs
    {
        public WorldCreatedEventArgs(IWorldManager World) : base(World) { }
    }
    public class WorldDeletedEventArgs : WorldEventArgs
    {
        public WorldDeletedEventArgs(IWorldManager World) : base(World) { }
    }
    public class WorldJoinedEventArgs : PlayerWorldEventArgs
    {
        public override bool EventCanceled
        {
            get { return base.EventCanceled; }
            set { }
        }
        public WorldJoinedEventArgs(IClient Client, IWorldManager World) : base(Client, World) { }
    }
    public class WorldLeftEventArgs : PlayerWorldEventArgs
    {
        public WorldLeftEventArgs(IClient Clent, IWorldManager World) : base(Clent, World) { }
    }
}
