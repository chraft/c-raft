using System;
using Chraft.Utils;

namespace Chraft.World
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
        
        public BoundingBox(Vector3 minimum, Vector3 maximum)
        {
            _minimum = Vector3.Origin;
            _maximum = Vector3.Origin;
            this.Minimum = minimum;
            this.Maximum = maximum;
        }
        
        public BoundingBox(double minX, double minY, double minZ, double maxX, double maxY, double maxZ)
            : this(new Vector3(minX, minY, minZ), new Vector3(maxX, maxY, maxZ))
        {
        
        }
        
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
            if (potentialCollisions == null || potentialCollisions.Length == 0)
                return this + motion;
            
            BoundingBox offsetBB = this;
            
            // If there is Y movement, calculate new Y offset for collisions
            // Y is calculated first to adjust for jumping height/falling before applying restrictions on X and Z
            if (Math.Abs(motion.Y) >= double.Epsilon)
            {
                foreach (var collision in potentialCollisions)
                {
                    // If the collision is on the same X or Z axis, then adjust the Y movement
                    if (collision.IsVectorWithinXZ(offsetBB.Minimum) || collision.IsVectorWithinXZ(offsetBB.Maximum))
                    {
                        if (motion.Y > 0.0d && collision.Maximum.Y <= offsetBB.Minimum.Y)
                        {
                            motion.Y = Math.Min(motion.Y, offsetBB.Minimum.Y - collision.Maximum.Y);
                        }
                        else if (motion.Y < 0.0d && collision.Minimum.Y >= offsetBB.Maximum.Y)
                        {
                            motion.Y = Math.Max(motion.Y, offsetBB.Maximum.Y - collision.Minimum.Y);
                        }
                    }
                }
                
                offsetBB = offsetBB + new Vector3(0, motion.Y, 0);
            }
            
            // If there is an X movement, calculate new X offset for collisions
            if (Math.Abs(motion.X) >= double.Epsilon)
            {
                foreach (var collision in potentialCollisions)
                {
                    // If the collision is on the same Y or Z axis, then adjust the X movement
                    if (collision.IsVectorWithinYZ(offsetBB.Minimum) || collision.IsVectorWithinYZ(offsetBB.Maximum))
                    {
                        if (motion.X > 0.0d && collision.Maximum.X <= offsetBB.Minimum.X)
                        {
                            motion.X = Math.Min(motion.X, offsetBB.Minimum.X - collision.Maximum.X);
                        }
                        else if (motion.X < 0.0d && collision.Minimum.X >= offsetBB.Maximum.X)
                        {
                            motion.X = Math.Max(motion.X, offsetBB.Maximum.X - collision.Minimum.X);
                        }
                    }
                }
                
                offsetBB = offsetBB + new Vector3(motion.X, 0, 0);
            }
            
            // If there is any Z movement, calculate new Z offset based on any collisions
            if (Math.Abs(motion.Z) >= double.Epsilon)
            {
                foreach (var collision in potentialCollisions)
                {
                    // If the collision is on the same Y or Z axis, then adjust the X movement
                    if (collision.IsVectorWithinXY(offsetBB.Minimum) || collision.IsVectorWithinXY(offsetBB.Maximum))
                    {
                        if (motion.Z > 0.0d && collision.Maximum.Z <= offsetBB.Minimum.Z)
                        {
                            motion.Z = Math.Min(motion.Z, offsetBB.Minimum.Z - collision.Maximum.Z);
                        }
                        else if (motion.Z < 0.0d && collision.Minimum.Z >= offsetBB.Maximum.Z)
                        {
                            motion.Z = Math.Max(motion.Z, offsetBB.Maximum.Z - collision.Minimum.Z);
                        }
                    }
                }
                
                offsetBB = offsetBB + new Vector3(motion.X, 0, 0);
            }
            
            return offsetBB;
        }
        
        /// <summary>
        /// Determines whether this instance hsa the specified vector within its bounds.
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
        
        public override string ToString()
        {
            return String.Format("boundingbox[{0} -> {1}]", Minimum.ToString(), Maximum.ToString());
        }
    }
}

