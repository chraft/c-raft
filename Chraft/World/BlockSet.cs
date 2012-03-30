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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks;

namespace Chraft.World
{
	public class BlockSet
	{
		public const int SIZE = 16 * 16 * 128;
		public delegate void ForEachBlock(int x, int y, int z);

		protected byte[] Types = new byte[SIZE];
		protected byte[] Light = new byte[SIZE];
		protected byte[] Data = new byte[SIZE];

		public unsafe byte this[int x, int y, int z]
		{
			get
			{
				fixed (byte* types = Types)
					return types[Translate(x, y, z)];
			}
			set
			{
				fixed (byte* types = Types)
					types[Translate(x, y, z)] = value;
			}
		}

		private int Translate(int x, int y, int z)
		{
			return x << 11 | z << 7 | y;
		}

		public unsafe byte GetBlockLight(int x, int y, int z)
		{
			fixed (byte* light = Light)
				return unchecked((byte)(light[Translate(x, y, z)] >> 4 & 0xf));
		}

		public unsafe byte GetSkyLight(int x, int y, int z)
		{
			fixed (byte* light = Light)
				return unchecked((byte)(light[Translate(x, y, z)] & 0xf));
		}

		public unsafe byte GetData(int x, int y, int z)
		{
			fixed (byte* data = Data)
				return unchecked(data[Translate(x, y, z)]);
		}

		public unsafe byte GetDualLight(int x, int y, int z)
		{
			fixed (byte* light = Light)
				return light[Translate(x, y, z)];
		}

		public unsafe void SetBlockLight(int x, int y, int z, byte value)
		{
			int i = Translate(x, y, z);
			fixed (byte* light = Light)
				light[i] = unchecked((byte)(light[i] & 0xf | (value << 4)));
		}

		public unsafe void SetSkyLight(int x, int y, int z, byte value)
		{
			int i = Translate(x, y, z);
			fixed (byte* light = Light)
				light[i] = unchecked((byte)(light[i] & 0xf0 | value));
		}

		public unsafe void SetDualLight(int x, int y, int z, byte value)
		{
			fixed (byte* light = Light)
				light[Translate(x, y, z)] = value;
		}

		public unsafe void SetData(int x, int y, int z, byte value)
		{
			fixed (byte* data = Data)
				data[Translate(x, y, z)] = value;
		}

		public byte GetLuminance(int x, int y, int z)
		{
            return BlockHelper.Instance.Luminance(this[x, y, z]);
		}

		public byte GetOpacity(int x, int y, int z)
		{
            return BlockHelper.Instance.Opacity(this[x, y, z]);
		}

		public void SetAllBlocks(byte[] data)
		{
			Types = data;
		}

		public void ForAdjacentSameChunk(int x, int y, int z, ForEachBlock predicate)
		{
			if (x > 0)
				predicate(x - 1, y, z);
			if (x < 15)
				predicate(x + 1, y, z);
			if (y > 0)
				predicate(x, y - 1, z);
			if (y < 127)
				predicate(x, y + 1, z);
			if (z > 0)
				predicate(x, y, z - 1);
			if (z < 15)
				predicate(x, y, z + 1);
		}

		public void ForEach(ForEachBlock predicate)
		{
			for (int x = 0; x < 16; x++)
				for (int z = 0; z < 16; z++)
					for (int y = 0; y < 128; y++)
						predicate(x, y, z);
		}

		public BlockData.Blocks GetType(int x, int y, int z)
		{
			return (BlockData.Blocks)this[x, y, z];
		}

		public void SetType(int x, int y, int z, BlockData.Blocks value)
		{
			this[x, y, z] = (byte)value;
		}
	}
}
