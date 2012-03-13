using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Utilities
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
}
