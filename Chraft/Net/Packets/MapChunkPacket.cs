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
using Chraft.PluginSystem;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.World;
using ICSharpCode.SharpZipLib.Zip.Compression;

namespace Chraft.Net.Packets
{
    public class MapChunkPacket : Packet
    {
        public UniversalCoords Coords { get { return Chunk.Coords; } }
        /*public byte SizeX { get { return 15; } }
        public byte SizeY { get { return 127; } }
        public byte SizeZ { get { return 15; } }*/
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
            int dataDim = (Chunk.SectionsNum * Section.BYTESIZE) + 256; // (Number of sections * (Section dimension + Add array) + Biome array

            byte[] data = new byte[dataDim];

            /*byte[] types = new byte[0];
            byte[] metadata = new byte[0];
            byte[] blockLight = new byte[0];
            byte[] skyLight = new byte[0];

            byte[] tempBlockLight = new byte[2048];
            byte[] tempSkyLight = new byte[2048];

            int primaryBitMask = 0;
            ushort mask = 1;

            for(int i = 0; i < 16; ++i)
            {
                Section currentSection = Chunk.Sections[i];
                if(currentSection != null && currentSection.NonAirBlocks > 0)
                {
                    types = currentSection.Types.Concat(types).ToArray();
                    metadata = currentSection.Data.Data.Concat(metadata).ToArray();

                    Buffer.BlockCopy(Chunk.Light.Data, i * Section.HALFSIZE, tempBlockLight, 0, Section.HALFSIZE);
                    Buffer.BlockCopy(Chunk.SkyLight.Data, i * Section.HALFSIZE, tempSkyLight, 0, Section.HALFSIZE);

                    blockLight = tempBlockLight.Concat(blockLight).ToArray();
                    skyLight = tempSkyLight.Concat(skyLight).ToArray();

                    primaryBitMask |= mask;
                }


                mask <<= 1;
            }

            byte[] data = types.Concat(metadata).Concat(blockLight).Concat(skyLight).ToArray();*/

            int halfSize = Chunk.SectionsNum * Section.HALFSIZE;
            int offsetData = Chunk.SectionsNum * Section.SIZE;
            int offsetLight = offsetData + halfSize;
            int offsetSkyLight = offsetLight + halfSize;

            int primaryBitMask = 0;
            ushort mask = 1;
            int sectionIndex = 0;
            for (int i = 0; i < 16; ++i)
            {
                Section currentSection = Chunk.Sections[i];

                //int typeIndex = i*Section.SIZE;
                
                if(currentSection != null && currentSection.NonAirBlocks > 0)
                {
                    Buffer.BlockCopy(currentSection.Types, 0, data, sectionIndex * Section.SIZE, Section.SIZE);
                    Buffer.BlockCopy(currentSection.Data.Data, 0, data, offsetData + (sectionIndex * Section.HALFSIZE),
                                     Section.HALFSIZE);

                    Buffer.BlockCopy(Chunk.Light.Data, i * Section.HALFSIZE, data, offsetLight + (sectionIndex * Section.HALFSIZE),
                                     Section.HALFSIZE);
                    Buffer.BlockCopy(Chunk.SkyLight.Data, i * Section.HALFSIZE, data, offsetSkyLight + (sectionIndex * Section.HALFSIZE),
                                     Section.HALFSIZE);

                    
                    primaryBitMask |= mask;
                    ++sectionIndex;  
                }

                

                mask <<= 1;

                // TODO: we leave add array and biome array to 0 (ocean), we need to change the chunk generator accordingly
            }

            /*for (int j = 0; j < Chunk.SkyLight.Data.Length; ++j)
            {
                Logger.Log(LogLevel.Info, "-" + Chunk.SkyLight.Data[j]);
            }
            Console.WriteLine(" ");*/
            /*Chunk.Types.CopyTo(data, i);
            i += Chunk.Types.Length;

            Chunk.Data.Data.CopyTo(data, i);
            i += Chunk.Data.Data.Length;

            Chunk.Light.Data.CopyTo(data, i);
            i += Chunk.Light.Data.Length;

            Chunk.SkyLight.Data.CopyTo(data, i);*/

            byte[] comp = new byte[data.Length];
            int len;

#if PROFILE_MAPCHUNK
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
            
#if PROFILE_MAPCHUNK
            DateTime end = DateTime.Now;
            TimeSpan deflateTime = end - start;
            int deflateLength = len;
            Console.WriteLine("1: {0}->{1}bytes in {2}ms", data.Length, deflateLength, deflateTime);
#endif

            SetCapacity(22 + len);
            Writer.Write(Coords.ChunkX);
            Writer.Write(Coords.ChunkZ);
            Writer.Write(false); // Ground Up Continous
            Writer.Write((ushort)primaryBitMask);
            Writer.Write((ushort)0); // Add BitMask
            Writer.Write(len);
            Writer.Write(0); // Unused
            Writer.Write(comp, 0, len);

#if PROFILE_MAPCHUNK
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
