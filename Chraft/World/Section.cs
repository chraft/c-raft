using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;
using Chraft.World.Blocks;

namespace Chraft.World
{
    public class Section
    {
        private Chunk _Parent;
        public static readonly int HALFSIZE = 16*16*8;
        public static readonly int SIZE = HALFSIZE*2;
        public static readonly int BYTESIZE = SIZE + SIZE + HALFSIZE;
        
        internal NibbleArray Data = new NibbleArray(HALFSIZE);
        
        internal byte[] Types = new byte[SIZE];

        private int _NonAirBlocks;

        public int NonAirBlocks
        {
            get { return _NonAirBlocks; }
            set { _NonAirBlocks = value; }
        }

        public Section(Chunk parent)
        {
            _Parent = parent;
            _NonAirBlocks = 0;
        }

        internal unsafe byte this[UniversalCoords coords]
        {
            get
            {
                fixed (byte* types = Types)
                    return types[coords.SectionPackedCoords];
            }
            set
            {
                fixed (byte* types = Types)
                {
                    byte oldValue = types[coords.SectionPackedCoords];
                    if (oldValue != value)
                    {
                        //if (value != (byte)BlockData.Blocks.Air)
                        types[coords.SectionPackedCoords] = value;
                        if (value == (byte)BlockData.Blocks.Air)
                        {
                            if (_NonAirBlocks > 0)
                                --_NonAirBlocks;
                        }
                        else
                            ++_NonAirBlocks;
                                
                    }
                }
            }
        }

        internal unsafe byte this[int blockIndex]
        {
            get
            {
                fixed (byte* types = Types)
                    return types[blockIndex];
            }
            set
            {
                fixed (byte* types = Types)
                {
                    byte oldValue = types[blockIndex];
                    if (oldValue != value)
                    {
                        

                        //if (value != (byte)BlockData.Blocks.Air)
                        types[blockIndex] = value;
                        if (value == (byte)BlockData.Blocks.Air)
                        {
                            if (_NonAirBlocks > 0)
                                --_NonAirBlocks;
                        }
                        else
                            ++_NonAirBlocks;
                    }
                }
            }
        }
    }
}
