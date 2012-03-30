using Chraft.Entity;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Collision;

namespace Chraft.World
{
    /// <summary>
    /// Ray trace hit entity.
    /// </summary>
    public class RayTraceHitEntity : RayTraceHit
    {
        /// <summary>
        /// Gets or sets the entity hit by the ray trace.
        /// </summary>
        /// <value>
        /// The entity hit by the ray trace.
        /// </value>
        public EntityBase Entity { get; protected set; }

        public RayTraceHitEntity(EntityBase entity, BlockFace faceHit)
            : base(entity.Position.ToVector(), faceHit)
        {
            this.Entity = entity;
        }
    }
}
