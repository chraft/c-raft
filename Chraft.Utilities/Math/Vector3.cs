using System;

namespace Chraft.Utilities.Math 
{
    /// <summary>
    /// Vector of doubles with three components (x,y,z)
    /// 
    /// Adapted from Richard Potter BSc(Hons) http://www.codeproject.com/KB/recipes/VectorType.aspx
    /// 
    /// IMPORTANT: This class is not suitable for use outside of Minecraft due to the Yaw and Pitch being flipped
    /// for Minecraft in regards to the + axis of X and Y respectively. 
    /// See: http://mc.kev009.com/Protocol#Player_Look_.280x0C.29
    /// Also: http://mc.kev009.com/File:Minecraft-trig-yaw.png notice how North (X+) is facing DOWN not UP
    /// 
    /// The methods impacted are: 
    ///     Vector3(double yaw, double pitch)
    ///     Yaw(double yaw)
    ///     SignedAngle(..) uses leftPerpendicular instead of rightPerpendicular
    /// </summary>
    public struct Vector3 : IComparable, IComparable<Vector3>, IEquatable<Vector3>, IFormattable
    {
        private double _x;
        private double _y;
        private double _z;

        // X component of the vector.
        public double X
        {
            get { return _x; }
            set { _x = value; }
        }

        // Y component of the vector.
        public double Y
        {
            get { return _y; }
            set { _y = value; }
        }

        // Z component of the vector.
        public double Z
        {
            get { return _z; }
            set { _z = value; }
        }

        /// <summary>
        /// Property for the magnitude (aka. length or absolute value) of the Vector3 from origin (0,0,0)
        /// </summary>
        public double Magnitude
        {
            get
            {
                return System.Math.Sqrt(SumComponentSqrs());
            }
            set
            {
                if (value < 0)
                { throw new ArgumentOutOfRangeException("value", value, NEGATIVE_MAGNITUDE); }

                if (this == Vector3.Origin)
                { throw new ArgumentException(ORIGIN_VECTOR_MAGNITUDE, "this"); }

                var newV = (this * (value / Magnitude));
                this.X = newV.X;
                this.Y = newV.Y;
                this.Z = newV.Z;
            }
        }

        //
        // Constructors
        //
        public Vector3(double x, double y, double z)
        {
            _x = 0;
            _y = 0;
            _z = 0;

            X = x;
            Y = y;
            Z = z;
        }

        /// <summary>
        /// Constructor for the Vector3 class from another Vector3 object
        /// </summary>
        /// <param name="v1">Vector3 representing the new values for the Vector3</param>
        /// <implementation>
        /// Copies values from Vector3 v1 to this vector, does not hold a reference to object v1 
        /// </implementation>
        public Vector3(Vector3 v1)
        {
            // Initialisation
            _x = 0;
            _y = 0;
            _z = 0;

            X = v1.X;
            Y = v1.Y;
            Z = v1.Z;
        }

        /// <summary>
        /// Constructs a unit vector for the provided yaw (y-axis rotation) and pitch (x-axis rotation). This is useful for generating a "facing" vector.
        /// </summary>
        /// <param name="yaw">The yaw in degrees, 0 points along the Z+ axis (east), positive rotates clockwise through south (90 or -270), west (180 or -180) and then north (270 or -90)</param>
        /// <param name="pitch">The pitch in degrees between -90 (straight up)  and 90 (straight down)</param>
        /// <remarks>
        /// Note: this is modified to work with the non-classical trigonometry rules used in Minecraft (http://mc.kev009.com/Protocol#Player_Look_.280x0C.29)
        /// </remarks>
        public Vector3(double yaw, double pitch)
        {
            _x = 0;
            _y = 0;
            _z = 0;
            
            double yawRadians = yaw.ToRadians();
            double cosPitch = System.Math.Cos(pitch.ToRadians());

            X = -(cosPitch * System.Math.Sin(yawRadians)); // Shorten X down from 1 based on the angle of pitch. We negate because a yaw of -90 or 270 (South) should be +1 not -1 (+yaw is clockwise whereas radians are normally counter-clockwise)
            Y = -System.Math.Sin(pitch.ToRadians());       // Y based on the angle of pitch. We negate because -90 points up and should be +1 not -1
            Z = cosPitch * System.Math.Cos(yawRadians);    // Shorten Z down from 1 based on the angle of pitch
        }

        #region Operators

        public static Vector3 Copy(Vector3 fromVector)
        {
            return new Vector3(
                fromVector.X,
                fromVector.Y,
                fromVector.Z);
        }

        public Vector3 Copy()
        {
            return Vector3.Copy(this);
        }

        /// <summary>
        /// Addition of two Vectors
        /// </summary>
        /// <param name="v1">Vector3 to be added to </param>
        /// <param name="v2">Vector3 to be added</param>
        /// <returns>Vector3 representing the sum of two Vectors</returns>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        public static Vector3 operator +(Vector3 v1, Vector3 v2)
        {
            return
            (
                new Vector3
                    (
                        v1.X + v2.X,
                        v1.Y + v2.Y,
                        v1.Z + v2.Z
                    )
            );
        }

        /// <summary>
        /// Subtraction of two Vectors
        /// </summary>
        /// <param name="v1">Vector3 to be subtracted from </param>
        /// <param name="v2">Vector3 to be subtracted</param>
        /// <returns>Vector3 representing the difference of two Vectors</returns>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        public static Vector3 operator -(Vector3 v1, Vector3 v2)
        {
            return
            (
                new Vector3
                    (
                        v1.X - v2.X,
                        v1.Y - v2.Y,
                        v1.Z - v2.Z
                    )
            );
        }

        /// <summary>
        /// Product of a Vector3 and a scalar value
        /// </summary>
        /// <param name="v1">Vector3 to be multiplied </param>
        /// <param name="s2">Scalar value to be multiplied by </param>
        /// <returns>Vector3 representing the product of the vector and scalar</returns>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        public static Vector3 operator *(Vector3 v1, double s2)
        {
            return
            (
                new Vector3
                (
                    v1.X * s2,
                    v1.Y * s2,
                    v1.Z * s2
                )
            );
        }

        /// <summary>
        /// Product of a scalar value and a Vector3
        /// </summary>
        /// <param name="s1">Scalar value to be multiplied </param>
        /// <param name="v2">Vector3 to be multiplied by </param>
        /// <returns>Vector3 representing the product of the scalar and Vector3</returns>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        /// <Implementation>
        /// Using the commutative law 'scalar x vector'='vector x scalar'.
        /// Thus, this function calls 'operator*(Vector3 v1, double s2)'.
        /// This avoids repetition of code.
        /// </Implementation>
        public static Vector3 operator *(double s1, Vector3 v2)
        {
            return v2 * s1;
        }

        /// <summary>
        /// Division of a Vector3 and a scalar value
        /// </summary>
        /// <param name="v1">Vector3 to be divided </param>
        /// <param name="s2">Scalar value to be divided by </param>
        /// <returns>Vector3 representing the division of the vector and scalar</returns>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        public static Vector3 operator /(Vector3 v1, double s2)
        {
            return
            (
                new Vector3
                    (
                        v1.X / s2,
                        v1.Y / s2,
                        v1.Z / s2
                    )
            );
        }

        /// <summary>
        /// Negation of a Vector3
        /// Invert the direction of the Vector3
        /// Make Vector3 negative (-vector)
        /// </summary>
        /// <param name="v1">Vector3 to be negated  </param>
        /// <returns>Negated vector</returns>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector3 operator -(Vector3 v1)
        {
            return
            (
                new Vector3
                    (
                        -v1.X,
                        -v1.Y,
                        -v1.Z
                    )
            );
        }

        /// <summary>
        /// Compare the magnitude of two Vectors (less than)
        /// </summary>
        /// <param name="v1">Vector3 to be compared </param>
        /// <param name="v2">Vector3 to be compared with</param>
        /// <returns>True if v1 less than v2</returns>
        public static bool operator <(Vector3 v1, Vector3 v2)
        {
            return v1.SumComponentSqrs() < v2.SumComponentSqrs();
        }

        /// <summary>
        /// Compare the magnitude of two Vectors (greater than)
        /// </summary>
        /// <param name="v1">Vector3 to be compared </param>
        /// <param name="v2">Vector3 to be compared with</param>
        /// <returns>True if v1 greater than v2</returns>
        public static bool operator >(Vector3 v1, Vector3 v2)
        {
            return v1.SumComponentSqrs() > v2.SumComponentSqrs();
        }

        /// <summary>
        /// Compare the magnitude of two Vectors (less than or equal to)
        /// </summary>
        /// <param name="v1">Vector3 to be compared </param>
        /// <param name="v2">Vector3 to be compared with</param>
        /// <returns>True if v1 less than or equal to v2</returns>
        public static bool operator <=(Vector3 v1, Vector3 v2)
        {
            return v1.SumComponentSqrs() <= v2.SumComponentSqrs();
        }

        /// <summary>
        /// Compare the magnitude of two Vectors (greater than or equal to)
        /// </summary>
        /// <param name="v1">Vector3 to be compared </param>
        /// <param name="v2">Vector3 to be compared with</param>
        /// <returns>True if v1 greater than or equal to v2</returns>
        public static bool operator >=(Vector3 v1, Vector3 v2)
        {
            return v1.SumComponentSqrs() >= v2.SumComponentSqrs();
        }

        /// <summary>
        /// Compare two Vectors for equality.
        /// Are two Vectors equal.
        /// </summary>
        /// <param name="v1">Vector3 to be compared for equality </param>
        /// <param name="v2">Vector3 to be compared to </param>
        /// <returns>Boolean decision (truth for equality)</returns>
        /// <implementation>
        /// Checks the equality of each pair of components, all pairs must be equal
        /// A tolerence to the equality operator is applied
        /// </implementation>
        public static bool operator ==(Vector3 v1, Vector3 v2)
        {
            return
            (
                System.Math.Abs(v1.X - v2.X) <= EqualityTolerence &&
                System.Math.Abs(v1.Y - v2.Y) <= EqualityTolerence &&
                System.Math.Abs(v1.Z - v2.Z) <= EqualityTolerence
            );
        }

        /// <summary>
        /// Negative comparator of two Vectors.
        /// Are two Vectors different.
        /// </summary>
        /// <param name="v1">Vector3 to be compared for in-equality </param>
        /// <param name="v2">Vector3 to be compared to </param>
        /// <returns>Boolean decision (truth for in-equality)</returns>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        /// <implementation>
        /// Uses the equality operand function for two vectors to prevent code duplication
        /// </implementation>
        public static bool operator !=(Vector3 v1, Vector3 v2)
        {
            return !(v1 == v2);
        }

        #endregion

        #region Component Operations

        /// <summary>
        /// The sum of a Vector3's components
        /// </summary>
        /// <param name="v1">The vector whose scalar components to sum</param>
        /// <returns>The sum of the Vectors X, Y and Z components</returns>
        public static double SumComponents(Vector3 v1)
        {
            return (v1.X + v1.Y + v1.Z);
        }

        /// <summary>
        /// The sum of this Vector3's components
        /// </summary>
        /// <returns>The sum of the Vectors X, Y and Z components</returns>
        /// <implementation>
        /// <see cref="SumComponents(Vector3)"/>
        /// The Components.SumComponents(Vector3) function has been used to prevent code duplication
        /// </implementation>
        public double SumComponents()
        {
            return SumComponents(this);
        }

        /// <summary>
        /// The sum of a Vector3's squared components
        /// </summary>
        /// <param name="v1">The vector whose scalar components to square and sum</param>
        /// <returns>The sum of the Vectors X^2, Y^2 and Z^2 components</returns>
        public static double SumComponentSqrs(Vector3 v1)
        {
            Vector3 v2 = SqrComponents(v1);
            return v2.SumComponents();
        }

        /// <summary>
        /// The sum of this Vector3's squared components (aka LengthSquared or MagnitudeSquared)
        /// </summary>
        /// <returns>The sum of the Vectors X^2, Y^2 and Z^2 components</returns>
        /// <implementation>
        /// <see cref="SumComponentSqrs(Vector3)"/>
        /// The Components.SumComponentSqrs(Vector3) function has been used to prevent code duplication
        /// </implementation>
        public double SumComponentSqrs()
        {
            return SumComponentSqrs(this);
        }

        /// <summary>
        /// The individual multiplication to a power of a Vector3's components
        /// </summary>
        /// <param name="v1">The vector whose scalar components to multiply by a power</param>
        /// <param name="power">The power by which to multiply the components</param>
        /// <returns>The multiplied Vector3</returns>
        public static Vector3 PowComponents(Vector3 v1, double power)
        {
            return
            (
                new Vector3
                    (
                        System.Math.Pow(v1.X, power),
                        System.Math.Pow(v1.Y, power),
                        System.Math.Pow(v1.Z, power)
                    )
            );
        }

        /// <summary>
        /// The individual multiplication to a power of this Vector3's components
        /// </summary>
        /// <param name="power">The power by which to multiply the components</param>
        /// <returns>The multiplied Vector3</returns>
        /// <implementation>
        /// <see cref="PowComponents(Vector3, Double)"/>
        /// The Components.PowComponents(Vector3, double) function has been used to prevent code duplication
        /// </implementation>
        public Vector3 PowComponents(double power)
        {
            return PowComponents(this, power);
        }

        /// <summary>
        /// The individual square root of a Vector3's components
        /// </summary>
        /// <param name="v1">The vector whose scalar components to square root</param>
        /// <returns>The rooted Vector3</returns>
        public static Vector3 SqrtComponents(Vector3 v1)
        {
            return
                (
                new Vector3
                    (
                        System.Math.Sqrt(v1.X),
                        System.Math.Sqrt(v1.Y),
                        System.Math.Sqrt(v1.Z)
                    )
                );
        }

        /// <summary>
        /// The individual square root of this Vector3's components
        /// </summary>
        /// <returns>The rooted Vector3</returns>
        /// <implementation>
        /// <see cref="SqrtComponents(Vector3)"/>
        /// The Components.SqrtComponents(Vector3) function has been used to prevent code duplication
        /// </implementation>
        public Vector3 SqrtComponents()
        {
            return SqrtComponents(this);
        }

        /// <summary>
        /// The Vector3's components squared
        /// </summary>
        /// <param name="v1">The vector whose scalar components are to square</param>
        /// <returns>The squared Vector3</returns>
        public static Vector3 SqrComponents(Vector3 v1)
        {
            return
                (
                new Vector3
                    (
                        v1.X * v1.X,
                        v1.Y * v1.Y,
                        v1.Z * v1.Z
                    )
                );
        }

        /// <summary>
        /// The Vector3's components squared
        /// </summary>
        /// <returns>The squared Vector3</returns>
        /// <implementation>
        /// <see cref="SqrtComponents(Vector3)"/>
        /// The Components.SqrComponents(Vector3) function has been used to prevent code duplication
        /// </implementation>
        public Vector3 SqrComponents()
        {
            return SqrtComponents(this);
        }

        #endregion

        #region Functions

        /// <summary>
        /// Determine the cross product of two Vectors
        /// Determine the vector product
        /// Determine the normal vector (Vector3 90° to the plane)
        /// </summary>
        /// <param name="v1">The vector to multiply</param>
        /// <param name="v2">The vector to multiply by</param>
        /// <returns>Vector3 representing the cross product of the two vectors</returns>
        /// <implementation>
        /// Cross products are non commutable
        /// </implementation>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        public static Vector3 CrossProduct(Vector3 v1, Vector3 v2)
        {
            return
            (
                new Vector3
                (
                    v1.Y * v2.Z - v1.Z * v2.Y,
                    v1.Z * v2.X - v1.X * v2.Z,
                    v1.X * v2.Y - v1.Y * v2.X
                )
            );
        }

        /// <summary>
        /// Determine the cross product of this Vector3 and another
        /// Determine the vector product
        /// Determine the normal vector (Vector3 90° to the plane)
        /// </summary>
        /// <param name="other">The vector to multiply by</param>
        /// <returns>Vector3 representing the cross product of the two vectors</returns>
        /// <implementation>
        /// Uses the CrossProduct function to avoid code duplication
        /// <see cref="CrossProduct(Vector3, Vector3)"/>
        /// </implementation>
        public Vector3 CrossProduct(Vector3 other)
        {
            return CrossProduct(this, other);
        }

        /// <summary>
        /// Determine the dot product of two Vectors on the X,Z plane
        /// </summary>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <returns></returns>
        public static double DotProductXZ(Vector3 v1, Vector3 v2)
        {
            return 
            (
                v1.X * v2.X +
                v1.Z * v2.Z
            );
        }

        /// <summary>
        /// Determine the dot product of this Vector3 and another on the X,Z plane
        /// </summary>
        /// <param name="other">The vector to multiply by</param>
        /// <returns>Scalar representing the dot product of the two vectors on the X,Z</returns>
        public double DotProductXZ(Vector3 other)
        {
            return DotProductXZ(this, other);
        }

        /// <summary>
        /// Determine the dot product of two Vectors
        /// </summary>
        /// <param name="v1">The vector to multiply</param>
        /// <param name="v2">The vector to multiply by</param>
        /// <returns>Scalar representing the dot product of the two vectors</returns>
        /// <implementation>
        /// </implementation>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        public static double DotProduct(Vector3 v1, Vector3 v2)
        {
            return
            (
                v1.X * v2.X +
                v1.Y * v2.Y +
                v1.Z * v2.Z
            );
        }

        /// <summary>
        /// Determine the dot product of this Vector3 and another
        /// </summary>
        /// <param name="other">The vector to multiply by</param>
        /// <returns>Scalar representing the dot product of the two vectors</returns>
        /// <implementation>
        /// <see cref="DotProduct(Vector3)"/>
        /// </implementation>
        public double DotProduct(Vector3 other)
        {
            return DotProduct(this, other);
        }

        /// <summary>
        /// Determine the mixed product of three Vectors
        /// Determine volume (with sign precision) of parallelepiped spanned on given vectors
        /// Determine the scalar triple product of three vectors
        /// </summary>
        /// <param name="v1">The first vector</param>
        /// <param name="v2">The second vector</param>
        /// <param name="v3">The third vector</param>
        /// <returns>Scalar representing the mixed product of the three vectors</returns>
        /// <implementation>
        /// Mixed products are non commutable
        /// <see cref="CrossProduct(Vector3, Vector3)"/>
        /// <see cref="DotProduct(Vector3, Vector3)"/>
        /// </implementation>
        /// <Acknowledgement>This code was provided by Michał Bryłka</Acknowledgement>
        public static double MixedProduct(Vector3 v1, Vector3 v2, Vector3 v3)
        {
            return DotProduct(CrossProduct(v1, v2), v3);
        }

        /// <summary>
        /// Determine the mixed product of three Vectors
        /// Determine volume (with sign precision) of parallelepiped spanned on given vectors
        /// Determine the scalar triple product of three vectors
        /// </summary>
        /// <param name="other_v1">The second vector</param>
        /// <param name="other_v2">The third vector</param>
        /// <returns>Scalar representing the mixed product of the three vectors</returns>
        /// <implementation>
        /// Mixed products are non commutable
        /// <see cref="MixedProduct(Vector3, Vector3, Vector3)"/>
        /// Uses MixedProduct(Vector3, Vector3, Vector3) to avoid code duplication
        /// </implementation>
        public double MixedProduct(Vector3 other_v1, Vector3 other_v2)
        {
            return DotProduct(CrossProduct(this, other_v1), other_v2);
        }

        /// <summary>
        /// Get the normalized vector
        /// Get the unit vector
        /// Scale the Vector3 so that the magnitude is 1
        /// </summary>
        /// <param name="v1">The vector to be normalized</param>
        /// <returns>The normalized Vector3</returns>
        /// <implementation>
        /// Uses the Magnitude function to avoid code duplication 
        /// </implementation>
        /// <exception cref="System.DivideByZeroException">
        /// Thrown when the normalisation of a zero magnitude vector is attempted
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector3 Normalize(Vector3 v1)
        {
            // Check for divide by zero errors
            if (v1.Magnitude == 0)
            {
                throw new DivideByZeroException(NORMALIZE_0);
            }
            else
            {
                // find the inverse of the vectors magnitude
                double inverse = 1 / v1.Magnitude;
                return
                (
                    new Vector3
                    (
                    // multiply each component by the inverse of the magnitude
                        v1.X * inverse,
                        v1.Y * inverse,
                        v1.Z * inverse
                    )
                );
            }
        }

        /// <summary>
        /// Get the normalized vector
        /// Get the unit vector
        /// Scale the Vector3 so that the magnitude is 1
        /// </summary>
        /// <returns>The normalized Vector3</returns>
        /// <implementation>
        /// Uses the Magnitude and Normalize function to avoid code duplication 
        /// </implementation>
        /// <exception cref="System.DivideByZeroException">
        /// Thrown when the normalisation of a zero magnitude vector is attempted
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public Vector3 Normalize()
        {
            return Normalize(this);
        }

        /// <summary>
        /// Take an interpolated value from between two Vectors or an extrapolated value if allowed
        /// </summary>
        /// <param name="v1">The Vector3 to interpolate from (where control ==0)</param>
        /// <param name="v2">The Vector3 to interpolate to (where control ==1)</param>
        /// <param name="control">The interpolated point between the two vectors to retrieve (fraction between 0 and 1), or an extrapolated point if allowed</param>
        /// <param name="allowExtrapolation">True if the control may represent a point not on the vertex between v1 and v2</param>
        /// <returns>The value at an arbitrary distance (interpolation) between two vectors or an extrapolated point on the extended virtex</returns>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the control is not between values of 0 and 1 and extrapolation is not allowed
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector3 Interpolate(Vector3 v1, Vector3 v2, double control, bool allowExtrapolation)
        {
            if (!allowExtrapolation && (control > 1 || control < 0))
            {
                // Error message includes information about the actual value of the argument
                throw new ArgumentOutOfRangeException
                        (
                            "control",
                            control,
                            INTERPOLATION_RANGE + "\n" + ARGUMENT_VALUE + control
                        );
            }
            else
            {
                return
                (
                    new Vector3
                    (
                        v1.X * (1 - control) + v2.X * control,
                        v1.Y * (1 - control) + v2.Y * control,
                        v1.Z * (1 - control) + v2.Z * control
                    )
                );
            }
        }

        /// <summary>
        /// Take an interpolated value from between two Vectors
        /// </summary>
        /// <param name="v1">The Vector3 to interpolate from (where control ==0)</param>
        /// <param name="v2">The Vector3 to interpolate to (where control ==1)</param>
        /// <param name="control">The interpolated point between the two vectors to retrieve (fraction between 0 and 1)</param>
        /// <returns>The value at an arbitrary distance (interpolation) between two vectors</returns>
        /// <implementation>
        /// <see cref="Interpolate(Vector3, Vector3, double, bool)"/>
        /// Uses the Interpolate(Vector3,Vector3,double,bool) method to avoid code duplication
        /// </implementation>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the control is not between values of 0 and 1
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector3 Interpolate(Vector3 v1, Vector3 v2, double control)
        {
            return Interpolate(v1, v2, control, false);
        }


        /// <summary>
        /// Take an interpolated value from between two Vectors
        /// </summary>
        /// <param name="other">The Vector3 to interpolate to (where control ==1)</param>
        /// <param name="control">The interpolated point between the two vectors to retrieve (fraction between 0 and 1)</param>
        /// <returns>The value at an arbitrary distance (interpolation) between two vectors</returns>
        /// <implementation>
        /// <see cref="Interpolate(Vector3, Vector3, double)"/>
        /// Overload for Interpolate method, finds an interpolated value between this Vector3 and another
        /// Uses the Interpolate(Vector3,Vector3,double) method to avoid code duplication
        /// </implementation>
        public Vector3 Interpolate(Vector3 other, double control)
        {
            return Interpolate(this, other, control);
        }

        /// <summary>
        /// Take an interpolated value from between two Vectors or an extrapolated value if allowed
        /// </summary>
        /// <param name="other">The Vector3 to interpolate to (where control ==1)</param>
        /// <param name="control">The interpolated point between the two vectors to retrieve (fraction between 0 and 1), or an extrapolated point if allowed</param>
        /// <param name="allowExtrapolation">True if the control may represent a point not on the vertex between v1 and v2</param>
        /// <returns>The value at an arbitrary distance (interpolation) between two vectors or an extrapolated point on the extended virtex</returns>
        /// <implementation>
        /// <see cref="Interpolate(Vector3, Vector3, double, bool)"/>
        /// Uses the Interpolate(Vector3,Vector3,double,bool) method to avoid code duplication
        /// </implementation>
        /// <exception cref="System.ArgumentOutOfRangeException">
        /// Thrown when the control is not between values of 0 and 1 and extrapolation is not allowed
        /// </exception>
        public Vector3 Interpolate(Vector3 other, double control, bool allowExtrapolation)
        {
            return Interpolate(this, other, control);
        }
  
        const double cSmallNumber = 0.00000007; // the small number is used to avoid division overflows
        
        /// <summary>
        /// Finds the intersection point for the line segment on plane.
        /// </summary>
        /// <returns>
        /// The point where the line segment intersects with plane, null if the line segment does not intersect the plane.
        /// </returns>
        /// <param name='v1'>
        /// The start of the segment
        /// </param>
        /// <param name='v2'>
        /// The end of the segment
        /// </param>
        /// <param name='planePoint'>
        /// A point on the plane.
        /// </param>
        /// <param name='planeNormal'>
        /// The plane normal (e.g. Vector3.XAxis represents the normal for south/north facing plane)
        /// </param>
        /// <Acknowledgement>Adapted from "http://softsurfer.com/Archive/algorithm_0104/algorithm_0104B.htm#Line-Plane%20Intersection"</Acknowledgement>
        public static Vector3? IntersectPointForSegmentAndPlane(Vector3 v1, Vector3 v2, Vector3 planePoint, Vector3 planeNormal)
        {
            // v1->v2 represent a line/ray/segment
            Vector3 u = v2 - v1;
            Vector3 w = v1 - planePoint;
            
            double D = planeNormal.DotProduct(u);
            double N = -(planeNormal.DotProduct(w));
            
            if (System.Math.Abs(D) < cSmallNumber) // segment is parallel to plane
            {
                // if N == 0 the line segment is contained in the plane
                // else no intersection
                return null;
            }
            
            // Not parallel so compute the intersect param
            double sI = N / D;
            if (sI < 0.0 || sI > 1.0) // no intersection
                return null;
                
            return v1 + sI * u; // compute segement intersect point
        }
        
        /// <summary>
        /// Finds the intersection point for the line segment on plane.
        /// </summary>
        /// <returns>
        /// The point where the line segment intersects with plane, null if the line segment does not intersect the plane.
        /// </returns>
        /// <param name='segmentEnd'>
        /// The end of the segment
        /// </param>
        /// <param name='planePoint'>
        /// A point on the plane.
        /// </param>
        /// <param name='planeNormal'>
        /// The plane normal (e.g. Vector3.XAxis represents the normal for north facing plane)
        /// </param>
        public Vector3? IntersectPointForSegmentAndPlane(Vector3 segmentEnd, Vector3 planePoint, Vector3 planeNormal)
        {
            return IntersectPointForSegmentAndPlane(this, segmentEnd, planePoint, planeNormal);
        }
              
        /// <summary>
        /// Find the distance squared between two vectors.
        /// </summary>
        /// <returns>
        /// The distance squared.
        /// </returns>
        /// <param name='v1'>
        /// The Vector3 to find the distance from
        /// </param>
        /// <param name='v2'>
        /// The Vector3 to find the distance to
        /// </param>
        public static double DistanceSquared(Vector3 v1, Vector3 v2)
        {
            return (v1.X - v2.X) * (v1.X - v2.X) +
                   (v1.Y - v2.Y) * (v1.Y - v2.Y) +
                   (v1.Z - v2.Z) * (v1.Z - v2.Z);
        }
        
        /// <summary>
        /// Find the distance squared between two vectors.
        /// </summary>
        /// <returns>
        /// The distance squared.
        /// </returns>
        /// <param name='other'>
        /// The Vector3 to find the distance to
        /// </param>
        public double DistanceSquared(Vector3 other)
        {
            return Vector3.DistanceSquared(this, other);
        }
                                
        /// <summary>
        /// Find the distance between two Vectors
        /// Pythagoras theorem on two Vectors
        /// </summary>
        /// <param name="v1">The Vector3 to find the distance from </param>
        /// <param name="v2">The Vector3 to find the distance to </param>
        /// <returns>The distance between two Vectors</returns>
        /// <implementation>
        /// </implementation>
        public static double Distance(Vector3 v1, Vector3 v2)
        {
            return
            (
                System.Math.Sqrt
                (
                    Vector3.DistanceSquared(v1, v2)
                )
            );
        }

        /// <summary>
        /// Find the distance between two Vectors
        /// Pythagoras theorem on two Vectors
        /// </summary>
        /// <param name="other">The Vector3 to find the distance to </param>
        /// <returns>The distance between two Vectors</returns>
        /// <implementation>
        /// <see cref="Distance(Vector3, Vector3)"/>
        /// Overload for Distance method, finds distance between this Vector3 and another
        /// Uses the Distance(Vector3,Vector3) method to avoid code duplication
        /// </implementation>
        public double Distance(Vector3 other)
        {
            return Distance(this, other);
        }

        /// <summary>
        /// Find the angle between two Vectors
        /// </summary>
        /// <param name="v1">The Vector3 to discern the angle from </param>
        /// <param name="v2">The Vector3 to discern the angle to</param>
        /// <returns>The angle between two positional Vectors</returns>
        /// <implementation>
        /// </implementation>
        /// <Acknowledgement>F.Hill, 2001, Computer Graphics using OpenGL, 2ed </Acknowledgement>
        public static double Angle(Vector3 v1, Vector3 v2)
        {
            return
            (
                System.Math.Acos
                    (
                        Normalize(v1).DotProduct(Normalize(v2))
                    )
            );
        }

        /// <summary>
        /// Find the angle between this Vector3 and another
        /// </summary>
        /// <param name="other">The Vector3 to discern the angle to</param>
        /// <returns>The angle between two positional Vectors</returns>
        /// <implementation>
        /// <see cref="Angle(Vector3, Vector3)"/>
        /// Uses the Angle(Vector3,Vector3) method to avoid code duplication
        /// </implementation>
        public double Angle(Vector3 other)
        {
            return Angle(this, other);
        }

        /// <summary>
        /// Returned the signed angle between two vectors
        /// </summary>
        /// <param name="source">The source vector</param>
        /// <param name="dest">The destination vector</param>
        /// <param name="destsRight">A vector perpendicular to the left of <paramref name="dest"/></param>
        /// <returns></returns>
        /// <remarks>
        /// Note: this is modified to work with the non-classical trigonometry rules used in Minecraft (http://mc.kev009.com/Protocol#Player_Look_.280x0C.29)
        /// </remarks>
        public static double SignedAngle(Vector3 source, Vector3 dest, Vector3 destsRight)
        {
            // We make sure all of our vectors are unit length (but not modifying originals)
            source = source.Normalize();
            dest = dest.Normalize();
            destsRight = destsRight.Normalize();

            double forwardDot = Vector3.DotProduct(source, dest);
            double rightDot = Vector3.DotProduct(source, destsRight);

            // Make sure we stay in range no matter what, so Acos doesn't fail later 
            forwardDot = forwardDot.Clamp(-1.0, 1.0);

            double angleBetween = System.Math.Acos(forwardDot);

            if (rightDot > 0.0) // Without minecraft modification this would be 'rightDot < 0.0'
                angleBetween *= -1.0;

            return angleBetween;
        }

        public double SignedAngle(Vector3 dest, Vector3 destsRight)
        {
            return SignedAngle(this, dest, destsRight);
        }

        /// <summary>
        /// compares the magnitude of two Vectors and returns the greater Vector3
        /// </summary>
        /// <param name="v1">The vector to compare</param>
        /// <param name="v2">The vector to compare with</param>
        /// <returns>
        /// The greater of the two Vectors (based on magnitude)
        /// </returns>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector3 Max(Vector3 v1, Vector3 v2)
        {
            if (v1 >= v2) { return v1; }
            return v2;
        }

        /// <summary>
        /// compares the magnitude of two Vectors and returns the greater Vector3
        /// </summary>
        /// <param name="other">The vector to compare with</param>
        /// <returns>
        /// The greater of the two Vectors (based on magnitude)
        /// </returns>
        /// <implementation>
        /// <see cref="Max(Vector3, Vector3)"/>
        /// Uses function Max(Vector3, Vector3) to avoid code duplication
        /// </implementation>
        public Vector3 Max(Vector3 other)
        {
            return Max(this, other);
        }

        /// <summary>
        /// compares the magnitude of two Vectors and returns the lesser Vector3
        /// </summary>
        /// <param name="v1">The vector to compare</param>
        /// <param name="v2">The vector to compare with</param>
        /// <returns>
        /// The lesser of the two Vectors (based on magnitude)
        /// </returns>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static Vector3 Min(Vector3 v1, Vector3 v2)
        {
            if (v1 <= v2) { return v1; }
            return v2;
        }

        /// <summary>
        /// Compares the magnitude of two Vectors and returns the greater Vector3
        /// </summary>
        /// <param name="other">The vector to compare with</param>
        /// <returns>
        /// The lesser of the two Vectors (based on magnitude)
        /// </returns>
        /// <implementation>
        /// <see cref="Min(Vector3, Vector3)"/>
        /// Uses function Min(Vector3, Vector3) to avoid code duplication
        /// </implementation>
        public Vector3 Min(Vector3 other)
        {
            return Min(this, other);
        }

        /// <summary>
        /// Rotates a Vector3 around the Y axis
        /// Change the yaw of a Vector3
        /// </summary>
        /// <param name="v1">The Vector3 to be rotated</param>
        /// <param name="angle">The angle to rotate the Vector3 around in degrees</param>
        /// <returns>Vector3 representing the rotation around the Y axis</returns>
        /// <remarks>
        /// Note: this is modified to work with the non-classical trigonometry rules used in Minecraft (http://mc.kev009.com/Protocol#Player_Look_.280x0C.29)
        /// </remarks>
        public static Vector3 Yaw(Vector3 v1, double angle)
        {
            // Negate Sine as X+ is North in trigonometry rules and we need it to be South, Z+ is east, angle+ is clockwise (as per remark above)
            double x = (v1.Z * -System.Math.Sin(angle)) + (v1.X * System.Math.Cos(angle));
            double y = v1.Y;
            // Negate Sine as X+ is North in trigonometry rules and we need it to be South, Z+ is east, angle+ is clockwise (as per remark above)
            double z = (v1.Z * System.Math.Cos(angle)) - (v1.X * -System.Math.Sin(angle));
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Rotates the Vector3 around the Y axis
        /// Change the yaw of the Vector3
        /// </summary>
        /// <param name="angle">The angle to rotate the Vector3 around in degrees</param>
        /// <returns>Vector3 representing the rotation around the Y axis</returns>
        /// <implementation>
        /// <see cref="Yaw(Vector3, Double)"/>
        /// Uses function Yaw(Vector3, double) to avoid code duplication
        /// </implementation>
        public Vector3 Yaw(double angle)
        {
            return Yaw(this, angle);
        }

        /// <summary>
        /// Rotates a Vector3 around the X axis
        /// Change the pitch of a Vector3
        /// </summary>
        /// <param name="v1">The Vector3 to be rotated</param>
        /// <param name="angle">The angle to rotate the Vector3 around in degrees</param>
        /// <returns>Vector3 representing the rotation around the X axis</returns>
        public static Vector3 Pitch(Vector3 v1, double angle)
        {
            double x = v1.X;
            double y = (v1.Y * System.Math.Cos(angle)) - (v1.Z * System.Math.Sin(angle));
            double z = (v1.Y * System.Math.Sin(angle)) + (v1.Z * System.Math.Cos(angle));
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Rotates a Vector3 around the X axis
        /// Change the pitch of a Vector3
        /// </summary>
        /// <param name="angle">The angle to rotate the Vector3 around in degrees</param>
        /// <returns>Vector3 representing the rotation around the X axis</returns>
        /// <see cref="Pitch(Vector3, Double)"/>
        /// <implementation>
        /// Uses function Pitch(Vector3, double) to avoid code duplication
        /// </implementation>
        public Vector3 Pitch(double angle)
        {
            return Pitch(this, angle);
        }

        /// <summary>
        /// Rotates a Vector3 around the Z axis
        /// Change the roll of a Vector3
        /// </summary>
        /// <param name="v1">The Vector3 to be rotated</param>
        /// <param name="angle">The angle to rotate the Vector3 around in degrees</param>
        /// <returns>Vector3 representing the rotation around the Z axis</returns>
        public static Vector3 Roll(Vector3 v1, double angle)
        {
            double x = (v1.X * System.Math.Cos(angle)) - (v1.Y * System.Math.Sin(angle));
            double y = (v1.X * System.Math.Sin(angle)) + (v1.Y * System.Math.Cos(angle));
            double z = v1.Z;
            return new Vector3(x, y, z);
        }

        /// <summary>
        /// Rotates a Vector3 around the Z axis
        /// Change the roll of a Vector3
        /// </summary>
        /// <param name="angle">The angle to rotate the Vector3 around in degrees</param>
        /// <returns>Vector3 representing the rotation around the Z axis</returns>
        /// <implementation>
        /// <see cref="Roll(Vector3, Double)"/>
        /// Uses function Roll(Vector3, double) to avoid code duplication
        /// </implementation>
        public Vector3 Roll(double angle)
        {
            return Roll(this, angle);
        }

        /// <summary>
        /// Reflect this Vector3 about a given other vector
        /// </summary>
        /// <param name="reflector">
        /// The Vector3 to reflect about
        /// </param>
        public Vector3 Reflection(Vector3 reflector)
        {
            return Vector3.Reflection(this, reflector);
        }

        /// <summary>
        /// Reflect a Vector3 about a given other vector
        /// </summary>
        /// <param name="vector">
        /// The Vector3 to reflect
        /// </param>
        /// <param name="reflector">
        /// The Vector3 to reflect about
        /// </param>
        /// <returns>
        /// The reflected Vector3
        /// </returns>
        public static Vector3 Reflection(Vector3 vector, Vector3 reflector)
        {
            // if reflector has a right angle to vector, return -vector and don't do all
            // the other calculations
            if (System.Math.Abs(System.Math.Abs(vector.Angle(reflector)) - System.Math.PI / 2) < Double.Epsilon)
            {
                return -vector;
            }
            else
            {
                Vector3 retval = new Vector3(2 * vector.Projection(reflector) - vector);
                retval.Magnitude = vector.Magnitude;
                return retval;
            }
        }

        /// <summary>
        /// Find the absolute value of a Vector3
        /// Find the magnitude of a Vector3
        /// </summary>
        /// <returns>A Vector3 representing the absolute values of the vector</returns>
        /// <implementation>
        /// An alternative interface to the magnitude property
        /// </implementation>
        public static Double Abs(Vector3 v1)
        {
            return v1.Magnitude;
        }

        /// <summary>
        /// Find the absolute value of a Vector3
        /// Find the magnitude of a Vector3
        /// </summary>
        /// <returns>A Vector3 representing the absolute values of the vector</returns>
        /// <implementation>
        /// An alternative interface to the magnitude property
        /// </implementation>
        public double Abs()
        {
            return this.Magnitude;
        }

        /// <summary>
        /// Projects the specified v1 onto the specified v2
        /// </summary>
        /// <param name="v1">The vector that will be projected.</param>
        /// <param name="v2">The vector that will be projected upon.</param>
        /// <returns></returns>
        public static Vector3 Projection(Vector3 v1, Vector3 v2)
        {
            // http://de.wikibooks.org/wiki/Ing_Mathematik:_Vektoren#Vektorprojektion
            // http://mathworld.wolfram.com/Reflection.html
            // V1_projectedOn_V2 = v2 * (v1 * v2 / (|v2| ^ 2))

            return new Vector3(v2 * (v1.DotProduct(v2) / v2.SumComponentSqrs()));
        }

        /// <summary>
        /// Projects this vector onto the specified v2
        /// </summary>
        /// <param name="v2">The vector that will be projected upon.</param>
        /// <returns></returns>
        public Vector3 Projection(Vector3 v2)
        {
            return Vector3.Projection(this, v2);
        }

        #endregion

        #region Standard Functions

        /// <summary>
        /// Textual description of the Vector3
        /// </summary>
        /// <Implementation>
        /// Uses ToString(string, IFormatProvider) to avoid code duplication
        /// </Implementation>
        /// <returns>Text (String) representing the vector</returns>
        public override string ToString()
        {
            return ToString(null, null);
        }

        /// <summary>
        /// Verbose textual description of the Vector3
        /// </summary>
        /// <returns>Text (string) representing the vector</returns>
        public string ToVerbString()
        {
            string output = null;

            if (IsUnitVector()) { output += UNIT_VECTOR; }
            else { output += POSITIONAL_VECTOR; }

            output += string.Format("( x={0}, y={1}, z={2} )", X, Y, Z);
            output += MAGNITUDE + Magnitude;

            return output;
        }

        /// <summary>
        /// Textual description of the Vector3
        /// </summary>
        /// <param name="format">Formatting string: 'x','y','z' or '' followed by standard numeric format string characters valid for a double precision floating point</param>
        /// <param name="formatProvider">The culture specific fromatting provider</param>
        /// <returns>Text (String) representing the vector</returns>
        public string ToString(string format, IFormatProvider formatProvider)
        {
            // If no format is passed
            if (format == null || format == "") return String.Format("({0}, {1}, {2})", X, Y, Z);

            char firstChar = format[0];
            string remainder = null;

            if (format.Length > 1)
                remainder = format.Substring(1);

            switch (firstChar)
            {
                case 'x': return X.ToString(remainder, formatProvider);
                case 'y': return Y.ToString(remainder, formatProvider);
                case 'z': return Z.ToString(remainder, formatProvider);
                default:
                    return String.Format
                        (
                            "({0}, {1}, {2})",
                            X.ToString(format, formatProvider),
                            Y.ToString(format, formatProvider),
                            Z.ToString(format, formatProvider)
                        );
            }
        }

        /// <summary>
        /// Get the hashcode
        /// </summary>
        /// <returns>Hashcode for the object instance</returns>
        /// <implementation>
        /// Required in order to implement comparator operations (i.e. ==, !=)
        /// </implementation>
        /// <Acknowledgement>This code is adapted from CSOpenGL - Lucas Viñas Livschitz </Acknowledgement>
        public override int GetHashCode()
        {
            return
            (
                (int)((X + Y + Z) % Int32.MaxValue)
            );
        }

        /// <summary>
        /// Comparator
        /// </summary>
        /// <param name="other">The other object (which should be a vector) to compare to</param>
        /// <returns>Truth if two vectors are equal within a tolerence</returns>
        /// <implementation>
        /// Checks if the object argument is a Vector3 object 
        /// Uses the equality operator function to avoid code duplication
        /// Required in order to implement comparator operations (i.e. ==, !=)
        /// </implementation>
        public override bool Equals(object other)
        {
            // Check object other is a Vector3 object
            if (other is Vector3)
            {
                // Convert object to Vector3
                Vector3 otherVector = (Vector3)other;

                // Check for equality
                return otherVector == this;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Comparator
        /// </summary>
        /// <param name="other">The other Vector3 to compare to</param>
        /// <returns>Truth if two vectors are equal within a tolerence</returns>
        /// <implementation>
        /// Uses the equality operator function to avoid code duplication
        /// </implementation>
        public bool Equals(Vector3 other)
        {
            return other == this;
        }

        /// <summary>
        /// compares the magnitude of this instance against the magnitude of the supplied vector
        /// </summary>
        /// <param name="other">The vector to compare this instance with</param>
        /// <returns>
        /// -1: The magnitude of this instance is less than the others magnitude
        /// 0: The magnitude of this instance equals the magnitude of the other
        /// 1: The magnitude of this instance is greater than the magnitude of the other
        /// </returns>
        /// <implementation>
        /// Implemented to fulfil the IComparable interface
        /// </implementation>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public int CompareTo(Vector3 other)
        {
            if (this < other)
            {
                return -1;
            }
            else if (this > other)
            {
                return 1;
            }
            return 0;
        }

        /// <summary>
        /// compares the magnitude of this instance against the magnitude of the supplied vector
        /// </summary>
        /// <param name="other">The vector to compare this instance with</param>
        /// <returns>
        /// -1: The magnitude of this instance is less than the others magnitude
        /// 0: The magnitude of this instance equals the magnitude of the other
        /// 1: The magnitude of this instance is greater than the magnitude of the other
        /// </returns>
        /// <implementation>
        /// Implemented to fulfil the IComparable interface
        /// </implementation>
        /// <exception cref="ArgumentException">
        /// Throws an exception if the type of object to be compared is not known to this class
        /// </exception>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public int CompareTo(object other)
        {
            if (other is Vector3)
            {
                return CompareTo((Vector3)other);
            }
            else
            {
                // Error condition: other is not a Vector3 object
                throw new ArgumentException
                (
                    // Error message includes information about the actual type of the argument
                    NON_VECTOR_COMPARISON + "\n" + ARGUMENT_TYPE + other.GetType().ToString(),
                    "other"
                );
            }
        }

        #endregion

        #region Decisions

        /// <summary>
        /// Checks if a vector a unit vector
        /// Checks if the Vector3 has been normalized
        /// Checks if a vector has a magnitude of 1
        /// </summary>
        /// <param name="v1">
        /// The vector to be checked for Normalization
        /// </param>
        /// <returns>Truth if the vector is a unit vector</returns>
        /// <implementation>
        /// <see cref="Magnitude"/>	
        /// Uses the Magnitude property in the check to avoid code duplication
        /// Within a tolerence
        /// </implementation>
        public static bool IsUnitVector(Vector3 v1)
        {
            return System.Math.Abs(v1.Magnitude - 1) <= EqualityTolerence;
        }

        /// <summary>
        /// Checks if the vector a unit vector
        /// Checks if the Vector3 has been normalized 
        /// Checks if the vector has a magnitude of 1
        /// </summary>
        /// <returns>Truth if this vector is a unit vector</returns>
        /// <implementation>
        /// <see cref="IsUnitVector(Vector3)"/>	
        /// Uses the isUnitVector(Vector3) property in the check to avoid code duplication
        /// Within a tolerence
        /// </implementation>
        public bool IsUnitVector()
        {
            return IsUnitVector(this);
        }

        /// <summary>
        /// Checks if a face normal vector represents back face
        /// Checks if a face is visible, given the line of sight
        /// </summary>
        /// <param name="normal">
        /// The vector representing the face normal Vector3
        /// </param>
        /// <param name="lineOfSight">
        /// The unit vector representing the direction of sight from a virtual camera
        /// </param>
        /// <returns>Truth if the vector (as a normal) represents a back face</returns>
        /// <implementation>
        /// Uses the DotProduct function in the check to avoid code duplication
        /// </implementation>
        public static bool IsBackFace(Vector3 normal, Vector3 lineOfSight)
        {
            return normal.DotProduct(lineOfSight) < 0;
        }

        /// <summary>
        /// Checks if a face normal vector represents back face
        /// Checks if a face is visible, given the line of sight
        /// </summary>
        /// <param name="lineOfSight">
        /// The unit vector representing the direction of sight from a virtual camera
        /// </param>
        /// <returns>Truth if the vector (as a normal) represents a back face</returns>
        /// <implementation>
        /// <see cref="Vector3.IsBackFace(Vector3, Vector3)"/> 
        /// Uses the isBackFace(Vector3, Vector3) function in the check to avoid code duplication
        /// </implementation>
        public bool IsBackFace(Vector3 lineOfSight)
        {
            return IsBackFace(this, lineOfSight);
        }

        /// <summary>
        /// Checks if two Vectors are perpendicular
        /// Checks if two Vectors are orthogonal
        /// Checks if one Vector3 is the normal of the other
        /// </summary>
        /// <param name="v1">
        /// The vector to be checked for orthogonality
        /// </param>
        /// <param name="v2">
        /// The vector to be checked for orthogonality to
        /// </param>
        /// <returns>Truth if the two Vectors are perpendicular</returns>
        /// <implementation>
        /// Uses the DotProduct function in the check to avoid code duplication
        /// </implementation>
        public static bool IsPerpendicular(Vector3 v1, Vector3 v2)
        {
            return v1.DotProduct(v2) == 0;
        }

        /// <summary>
        /// Checks if two Vectors are perpendicular
        /// Checks if two Vectors are orthogonal
        /// Checks if one Vector3 is the Normal of the other
        /// </summary>
        /// <param name="other">
        /// The vector to be checked for orthogonality
        /// </param>
        /// <returns>Truth if the two Vectors are perpendicular</returns>
        /// <implementation>
        /// Uses the isPerpendicualr(Vector3, Vector3) function in the check to avoid code duplication
        /// </implementation>
        public bool IsPerpendicular(Vector3 other)
        {
            return IsPerpendicular(this, other);
        }

        #endregion

        #region Cartesian Vectors

        /// <summary>
        /// Vector3 representing the Cartesian origin
        /// </summary>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static readonly Vector3 Origin = new Vector3(0, 0, 0);

        /// <summary>
        /// Vector3 representing the Cartesian XAxis
        /// </summary>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static readonly Vector3 XAxis = new Vector3(1, 0, 0);

        /// <summary>
        /// Vector3 representing the Cartesian YAxis
        /// </summary>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static readonly Vector3 YAxis = new Vector3(0, 1, 0);

        /// <summary>
        /// Vector3 representing the Cartesian ZAxis
        /// </summary>
        /// <Acknowledgement>This code is adapted from Exocortex - Ben Houston </Acknowledgement>
        public static readonly Vector3 ZAxis = new Vector3(0, 0, 1);

        #endregion

        #region Messages

        /// <summary>
        /// Exception message descriptive text 
        /// Used for a failure for an array argument to have three components when three are needed 
        /// </summary>
        private const string THREE_COMPONENTS = "Array must contain exactly three components , (x,y,z)";

        /// <summary>
        /// Exception message descriptive text 
        /// Used for a divide by zero event caused by the normalization of a vector with magnitude 0 
        /// </summary>
        private const string NORMALIZE_0 = "Can not normalize a vector when it's magnitude is zero";

        /// <summary>
        /// Exception message descriptive text 
        /// Used when interpolation is attempted with a control parameter not between 0 and 1 
        /// </summary>
        private const string INTERPOLATION_RANGE = "Control parameter must be a value between 0 & 1";

        /// <summary>
        /// Exception message descriptive text 
        /// Used when attempting to compare a Vector3 to an object which is not a type of Vector3 
        /// </summary>
        private const string NON_VECTOR_COMPARISON = "Cannot compare a Vector3 to a non-Vector3";

        /// <summary>
        /// Exception message additional information text 
        /// Used when adding type information of the given argument into an error message 
        /// </summary>
        private const string ARGUMENT_TYPE = "The argument provided is a type of ";

        /// <summary>
        /// Exception message additional information text 
        /// Used when adding value information of the given argument into an error message 
        /// </summary>
        private const string ARGUMENT_VALUE = "The argument provided has a value of ";

        /// <summary>
        /// Exception message additional information text 
        /// Used when adding length (number of components in an array) information of the given argument into an error message 
        /// </summary>
        private const string ARGUMENT_LENGTH = "The argument provided has a length of ";

        /// <summary>
        /// Exception message descriptive text 
        /// Used when attempting to set a Vectors magnitude to a negative value 
        /// </summary>
        private const string NEGATIVE_MAGNITUDE = "The magnitude of a Vector3 must be a positive value, (i.e. greater than 0)";

        /// <summary>
        /// Exception message descriptive text 
        /// Used when attempting to set a Vectors magnitude where the Vector3 represents the origin
        /// </summary>
        private const string ORIGIN_VECTOR_MAGNITUDE = "Cannot change the magnitude of Vector3(0,0,0)";

        ///////////////////////////////////////////////////////////////////////////////

        private const string UNIT_VECTOR = "Unit vector composing of ";

        private const string POSITIONAL_VECTOR = "Positional vector composing of  ";

        private const string MAGNITUDE = " of magnitude ";

        ///////////////////////////////////////////////////////////////////////////////

        #endregion

        #region Constants

        /// <summary>
        /// The tolerence used when determining the equality of two vectors 
        /// </summary>
        public const double EqualityTolerence = Double.Epsilon;

        /// <summary>
        /// The smallest vector possible (based on the double precision floating point structure)
        /// </summary>
        public static readonly Vector3 MinValue = new Vector3(Double.MinValue, Double.MinValue, Double.MinValue);

        /// <summary>
        /// The largest vector possible (based on the double precision floating point structure)
        /// </summary>
        public static readonly Vector3 MaxValue = new Vector3(Double.MaxValue, Double.MaxValue, Double.MaxValue);

        /// <summary>
        /// The smallest positive (non-zero) vector possible (based on the double precision floating point structure)
        /// </summary>
        public static readonly Vector3 Epsilon = new Vector3(Double.Epsilon, Double.Epsilon, Double.Epsilon);

        #endregion
    }
}
