using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;
using Chraft.Utils;

namespace Chraft.World
{
    /// <summary>
    /// A class to represent the Location of an Entity within the world.
    /// </summary>
    public class Location : ICloneable, IComparable<Location>, IEquatable<Vector3>, IEquatable<Location>
    {
        public double X { get { return this.Vector.X; } set { this.Vector.X = value; } }
        public double Y { get { return this.Vector.Y; } set { this.Vector.Y = value; } }
        public double Z { get { return this.Vector.Z; } set { this.Vector.Z = value; } }

        /// <summary>
        /// Rotation around the X-axis
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// Rotation around the Y-axis.
        /// </summary>
        public double Yaw { get; set; }

        public sbyte PackedPitch { get { return (sbyte)(this.Pitch / 360.0 * 256.0 % 256.0); } }
        public sbyte PackedYaw { get { return (sbyte)(this.Yaw / 360.0 * 256.0 % 256.0); } }

        public Vector3 Vector;

        public int ChunkX { get { return (int)this.X >> 4; } }
        public int ChunkZ { get { return (int)this.Z >> 4; } }

        public int BlockX { get { return (int)Math.Floor(this.X); } }
        public int BlockY { get { return (int)Math.Floor(this.Y); } }
        public int BlockZ { get { return (int)Math.Floor(this.Z); } }

        public Location(double x, double y, double z)
            : this(x, y, z, 0, 0)
        {
        }

        public Location(double x, double y, double z, double pitch, double yaw)
            : this(new Vector3(x, y, z), pitch, yaw)
        {
        }

        public Location(Vector3 vector)
            : this(vector, 0, 0)
        {
        }

        public Location(Vector3 vector, double pitch, double yaw)
        {
            this.Vector = vector;
            this.Pitch = pitch;
            this.Yaw = yaw;
        }

        /// <summary>
        /// Returns a unit vector in the direction (Pitch,Yaw) this location is facing
        /// </summary>
        /// <returns></returns>
        public Vector3 FacingDirection()
        {
            return new Vector3(this.Yaw, this.Pitch);
        }

        public object Clone()
        {
            return new Location(this.Vector, this.Pitch, this.Yaw);
        }

        public int CompareTo(Location other)
        {
            return this.Vector.CompareTo(other);
        }

        public bool Equals(Vector3 other)
        {
            return this.Vector == other;
        }

        public bool Equals(Location other)
        {
            return this.Vector == other.Vector && this.Pitch == other.Pitch && this.Yaw == other.Yaw;
        }

    }
}
