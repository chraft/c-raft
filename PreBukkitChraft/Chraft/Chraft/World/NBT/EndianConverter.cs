using System;
using System.Collections.Generic;
using System.Text;

namespace Chraft.World.NBT
{
    public static class EndianConverter
    {
        public static Int16 SwapInt16(Int16 value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToInt16(cVal, 0);
        }

        public static Int32 SwapInt32(Int32 value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToInt32(cVal, 0);
        }

        public static Int64 SwapInt64(Int64 value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToInt64(cVal, 0);
        }

        public static Single SwapSingle(Single value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToSingle(cVal, 0);
        }

        public static Double SwapDouble(Double value)
        {
            byte[] cVal = BitConverter.GetBytes(value);

            Array.Reverse(cVal);

            return BitConverter.ToDouble(cVal, 0);
        }
    }
}
