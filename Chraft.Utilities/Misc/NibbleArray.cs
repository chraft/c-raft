#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion

namespace Chraft.Utilities.Misc
{
    public class NibbleArray
    {
        public byte[] Data;
        public NibbleArray(int i)
        {
            Data = new byte[i];
        }

        public NibbleArray(byte[] data)
        {
            Data = data;
        }

        public int getNibble(int blockX, int blockY, int blockZ)
        {

            return getNibble(blockY << 8 | blockZ << 4 | blockX);
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
            setNibble(blockY << 8 | blockZ << 4 | blockX, value);
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
