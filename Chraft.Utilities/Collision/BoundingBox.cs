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
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;

namespace Chraft.Utilities.Collision
{
    /// <summary>
    /// Used to represent the bounds of objects within the World for the purposes of collision detection (e.g. items, mobs, blocks).
    /// </summary>
    public struct BoundingBox
    {
        Vector3 _minimum;
        Vector3 _maximum;
        public Vector3 Minimum
        {
            get { return _minimum; }
            set { _minimum = value; } 
        }

        public Vector3 Maximum
        {
            get { return _maximum; }
            set { _maximum = value; }
        }
        
        public BoundingBox(AbsWorldCoords minimum, AbsWorldCoords maximum) : 
            this(minimum.ToVector(), maximum.ToVector())
        {
            
        }
        
        public BoundingBox(Vector3 minimum, Vector3 maximum)
        {
            _minimum = Vector3.Origin;
            _maximum = Vector3.Origin;
           // System.Diagnostics.Debug.Assert(minimum <= maximum, "Minimum must be less than or equal to Maximum");
            this.Minimum = minimum;
            this.Maximum = maximum;
        }
        
        public BoundingBox(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
            : this(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ))
        {
        
        }
        
        #region Operators
        
        /// <summary>
        /// Adds a <see cref="Vector3"/> to a <see cref="BoundingBox"/>, yielding a new <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name='boundingBox'>
        /// The <see cref="BoundingBox"/> to offset.
        /// </param>
        /// <param name='vector'>
        /// The <see cref="Vector3"/> to add.
        /// </param>
        /// <returns>
        /// The <see cref="BoundingBox"/> that has its Minimum and Maximum vectors added to <paramref name="vector"/>.
        /// </returns>
        public static BoundingBox operator +(BoundingBox boundingBox, Vector3 vector)
        {
            return new BoundingBox(
                boundingBox.Minimum + vector,
                boundingBox.Maximum + vector
            );
        }
        
        /// <summary>
        /// Subtracts a <see cref="Vector3"/> from a <see cref="BoundingBox"/>, yielding a new <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name='boundingBox'>
        /// The <see cref="BoundingBox"/> to subtract from (the minuend).
        /// </param>
        /// <param name='vector'>
        /// The <see cref="Vector3"/> to subtract (the subtrahend).
        /// </param>
        /// <returns>
        /// The <see cref="BoundingBox"/> that has its Minimum and Maximum vectors subtracted by <paramref name="vector"/>.
        /// </returns>
        public static BoundingBox operator -(BoundingBox boundingBox, Vector3 vector)
        {
            return new BoundingBox(
                boundingBox.Minimum - vector,
                boundingBox.Maximum - vector
            );
        }
        
        /// <summary>
        /// Determines whether a specified instance of <see cref="BoundingBox"/> is equal to another specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name='boundingBox1'>
        /// The first <see cref="BoundingBox"/> to compare.
        /// </param>
        /// <param name='boundingBox2'>
        /// The second <see cref="BoundingBox"/> to compare.
        /// </param>
        /// <returns>
        /// <c>true</c> if <c>boundingBox1</c> and <c>boundingBox2</c> are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(BoundingBox boundingBox1, BoundingBox boundingBox2)
        {
            return boundingBox1.Minimum == boundingBox1.Minimum && boundingBox1.Maximum == boundingBox2.Maximum;
        }
        
        /// <summary>
        /// Determines whether a specified instance of <see cref="BoundingBox"/> is not equal to another specified <see cref="BoundingBox"/>.
        /// </summary>
        /// <param name='boundingBox1'>
        /// The first <see cref="BoundingBox"/> to compare.
        /// </param>
        /// <param name='boundingBox2'>
        /// The second <see cref="BoundingBox"/> to compare.
        /// </param>
        /// <returns>
        /// <c>true</c> if <c>boundingBox1</c> and <c>boundingBox2</c> are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(BoundingBox boundingBox1, BoundingBox boundingBox2)
        {
            return !(boundingBox1 == boundingBox2);
        }
        
        #endregion
        
        #region Resizing
        
        /// <summary>
        /// Expand this instance's Minimum and Maximum vectors by the specified expandMinAndMaxBy vector.
        /// </summary>
        /// <param name='expandMinAndMaxBy'>
        /// Expand minimum and max by.
        /// </param>
        /// <returns>The <see cref="BoundingBox"/> that has been expanded</returns>
        public BoundingBox Expand(Vector3 expandMinAndMaxBy)
        {
            return new BoundingBox(
                this.Minimum - expandMinAndMaxBy,
                this.Maximum + expandMinAndMaxBy
            );
        }
        
        /// <summary>
        /// Contract this instance's Minimum and Maximum vectors by the specified contractMinAndMaxBy.
        /// </summary>
        /// <param name='contractMinAndMaxBy'>
        /// Contract minimum and max by.
        /// </param>
        /// <returns>The <see cref="BoundingBox"/> that has been contracted</returns>
        public BoundingBox Contract(Vector3 contractMinAndMaxBy)
        {
            return new BoundingBox(
                this.Minimum + contractMinAndMaxBy,
                this.Maximum - contractMinAndMaxBy
            );
        }
        
        #endregion
        
        #region Movement
        
        /// <summary>
        /// Offset this instance by the specified offsetVector. See also the +/- operators
        /// </summary>
        /// <param name='offsetVector'>
        /// Offset vector.
        /// </param>
        public BoundingBox Offset(Vector3 offsetVector)
        {
            return this + offsetVector;
        }
        
        /// <summary>
        /// Offsets the BoundingBox with motion, with clipping based on potential collisions.
        /// </summary>
        /// <returns>
        /// The BoundingBox in the new location after clipping, and set motion to the actual offset used after clipping is applied.
        /// </returns>
        /// <param name='motion'>
        /// Motion to be applied. The value will be changed to reflect the actual offset used after clipping is applied.
        /// </param>
        /// <param name='potentialCollisions'>
        /// Potential collisions.
        /// </param>
        public BoundingBox OffsetWithClipping(ref Vector3 motion, BoundingBox[] potentialCollisions)
        {
            System.Diagnostics.Debug.Assert(motion.X < 1.0 && motion.Y < 1.0 && motion.Z < 1.0, "OffsetWithClipping: motion X/Y/Z must be less than 1.0");
            
            if (potentialCollisions == null || potentialCollisions.Length == 0)
                return this + motion;
            
            BoundingBox offsetBB = this;
            
            // If there is Y movement, calculate new Y offset for collisions
            // Y is calculated first to adjust for jumping height/falling before applying restrictions on X and Z
            if (System.Math.Abs(motion.Y) >= double.Epsilon)
            {
                foreach (var collision in potentialCollisions)
                {
                    // If the collision is on the same X or Z axis, then adjust the Y movement
                    // Check against the target boundingbox, but adjust against the current boundingbox
                    if (collision.IsVectorWithinXZ(offsetBB.Minimum) || collision.IsVectorWithinXZ(offsetBB.Maximum))
                    {
                        if (motion.Y > 0.0d && offsetBB.Maximum.Y <= collision.Minimum.Y)
                        {
                            motion.Y = System.Math.Min(motion.Y, collision.Minimum.Y - offsetBB.Maximum.Y);
                        }
                        else if (motion.Y < 0.0d && offsetBB.Minimum.Y >= collision.Maximum.Y)
                        {
                            motion.Y = System.Math.Max(motion.Y, collision.Maximum.Y - offsetBB.Minimum.Y);
                        }
                    }
                }
                
                offsetBB = offsetBB + new Vector3(0, motion.Y, 0);
            }
            
            // If there is an X movement, calculate new X offset for collisions
            if (System.Math.Abs(motion.X) >= double.Epsilon)
            {
                foreach (var collision in potentialCollisions)
                {
                    // If the collision is on the same Y or Z axis, then adjust the X movement
                    // Check against the target boundingbox, but adjust against the current boundingbox
                    if (collision.IsVectorWithinYZ(offsetBB.Minimum) || collision.IsVectorWithinYZ(offsetBB.Maximum))
                    {
                        if (motion.X > 0.0d && offsetBB.Maximum.X <= collision.Minimum.X)
                        {
                            motion.X = System.Math.Min(motion.X, collision.Minimum.X - offsetBB.Maximum.X);
                        }
                        else if (motion.X < 0.0d && offsetBB.Minimum.X >= collision.Maximum.X)
                        {
                            motion.X = System.Math.Max(motion.X, collision.Maximum.X - offsetBB.Minimum.X);
                        }
                    }
                }
                
                offsetBB = offsetBB + new Vector3(motion.X, 0, 0);
            }
            
            // If there is any Z movement, calculate new Z offset based on any collisions
            if (System.Math.Abs(motion.Z) >= double.Epsilon)
            {
                foreach (var collision in potentialCollisions)
                {
                    // If the collision is on the same Y or Z axis, then adjust the X movement
                    // Check against the target boundingbox, but adjust against the current boundingbox
                    if (collision.IsVectorWithinXY(offsetBB.Minimum) || collision.IsVectorWithinXY(offsetBB.Maximum))
                    {
                        if (motion.Z > 0.0d && offsetBB.Maximum.Z <= collision.Minimum.Z)
                        {
                            motion.Z = System.Math.Min(motion.Z, collision.Minimum.Z - offsetBB.Maximum.Z);
                        }
                        else if (motion.Z < 0.0d && offsetBB.Minimum.Z >= collision.Maximum.Z)
                        {
                            motion.Z = System.Math.Max(motion.Z, collision.Maximum.Z - offsetBB.Minimum.Z);
                        }
                    }
                }
                
                offsetBB = offsetBB + new Vector3(0, 0, motion.Z);
            }
            
            return offsetBB;
        }
        
        #endregion
        
        #region Intersection
        
        /// <summary>
        /// Determines whether this instance has the specified vector within its bounds.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance is vector within bounds the specified vector; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='vector'>
        /// If set to <c>true</c> vector.
        /// </param>
        public bool IsVectorWithinBounds(Vector3 vector)
        {
            return vector.X > this.Minimum.X && vector.X < this.Maximum.X &&
                   vector.Y > this.Minimum.Y && vector.Y < this.Maximum.Y &&
                   vector.Z > this.Minimum.Z && vector.Z < this.Maximum.Z;
        }
        
        /// <summary>
        /// Determines whether this instance contains the specified vector within the X/Y axis.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance contains the specified vector within the X/Y axis; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='vector'>
        /// The vector to check
        /// </param>
        public bool IsVectorWithinXY(Vector3 vector)
        {
            return vector.X > Minimum.X && vector.X < Maximum.X && vector.Y > Minimum.Y && vector.Y < Maximum.Y;
        }
        
        /// <summary>
        /// Determines whether this instance contains the specified vector within the X/Z axis.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance contains the specified vector within the X/Z axis; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='vector'>
        /// The vector to check
        /// </param>
        public bool IsVectorWithinXZ(Vector3 vector)
        {
            return vector.X > Minimum.X && vector.X < Maximum.X && vector.Z > Minimum.Z && vector.Z < Maximum.Z;
        }
        
        /// <summary>
        /// Determines whether this instance contains the specified vector within the Y/Z axis.
        /// </summary>
        /// <returns>
        /// <c>true</c> if this instance contains the specified vector within the Y/Z axis; otherwise, <c>false</c>.
        /// </returns>
        /// <param name='vector'>
        /// The vector to check
        /// </param>
        public bool IsVectorWithinYZ(Vector3 vector)
        {
            return vector.Y > Minimum.Y && vector.Y < Maximum.Y && vector.Z > Minimum.Z && vector.Z < Maximum.Z;
        }
        
        /// <summary>
        /// Determines if this intersects with <paramref name="boundingBox"/>.
        /// </summary>
        /// <returns>
        /// If true the bounding boxes intersect
        /// </returns>
        /// <param name='boundingBox'>
        /// The bounding box to check for an intersection with
        /// </param>
        public bool IntersectsWith(BoundingBox boundingBox)
        {
            return boundingBox.Maximum.X > Minimum.X && boundingBox.Minimum.X < Maximum.X &&
                   boundingBox.Maximum.Y > Minimum.Y && boundingBox.Minimum.Y < Maximum.Y &&
                   boundingBox.Maximum.Z > Minimum.Z && boundingBox.Minimum.Z < Maximum.Z;
        }
        
        public RayTraceHit RayTraceIntersection(Vector3 startSegment, Vector3 endSegment)
        {
            // Plane intersection
            
            Vector3? southIntersection = Vector3.IntersectPointForSegmentAndPlane(startSegment, endSegment, this.Maximum, Vector3.XAxis);
            Vector3? northIntersection = Vector3.IntersectPointForSegmentAndPlane(startSegment, endSegment, this.Minimum, Vector3.XAxis);
            Vector3? topIntersection = Vector3.IntersectPointForSegmentAndPlane(startSegment, endSegment, this.Maximum, Vector3.YAxis);
            Vector3? bottomIntersection = Vector3.IntersectPointForSegmentAndPlane(startSegment, endSegment, this.Minimum, Vector3.YAxis);
            Vector3? westIntersection = Vector3.IntersectPointForSegmentAndPlane(startSegment, endSegment, this.Maximum, Vector3.ZAxis);
            Vector3? eastIntersection = Vector3.IntersectPointForSegmentAndPlane(startSegment, endSegment, this.Minimum, Vector3.ZAxis);
            
            // Check each intersection to be sure it lies within the other axis as well
            if (northIntersection.HasValue && !this.IsVectorWithinYZ(northIntersection.Value))
            {
                northIntersection = null;
            }
            if (southIntersection.HasValue && !this.IsVectorWithinYZ(southIntersection.Value))
            {
                southIntersection = null;
            }
            if (topIntersection.HasValue && !this.IsVectorWithinXZ(topIntersection.Value))
            {
                topIntersection = null;
            }
            if (bottomIntersection.HasValue && !this.IsVectorWithinXZ(bottomIntersection.Value))
            {
                bottomIntersection = null;
            }
            if (westIntersection.HasValue && !this.IsVectorWithinXY(westIntersection.Value))
            {
                westIntersection = null;
            }
            if (eastIntersection.HasValue && !this.IsVectorWithinXY(eastIntersection.Value))
            {
                eastIntersection = null;
            }
            
            Vector3? rayHitPoint = null;
            BlockFace faceHit = BlockFace.Self;
            if (northIntersection != null)
            {
                rayHitPoint = northIntersection;
                faceHit = BlockFace.North;
            }
            if (southIntersection != null && (rayHitPoint == null || startSegment.DistanceSquared(southIntersection.Value) < startSegment.DistanceSquared(rayHitPoint.Value)))
            {
                rayHitPoint = southIntersection;
                faceHit = BlockFace.South;
            }
            if (topIntersection != null && (rayHitPoint == null || startSegment.DistanceSquared(topIntersection.Value) < startSegment.DistanceSquared(rayHitPoint.Value)))
            {
                rayHitPoint = topIntersection;
                faceHit = BlockFace.Up;
            }
            if (bottomIntersection != null && (rayHitPoint == null || startSegment.DistanceSquared(bottomIntersection.Value) < startSegment.DistanceSquared(rayHitPoint.Value)))
            {
                rayHitPoint = bottomIntersection;
                faceHit = BlockFace.Down;
            }
            if (westIntersection != null && (rayHitPoint == null || startSegment.DistanceSquared(westIntersection.Value) < startSegment.DistanceSquared(rayHitPoint.Value)))
            {
                rayHitPoint = westIntersection;
                faceHit = BlockFace.West;
            }
            if (eastIntersection != null && (rayHitPoint == null || startSegment.DistanceSquared(eastIntersection.Value) < startSegment.DistanceSquared(rayHitPoint.Value)))
            {
                rayHitPoint = eastIntersection;
                faceHit = BlockFace.East;
            }
            
            if (rayHitPoint != null)
                return new RayTraceHit(rayHitPoint.Value, faceHit);
            else
                return null;
        }
        
        #endregion
        
        public override string ToString()
        {
            return String.Format("boundingbox[{0} -> {1}]", Minimum.ToString(), Maximum.ToString());
        }

        public bool Equals(BoundingBox other)
        {
            return other._minimum.Equals(_minimum) && other._maximum.Equals(_maximum);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (obj.GetType() != typeof (BoundingBox)) return false;
            return Equals((BoundingBox) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (_minimum.GetHashCode()*397) ^ _maximum.GetHashCode();
            }
        }
    }
}

