using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.World;

namespace Chraft
{
    public class NibbleArray
    {
        public byte[] Data;
        public NibbleArray(int i)
        {
            Data = new byte[i >> 1];
        }

        public NibbleArray(byte[] data)
        {
            Data = data;
        }

        public int getNibble(int blockX, int blockY, int blockZ)
        {
            return getNibble(blockX << 11 | blockZ << 7 | blockY);
        }

        public int getNibble(int packed)
        {
            int i1 = packed >> 1;
            int j1 = packed & 1;
            if(j1 == 0)
            {
                return Data[i1] & 0xf;
            } else
            {
                return Data[i1] >> 4 & 0xf;
            }
        }

        public void setNibble(int blockX, int blockY, int blockZ, byte value)
        {
            setNibble(blockX << 11 | blockZ << 7 | blockY, value);
        }

        public void setNibble(int packed, byte value)
        {
            int j1 = packed >> 1;
            int k1 = packed & 1;
            if(k1 == 0)
            {
                Data[j1] = (byte)(Data[j1] & 0xf0 | value & 0xf);
            } else
            {
                 Data[j1] = (byte)(Data[j1] & 0xf | (value & 0xf) << 4);
            }
        }

        public bool isValid()
        {
            return Data != null;
        }

        
    }
}
