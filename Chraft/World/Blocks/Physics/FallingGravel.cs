using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utils;

namespace Chraft.World.Blocks.Physics
{
    public class FallingGravel : FallingSand
    {
        public FallingGravel(WorldManager world, AbsWorldCoords pos) : base(world, pos)
        {
            Type = Net.Packets.AddObjectVehiclePacket.ObjectType.FallingGravel;
            BlockId = (byte) BlockData.Blocks.Gravel;
        }
    }
}
