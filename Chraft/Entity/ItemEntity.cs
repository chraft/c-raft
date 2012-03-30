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
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;
using Chraft.World.Blocks;

namespace Chraft.Entity
{
	public class ItemEntity : EntityBase
	{
		public short ItemId { get; set; }
		public sbyte Count { get; set; }
		public short Durability { get; set; }
              
		public ItemEntity(Server server, int entityId)
			: base(server, entityId)
		{              
            this.Height = 0.25f;
            this.Width = 0.25f;
		}
  
        protected override void DoUpdate()
        {
            base.DoUpdate();
            
            Velocity.Y -= 0.04;
            
            PushOutOfBlocks(new AbsWorldCoords(this.Position.X, (this.BoundingBox.Minimum.Y + this.BoundingBox.Maximum.Y) / 2.0, this.Position.Z));
            
            this.MoveTo(ApplyVelocity(this.Velocity));
            
            double friction = 0.98;
            if (OnGround)
            {
                friction = 0.58;
                
                // TODO: adjust based on Block friction
            }
            Velocity.X *= friction;
            Velocity.Y *= 0.98;
            Velocity.Z *= friction;
            
            if (OnGround)
                Velocity.Y *= -0.5;
        }   
	}
}
