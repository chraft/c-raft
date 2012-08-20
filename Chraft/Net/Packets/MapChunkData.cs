using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Net.Packets
{
    
    public class MapChunkData
    {
        public byte[] Data { get; set; }
        public int PrimaryBitMask { get; set; }
        public int AddBitMask { get; set; }
    }
}
