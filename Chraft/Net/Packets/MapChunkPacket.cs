using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;
using Chraft.World;
using System.IO;
using System.IO.Compression;
using java.util.zip;

namespace Chraft.Net
{
	public class MapChunkPacket : Packet
	{
		public int X { get { return Chunk.X; } }
		public short Y { get { return 0; } }
		public int Z { get { return Chunk.Z; } }
		public byte SizeX { get { return 15; } }
		public byte SizeY { get { return 127; } }
		public byte SizeZ { get { return 15; } }
		public Chunk Chunk { get; set; }

		public override void Read(BigEndianStream stream)
		{
			int posX = stream.ReadInt();
			short posY = stream.ReadShort();
			int posZ = stream.ReadInt();
			byte sizeX = (byte)(stream.ReadByte() + 1);
			byte sizeY = (byte)(stream.ReadByte() + 1);
			byte sizeZ = (byte)(stream.ReadByte() + 1);

			int o = sizeX * sizeY * sizeZ;
			Chunk = new Chunk(null, posX, posZ);

			int len = stream.ReadInt();
			byte[] comp = new byte[len];
			byte[] data = new byte[o * 5 / 2];
			len = stream.Read(comp, 0, len);
		}

		public override void Write(BigEndianStream stream)
		{
			stream.Write(X);
			stream.Write(Y);
			stream.Write(Z);
			stream.Write(SizeX);
			stream.Write(SizeY);
			stream.Write(SizeZ);

			int o = 16 * 16 * 128;
			byte[] data = new byte[o * 5 / 2];

			int i = 0;
			for (int x = 0; x < 16; x++)
			{
				for (int z = 0; z < 16; z++)
				{
					for (int y = 0; y < 128; y++)
					{
						int s = ((i + 1) & 1) * 4;
						int ofst = i;
						data[ofst] = Chunk[x, y, z];
						ofst = i / 2 + o * 2 / 2;
						data[ofst] = unchecked((byte)(data[ofst] | (Chunk.GetData(x, y, z) << s)));
						ofst = i / 2 + o * 3 / 2;
						data[ofst] = unchecked((byte)(data[ofst] | (Chunk.GetBlockLight(x, y, z) << s)));
						ofst = i / 2 + o * 4 / 2;
						data[ofst] = unchecked((byte)(data[ofst] | (Chunk.GetSkyLight(x, y, z) << s)));
						i++;
					}
				}
			}

			byte[] comp = new byte[o * 5];
			int len;

			Deflater deflater = new Deflater(0);
			try
			{
				deflater.setInput(data);
				deflater.finish();
				len = deflater.deflate(comp);
			}
			finally
			{
				deflater.end();
			}

			stream.Write(len);
			stream.Write(comp, 0, len);
		}
	}
}
