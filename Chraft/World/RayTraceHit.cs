using System;
using Chraft.Utils;
using Chraft.Entity;

namespace Chraft.World
{
    /// <summary>
    /// Represents a Ray Trace hit / intersection
    /// </summary>
    public class RayTraceHit
    {
        public Vector3 Hit { get; protected set; }
        public BlockFace FaceHit { get; protected set; }
        
        public RayTraceHit(Vector3 hit, BlockFace faceHit)
        {
            Hit = hit;
            FaceHit = faceHit;
        }
    }
    
    /// <summary>
    /// Ray trace hit block.
    /// </summary>
    public class RayTraceHitBlock : RayTraceHit
    {
        /// <summary>
        /// Gets or sets the target block coordinate of the ray trace.
        /// </summary>
        /// <value>
        /// The target block coordinate.
        /// </value>
        public UniversalCoords TargetBlock { get; protected set; }
        
        public RayTraceHitBlock(UniversalCoords targetBlock, BlockFace faceHit, Vector3 hitVector)
            : base(hitVector, faceHit)
        {
            this.TargetBlock = targetBlock;
            this.FaceHit = faceHit;
        }
        
        public override string ToString()
        {
            return string.Format("[RayTraceHitBlock: TargetBlock={0}, Face={1}]", TargetBlock, FaceHit, Hit);
        }
    }
    
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

