using Chraft.Utilities.Blocks;
using Chraft.Utilities.Collision;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;

namespace Chraft.World
{
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
}
