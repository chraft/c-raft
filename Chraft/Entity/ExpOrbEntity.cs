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

using Chraft.Utilities.Coords;
using Chraft.World;

namespace Chraft.Entity
{
    public class ExpOrbEntity : EntityBase
    {
        public short Experience { get; protected set; }
        public short Age { get; protected set; }

		public ExpOrbEntity(Server server, int entityId, short exp)
			: base(server, entityId)
		{              
            Height = 0.5f;
            Width = 0.5f;
		    Experience = exp;
		    World = server.GetDefaultWorld() as WorldManager;
		}

        protected override void DoUpdate()
        {
            base.DoUpdate();

            Age++;
            if (Age >= 6000)
            {
                Server.RemoveEntity(this);
                return;
            }
        }
    }
}
