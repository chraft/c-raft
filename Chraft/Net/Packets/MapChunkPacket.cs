using Chraft.World;
using java.util.zip;

namespace Chraft.Net.Packets
{
	public class MapChunkPacket : Packet
	{
		public int X { get { return (Chunk.X << 4); } }
		public short Y { get { return 0; } }
		public int Z { get { return (Chunk.Z << 4); } }
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
			Chunk = new Chunk(null, posX << 4, posZ << 4);

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
            Writer.Write(X);
            Writer.Write(Y);
            Writer.Write(Z);
            Writer.Write(SizeX);
            Writer.Write(SizeY);
            Writer.Write(SizeZ);

            Writer.Write(len);
            Writer.Write(comp, 0, len);
		}
	}
}
