﻿/*  Minecraft NBT reader
 * 
 *  Copyright 2010-2011 Michael Ong, all rights reserved.
 *  
 *  This program is free software; you can redistribute it and/or
 *  modify it under the terms of the GNU General Public License
 *  as published by the Free Software Foundation; either version 2
 *  of the License, or (at your option) any later version.
 *  
 *  This program is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *  
 *  You should have received a copy of the GNU General Public License
 *  along with this program; if not, write to the Free Software
 *  Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.
 */

using System;

namespace Chraft.Utilities.NBT
{
    /// <summary>
    /// Provides helper methods to convert between byte endianness.
    /// </summary>
    public class EndiannessConverter
    {
        public static short ToInt16(short value)
        {
            return (short)((value >> 8) | ((value << 8) & 0xFF));
        }
        public static int ToInt32(int value)
        {
            value = (int)((value << 8) & 0xFF00FF00) | (value >> 8 & 0xFF00FF);

            return (value << 16) | ((value >> 16) & 0xFFFF);
        }
        public static long ToInt64(long value)
        {
            byte[] reverse = BitConverter.GetBytes(value);
            Array.Reverse(reverse);

            return BitConverter.ToInt64(reverse, 0);
        }

        public static float ToSingle(float value)
        {
            byte[] reverse = BitConverter.GetBytes(value);
            Array.Reverse(reverse);

            return BitConverter.ToSingle(reverse, 0);
        }
        public static double ToDouble(double value)
        {
            byte[] reverse = BitConverter.GetBytes(value);
            Array.Reverse(reverse);

            return BitConverter.ToDouble(reverse, 0);
        }
    }
}
