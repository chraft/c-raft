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
            
            // Produce a random Velocity for new item
//            Velocity.X = (float)(Server.Rand.NextDouble() * 0.2 - 0.1);
//            Velocity.Y = 0.2;
//            Velocity.Z = (float)(Server.Rand.NextDouble() * 0.2 - 0.1);   
		}
  
        protected override void DoUpdate()
        {
            base.DoUpdate();
            
            Velocity.Y -= 0.04;
            
            // TODO: push item out of blocks
            
            ApplyVelocity(this.Velocity);
            
            Velocity.Y *= 0.98;
        }   
	}
}
