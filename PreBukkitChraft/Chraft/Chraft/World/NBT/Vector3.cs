using System;

namespace Chraft.World.NBT {
    public class Vector3 {

        private double x = 0;
        private double y = 0;
        private double z = 0;
        private int scale = 1;

        // X component of the vector.
        public double X {
            get {
                return x * scale;
            }
            set { x = value; }
        }

        // Y component of the vector.
        public double Y { 
            get {
                return y * scale;
            }
            set { y = value; }
        }

        // Z component of the vector.
        public double Z {
            get {
                return z * scale;
            }
            set { z = value; }
        }

        // Length of the vector
        // If scale is 2 and x is 3, then x will return 6.
        public int Scale {
            get {
                return scale;
            }
            set { scale = value; }
        } // Default 1

        //
        // Constructors
        //
        public Vector3(double xin, double yin, double zin) {
            X = xin;
            Y = yin;
            Z = zin;
        }

        public Vector3(double xin, double yin, double zin, int scalein) {
            X = xin;
            Y = yin;
            Z = zin;
            Scale = scalein;
        }
    }
}
