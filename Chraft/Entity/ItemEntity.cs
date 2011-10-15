using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            this.Health = 5;
               
            this.Height = 0.25f;
            this.Width = 0.25f;
		}
  
        protected override void DoUpdate()
        {
            base.DoUpdate();
            
            Velocity.Y -= 0.04;
            
            PushOutOfBlocks(new AbsWorldCoords(this.Position.X, (this.BoundingBox.Minimum.Y + this.BoundingBox.Maximum.Y) / 2.0, this.Position.Z));
            
            ApplyVelocity(this.Velocity);
            
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
