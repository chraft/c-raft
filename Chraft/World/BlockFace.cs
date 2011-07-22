using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.World
{
    public enum BlockFace : sbyte
    {
        Self = -1,
        Held = -1,
        Down = 0,
        Up = 1,
        East = 2,
        West = 3,
        North = 4,
        South = 5,
        NorthEast = 6,
        NorthWest = 7,
        SouthEast = 8,
        SouthWest = 9
    }

}