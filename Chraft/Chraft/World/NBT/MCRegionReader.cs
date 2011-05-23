using System;
using System.Text;
using System.IO;
using System.IO.Compression;
using System.Collections.Generic;


namespace Chraft.World.NBT
{
    /// <summary>
    /// Loader for Minecraft region files.
    /// </summary>
    public class MCRegionReader
    {
        private BinaryReader _bread = null;

        private int[] _offsets = new int[1024];

        private int[] _tStamps = new int[1024];


        /// <summary>
        /// Creates a new Region loader with the specified file location.
        /// </summary>
        /// <param name="fileIn">The file location of the region.</param>
        public MCRegionReader(FileStream fileIn)
        {
            _bread = new BinaryReader(fileIn);

            for (int i = 0; i < 1024; i++)
            {
                _offsets[i] = EndianConverter.SwapInt32(_bread.ReadInt32());
            }

            for (int i = 0; i < 1024; i++)
            {
                _tStamps[i] = EndianConverter.SwapInt32(_bread.ReadInt32());
            }
        }


        /// <summary>
        /// Gets the chunk inside the region with the specified coordinates.
        /// </summary>
        /// <param name="x">The abscissa of the region.</param>
        /// <param name="z">The ordinate of the region.</param>
        /// <returns>Returns a TagNodeListNamed that contains the chunk data.</returns>
        public INBTTag GetChunkData(int x, int z)
        {
            try
            {
                // check if the chunk position is out of the physical bounds of the region
                if (IsOutOfBounds(x, z))
                {
                    // if so, throw an OutOfRangeException to the user
                    throw new ArgumentOutOfRangeException();
                }

                // get the chunk offset of the chunk
                int offset = GetChunkOffset(x, z);

                // check if the offset contains chunk data
                if (!IsOffsetHasChunk(x, z))
                    // if not, return an empty chunk file that has a name of "Empty chunk"
                    return new TagNode(TagNodeType.TAG_COMPOUND, "Empty chunk", null);

                // move the byte order of the offset by 8 to the right
                int sectornumber = offset >> 8;
                // get the last two bytes of the offset (not sure about this :P)
                int nosofsectors = offset & 0xFF;

                // reposition the stream cursor to the start position of the chunk
                _bread.BaseStream.Seek(sectornumber * 4096, SeekOrigin.Begin);

                // then get the size of the chunk
                int chunklength = EndianConverter.SwapInt32(_bread.ReadInt32());

                // if the size of the chunk is GREATER than 4096 bytes
                if (chunklength > 4096 * nosofsectors)
                    // throw an OutOfRangeException
                    throw new ArgumentOutOfRangeException();

                // get the compression version of the chunk
                byte chunkVersion = _bread.ReadByte();

                // create a new NBT reader with the specified information
                NBTReader _nbtRead = new NBTReader(new MemoryStream(_bread.ReadBytes(chunklength - 1)), (int)chunkVersion);

                // read the chunk :D
                return _nbtRead.BeginRead();
            }
            catch (IOException ex)
            {
                Console.WriteLine("An error occurred while reading the region.\n\n");

                Console.WriteLine("Exception message: {0}\n\n", ex.Message);

                Console.WriteLine("Stack trace:\n\n{0}", ex.StackTrace);

                return null;
            }
        }

        /// <summary>
        /// Gets all the chunk stored inside the region.
        /// </summary>
        /// <returns>Returns the chunks stored inside the chunk.</returns>
        public INBTTag[,] GetRegionChunks()
        {
            // create a 2D array that will store the chunks
            INBTTag[,] _chunks = new INBTTag[32, 32];
            int i = 0;
            for (int x = 0; x < 32; x++)
                for (int z = 0; z < 32; z++)
                {
                    // get the chunk location of the chunk with the specified coordinates
                    _chunks[x, z] = GetChunkData(x, z);

                    // debug information
#if DEBUG
                    if (_chunks[x, z].Name != "Empty chunk")
                        Console.ForegroundColor = ConsoleColor.Green;
                    else
                        Console.ForegroundColor = ConsoleColor.Red;

                    Console.WriteLine("{1}:\tChunk parse complete! {0}", _chunks[x, z].Name, ++i);
#endif
                }

#if DEBUG
            Console.ForegroundColor = ConsoleColor.White;
#endif

            return _chunks;
        }


        private int GetChunkOffset(int x, int z)
        {
            return _offsets[x + z * 32];
        }

        private bool IsOutOfBounds(int x, int z)
        {
            return x < 0 || x >= 32 || z < 0 || z >= 32;
        }

        private bool IsOffsetHasChunk(int x, int z)
        {
            return GetChunkOffset(x, z) != 0;
        }
    }
}