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
using System.Collections.Concurrent;
using System.Linq;
using System.Collections.Generic;
using Chraft.World;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Chraft.Net.Packets
{
    public class MapChunkPacket : Packet
    {
        private const int DataDimension = 81920;
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
            stream.ReadBytes(len);
        }

#if PROFILE
        private static System.Collections.Generic.List<int> writeDiffsLength = new List<int>();
        private static System.Collections.Generic.List<double> writeDiffsTime = new List<double>();
#endif
        
        // Compression level 5 gives 0.3ms faster than the Java version, and exact same size
        // Using the static instance gives another 0.3ms
        //private static readonly ICSharpCode.SharpZipLib.Zip.Compression.Deflater Deflater = new ICSharpCode.SharpZipLib.Zip.Compression.Deflater(5);

        public static ConcurrentStack<Deflater> DeflaterPool = new ConcurrentStack<Deflater>();

        public override void Write()
        {
            byte[] data = new byte[DataDimension];

            int i = 0;

            Chunk.Types.CopyTo(data, i);
            i += Chunk.Types.Length;

            Chunk.Data.Data.CopyTo(data, i);
            i += Chunk.Data.Data.Length;

            Chunk.Light.Data.CopyTo(data, i);
            i += Chunk.Light.Data.Length;

            Chunk.SkyLight.Data.CopyTo(data, i);

            byte[] comp = new byte[DataDimension];
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

            Deflater deflater;
            DeflaterPool.TryPop(out deflater);

            if(deflater == null)
                deflater = new Deflater(5);

            deflater.SetInput(data);
            deflater.Finish();
            len = deflater.Deflate(comp);
            deflater.Reset();

            DeflaterPool.Push(deflater);
            
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
            comp = new byte[DataDimension];

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
