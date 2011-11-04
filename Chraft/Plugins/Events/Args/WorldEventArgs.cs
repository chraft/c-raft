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
using Chraft.Net;
using Chraft.World;

namespace Chraft.Plugins.Events.Args
{
    public class WorldEventArgs : ChraftEventArgs
    {
        public virtual WorldManager World { get; private set; }

        public WorldEventArgs(WorldManager World)
        {
            this.World = World;
            this.EventCanceled = false;
        }
    }
    public class PlayerWorldEventArgs : WorldEventArgs
    {
        public virtual Client Client { get; set; }
        public PlayerWorldEventArgs(Client Client, WorldManager World)
            : base(World)
        {
            this.Client = Client;
        }
    }
    public class WorldLoadEventArgs : WorldEventArgs
    {
        public WorldLoadEventArgs(WorldManager World) : base(World) { }
    }
    public class WorldUnloadEventArgs : WorldEventArgs
    {
        public WorldUnloadEventArgs(WorldManager World) : base(World) { }
    }
    public class WorldCreatedEventArgs : WorldEventArgs
    {
        public WorldCreatedEventArgs(WorldManager World) : base(World) { }
    }
    public class WorldDeletedEventArgs : WorldEventArgs
    {
        public WorldDeletedEventArgs(WorldManager World) : base(World) { }
    }
    public class WorldJoinedEventArgs : PlayerWorldEventArgs
    {
        public override bool EventCanceled
        {
            get { return base.EventCanceled; }
            set { }
        }
        public WorldJoinedEventArgs(Client Client, WorldManager World) : base(Client, World) { }
    }
    public class WorldLeftEventArgs : PlayerWorldEventArgs
    {
        public WorldLeftEventArgs(Client Clent, WorldManager World) : base(Clent, World) { }
    }
}
