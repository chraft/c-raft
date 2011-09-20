using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        public int getNibble(int x, int y, int z)
        {
            int l = x << 11 | z << 7 | y;
            int i1 = l >> 1;
            int j1 = l & 1;
            if(j1 == 0)
            {
                return Data[i1] & 0xf;
            } else
            {
                return Data[i1] >> 4 & 0xf;
            }
        }

        public void setNibble(int x, int y, int z, int value)
        {
            int i1 = x << 11 | z << 7 | y;
            int j1 = i1 >> 1;
            int k1 = i1 & 1;
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
