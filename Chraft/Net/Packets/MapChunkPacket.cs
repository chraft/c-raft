using System;
using System.Linq;
using System.Collections.Generic;
using Chraft.World;

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

#if PROFILE
        private static System.Collections.Generic.List<int> writeDiffsLength = new List<int>();
        private static System.Collections.Generic.List<double> writeDiffsTime = new List<double>();
#endif

        private static object _deflaterLock = new object();
        
        // Compression level 5 gives 0.3ms faster than the Java version, and exact same size
        // Using the static instance gives another 0.3ms
        private static readonly ICSharpCode.SharpZipLib.Zip.Compression.Deflater Deflater = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(5);

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
            i += Chunk.SkyLight.Data.Length;

            byte[] comp = new byte[o * 5 / 2];
            int len;

#if PROFILE
            DateTime start = DateTime.Now;
#endif
            // The ZlibStream gives up to 3ms faster with approx. 10bytes larger, but currently has a bug
            // need to reproduce and post on codeplex DotNetZip site.

            //comp = ZlibStream.CompressBuffer(data);
            //len = comp.Length;
            //try
            //{
            //    byte[] testout = ZlibStream.UncompressBuffer(comp);
            //}
            //catch (Exception)
            //{
            //    // TODO: add some code to generate test case for DotNetZip issue
            //    throw;
            //}
            
            // Compression level 5 gives 0.3ms faster than the Java version, and exact same size
            // Using the static instance gives another 0.3ms
            lock (_deflaterLock)
            {
                Deflater.SetInput(data);
                Deflater.Finish();
                len = Deflater.Deflate(comp);
                Deflater.Reset();
            }
            
#if PROFILE
            DateTime end = DateTime.Now;
            TimeSpan deflateTime = end - start;
            int deflateLength = len;
            Console.WriteLine("1: {0}->{1}bytes in {2}ms", data.Length, deflateLength, deflateTime);
#endif

            SetCapacity(18 + len);
            Writer.Write(Coords.WorldX);
            Writer.Write((short)0);
            Writer.Write(Coords.WorldZ);
            Writer.Write(SizeX);
            Writer.Write(SizeY);
            Writer.Write(SizeZ);

            Writer.Write(len);
            Writer.Write(comp, 0, len);

#if PROFILE
            start = DateTime.Now;
            comp = new byte[o * 5 / 2];

            // Original Java compression version
            //java.util.zip.Deflater deflater = new java.util.zip.Deflater(-1);
            //try
            //{
            //    deflater.setInput(data);
            //    deflater.finish();
                
            //    len = deflater.deflate(comp);
            //}
            //finally
            //{
            //    deflater.end();
            //}
            end = DateTime.Now;
            TimeSpan jTime = end - start;
            int jLength = len;

            writeDiffsLength.Add(deflateLength - jLength);
            writeDiffsTime.Add(deflateTime.TotalMilliseconds - jTime.TotalMilliseconds);

            Console.WriteLine("Avg {0}, {1}", writeDiffsLength.Average(), writeDiffsTime.Average());
#endif
        }
    }
}
