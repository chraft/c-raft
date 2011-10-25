using System;
using Chraft.World;
using java.util.zip;

namespace Chraft.Net.Packets
{
	public class MapChunkPacket : Packet
	{
        public UniversalCoords Coords { get { return Chunk.Coords; } }
		public byte SizeX { get { return 15; } }
		public byte SizeY { get { return 127; } }
		public byte SizeZ { get { return 15; } }
		public Chunk Chunk { get; set; }

		public override void Read(PacketReader stream)
		{
			int posX = stream.ReadInt();
			short posY = stream.ReadShort();
			int posZ = stream.ReadInt();
			byte sizeX = (byte)(stream.ReadByte() + 1);
			byte sizeY = (byte)(stream.ReadByte() + 1);
			byte sizeZ = (byte)(stream.ReadByte() + 1);

			int o = sizeX * sizeY * sizeZ;
            Chunk = new Chunk(null, UniversalCoords.FromWorld(posX, posY, posZ));

		    int len = stream.ReadInt();
			byte[] data = new byte[o * 5 / 2];
			byte[] comp = stream.ReadBytes(len);
		}

        public override void Write()
		{
            int o = 16 * 16 * 128;
            byte[] data = new byte[o * 5 / 2];

            int i = 0;

            Chunk.Types.CopyTo(data, i);
            i += Chunk.Types.Length;

            Chunk.Data.Data.CopyTo(data, i);
            i += Chunk.Data.Data.Length;

            Chunk.Light.Data.CopyTo(data, i);
            i += Chunk.Light.Data.Length;

            Chunk.SkyLight.Data.CopyTo(data, i);

            byte[] comp = new byte[o * 5 / 2];
            int len;

            Deflater deflater = new Deflater(-1);
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

            SetCapacity(18 + len);
            Writer.Write(Coords.WorldX);
            Writer.Write((short)0);
            Writer.Write(Coords.WorldZ);
            Writer.Write(SizeX);
            Writer.Write(SizeY);
            Writer.Write(SizeZ);

            Writer.Write(len);
            Writer.Write(comp, 0, len);
		}
	}
}
