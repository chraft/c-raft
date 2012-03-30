/*  Minecraft NBT reader
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
using System.IO;
using System.Threading;

namespace Chraft.Utilities.NBT
{
    /// <summary>
    /// A Minecraft region file.
    /// </summary>
    public class RegionFile
    {
        /// <summary>
        /// The maximum threads the region reader will allocate (use 2^n values).
        /// </summary>
        public static int MaxTHREADS = 4;

        bool[] chunkChanged;

        NBTFile[] chunks;
        /// <summary>
        /// The chunks of the region file.
        /// </summary>
        public NBTFile[] Content
        {
            get { return this.chunks; }
        }

        McrOffset[] offsets;
        McrtStamp[] tstamps;

        /// <summary>
        /// Gets a chunk from this region.
        /// </summary>
        /// <param name="point">The location of the chunk.</param>
        /// <returns>An NBT file that has the </returns>
        public NBTFile this[MCPoint point]
        {
            get
            {
                return this.chunks[point.X + point.Y * 32];
            }
            set
            {
                InsertChunk(point, value);
            }
        }

        private RegionFile()
        {
            chunks = new NBTFile[1024];

            offsets = new McrOffset[1024];
            tstamps = new McrtStamp[1024];

            chunkChanged = new bool[1024];
        }

        /// <summary>
        /// Inserts/replaces a new chunk on a specified location.
        /// </summary>
        /// <param name="location">The region location of the chunk.</param>
        /// <param name="chunk">The chunk to be added.</param>
        public void InsertChunk(MCPoint location, NBTFile chunk)
        {
            int offset = location.X + (location.Y * 32);

            chunks[offset] = chunk;

            chunkChanged[offset] = true;
        }
        /// <summary>
        /// Removes a chunk on a specified location.
        /// </summary>
        /// <param name="location">The region location of the chunk to be removed.</param>
        public void RemoveChunk(MCPoint location)
        {
            int offset = location.X + (location.Y * 32);

            chunks[offset] = null;

            if (chunks[offset] != null)
                chunkChanged[offset] = true;
        }

        /// <summary>
        /// Saves the region file to a stream.
        /// </summary>
        /// <param name="stream">The stream the region file will write to.</param>
        public void SaveRegion(Stream stream)
        {

        }

        /// <summary>
        /// Opens the region file from a stream.
        /// </summary>
        /// <param name="stream">The stream the region file will read from.</param>
        /// <returns>The parsed region file.</returns>
        public static RegionFile OpenRegion(Stream stream)
        {
#if DEBUG
            DateTime wStart;
#endif
            RegionFile region = new RegionFile();

            using (BinaryReader reader = new BinaryReader(stream))
            {
                int[] sectors = new int[1024];
                int[] tstamps = new int[1024];

                for (int i = 0; i < 1024; i++)
                    sectors[i] = reader.ReadInt32();

                for (int i = 0; i < 1024; i++)
                    tstamps[i] = reader.ReadInt32();

                Thread offsetThread = new Thread(new ThreadStart(() =>
                {
                    int sector = 0;

                    lock (sectors)
                        for (int i = 0; i < 1024; i++)
                        {
                            sector = EndiannessConverter.ToInt32(sectors[i]);

                            region.offsets[i] = new McrOffset()
                            {
                                SectorSize = (byte)(sector & 0xFF),
                                SectorOffset = sector >> 8,
                            };
                        }

                    sectors = null;
                }));
                offsetThread.Name = "offset calculator thread";
                offsetThread.Start();

                offsetThread.Join();

                Thread tstampThread = new Thread(() =>
                                                     {
                                                         int tstamp = 0;
                                                         lock (tstamps)
                                                             for (int i = 0; i < 1024; i++)
                                                             {
                                                                 tstamp = EndiannessConverter.ToInt32(tstamps[i]);
                                                                 region.tstamps[i] = new McrtStamp
                                                                                         {
                                                                                             Timestamp = tstamp,
                                                                                         };
                                                             }

                                                         tstamps = null;
                                                     }) {Name = "timestamp calculator thread"};
                tstampThread.Start();
                tstampThread.Join();
#if DEBUG
                wStart = DateTime.Now;
#endif
                byte[][] chunkBuffer = new byte[1024][];
                {
                    int length;
                    McrOffset offset;

                    for (int i = 0; i < 1024; i++)
                    {
                        offset = region.offsets[i];

                        if (offset.SectorOffset != 0)
                        {
                            stream.Seek(offset.SectorOffset * 4096, SeekOrigin.Begin);

                            length = EndiannessConverter.ToInt32(reader.ReadInt32());
                            reader.ReadByte();

                            chunkBuffer[i] = reader.ReadBytes(length - 1);
                        }
                    }
                }

                int chunkSlice = 1024 / MaxTHREADS;
                Thread[] workerThreads = new Thread[MaxTHREADS];
                {

                    for (int i = 0; i < MaxTHREADS; i++)
                    {
                        byte[][] chunkWorkerBuffer = new byte[chunkSlice][];
                        Array.Copy(chunkBuffer, i * chunkSlice, chunkWorkerBuffer, 0, chunkSlice);

                        int index = i;

                        workerThreads[i] = new Thread(new ThreadStart(() =>
                        {
#if DEBUG
                            DateTime start = DateTime.Now;
#endif

                            int offset = index * (1024 / MaxTHREADS);
                            MemoryStream mmStream = null;

                            for (int n = 0; n < chunkWorkerBuffer.Length; n++)
                            {
                                byte[] chunk = chunkWorkerBuffer[n];

                                if (chunk == null)
                                    continue;

                                mmStream = new MemoryStream(chunk);
                                region.chunks[n + offset] = NBTFile.OpenFile(mmStream, 2);
                                mmStream.Dispose();
                            }

                            chunkWorkerBuffer = null;

#if DEBUG
                            Console.WriteLine("Thread worker " + (index + 1) + " is complete! Took " + (int)(DateTime.Now - start).TotalMilliseconds + "ms to process.");
                        }));

                        workerThreads[i].Name = "chunk worker thread " + (index + 1);
#else
                        }));
#endif
                        workerThreads[i].Start();
                    }
                    for (int i = 0; i < workerThreads.Length; i++)
                        workerThreads[i].Join();
                }
            }

#if DEBUG
            Console.WriteLine("\n=====================================================================");
            Console.WriteLine("Region Parse complete! Actual process time is " + (DateTime.Now - wStart).TotalMilliseconds + "ms.");
#endif

            return region;
        }
    }
}
