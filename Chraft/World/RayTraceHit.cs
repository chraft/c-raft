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

