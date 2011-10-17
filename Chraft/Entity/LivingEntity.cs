using System;
using Chraft.World;

namespace Chraft.Entity
{
    public abstract class LivingEntity : EntityBase
    {
        public override short MaxHealth
        {
            get
            {
                return 20;
            }
        }
        
        public virtual float EyeHeight
        {
            get { return this.Height * 0.85f; }
        }
    
        public LivingEntity(Server server, int entityId)
         : base(server, entityId)
        {
            this.Health = 20;
        }
        
        /// <summary>
        /// Determines whether this instance can see the specified entity.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance can see the specified entity; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='entity'>
        /// The entity to check for line of sight to.
        /// </param>
        public bool CanSee(LivingEntity entity)
        {
            return this.World.RayTraceBlocks(new AbsWorldCoords(this.Position.X, this.Position.Y + this.EyeHeight, this.Position.Z), new AbsWorldCoords(entity.Position.X, entity.Position.Y + entity.EyeHeight, entity.Position.Z)) == null;
        }
    }
}

