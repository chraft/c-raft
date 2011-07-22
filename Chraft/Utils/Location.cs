using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Utils
{
   public class Location
    {
       public double X { get; set; }
       public double Y { get; set; }
       public double Z { get; set; }
       public float Pitch { get; set; }
       public float Yaw { get; set; }

       /// <summary>
       /// Creates a new location
       /// </summary>
       /// <param name="x">the x co-ord</param>
       /// <param name="y">the y co-ord (height)</param>
       /// <param name="z">the z co-ord</param>
       /// <param name="pitch">the x plane rotation</param>
       /// <param name="yaw">the y plane rotation</param>
       public Location(double x, double y, double z, float pitch, float yaw)
       {
           X = x;
           Y = y;
           Z = z;
           Pitch = pitch;
           Yaw = yaw;
       }
    }
}
