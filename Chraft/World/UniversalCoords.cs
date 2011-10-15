using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utils;

namespace Chraft.World
{
    public struct UniversalCoords
    {
        public readonly int WorldX;
        public readonly byte WorldY;
        public readonly int WorldZ;

        public int BlockX { get { return WorldX & 0XF; } }
        public int BlockY { get { return WorldY; } }
        public int BlockZ { get { return WorldZ & 0xF; } }

        public int ChunkX { get { return WorldX >> 4; } }
        public int ChunkZ { get { return WorldZ >> 4; } }

        public readonly short BlockPackedCoords;
        public readonly int ChunkPackedCoords;

        private UniversalCoords(int worldX, int worldY, int worldZ)
        {
            WorldX = worldX;
            WorldY = (byte)worldY;
            WorldZ = worldZ;

            int chunkX = worldX >> 4;
            int chunkZ = worldZ >> 4;

            BlockPackedCoords = (short)((worldX & 0xF) << 11 | (worldZ & 0xF) << 7 | worldY);
            ChunkPackedCoords = (short)chunkX << 16 | (short)chunkZ & 0xFFFF;
        }

        private UniversalCoords(int chunkX, int chunkZ, int blockX, int blockY, int blockZ)
        {
            WorldX = (chunkX << 4) + blockX;
            WorldY = (byte)blockY;
            WorldZ = (chunkZ << 4) + blockZ;

            BlockPackedCoords = (short)(blockX << 11 | blockZ << 7 | blockY);
            ChunkPackedCoords = (short)chunkX << 16 | (short)chunkZ & 0xFFFF;
        }

        private UniversalCoords(int chunkX, int chunkZ)
        {
            WorldX = chunkX << 4;
            WorldY = 0;
            WorldZ = chunkZ << 4;

            BlockPackedCoords = 0;
            ChunkPackedCoords = (short)chunkX << 16 | (short)chunkZ & 0xFFFF;
        }

        private UniversalCoords(int packedChunk)
        {
            WorldX = (short)(packedChunk >> 12);
            WorldY = 0;
            WorldZ = (short)((packedChunk & 0xFFFF) << 4);

            BlockPackedCoords = 0;
            ChunkPackedCoords = packedChunk;
        }
  
        /// <summary>
        /// Execute the action for each adjacent coordinate in the order: South, North, Down, Up, East, West.
        /// </summary>
        /// <param name='action'>
        /// Action.
        /// </param>
        public void ForAdjacent(Action<UniversalCoords, Direction> action)
        {
            action(UniversalCoords.FromWorld(this.WorldX - 1, this.WorldY, this.WorldZ), Direction.South);
            action(UniversalCoords.FromWorld(this.WorldX + 1, this.WorldY, this.WorldZ), Direction.North);
            if (this.BlockY > 0)
                action(UniversalCoords.FromWorld(this.WorldX, this.WorldY - 1, this.WorldZ), Direction.Down);
            if (this.BlockY < 127)
                action(UniversalCoords.FromWorld(this.WorldX, this.WorldY + 1, this.WorldZ), Direction.Up);
            action(UniversalCoords.FromWorld(this.WorldX, this.WorldY, this.WorldZ - 1), Direction.East);
            action(UniversalCoords.FromWorld(this.WorldX, this.WorldY, this.WorldZ + 1), Direction.West);
        }
        
        public override string ToString()
        {
            return string.Format("[UniversalCoords: WorldX={0}, WorldY={1}, WorldZ={2}]", WorldX, WorldY, WorldZ);
        }
                    
        public static UniversalCoords FromWorld(int worldX, int worldY, int worldZ)
        {
            return new UniversalCoords(worldX, worldY, worldZ);
        }

        public static UniversalCoords FromAbsWorld(double worldX, double worldY, double worldZ)
        {
            return new UniversalCoords((int)Math.Floor(worldX), (int)Math.Floor(worldY), (int)Math.Floor(worldZ));
        }
        
        public static UniversalCoords FromAbsWorld(AbsWorldCoords absWorldCoords)
        {
            return FromAbsWorld(absWorldCoords.X, absWorldCoords.Y, absWorldCoords.Z);
        }

        public static UniversalCoords FromBlock(int chunkX, int chunkZ, int blockX, int blockY, int blockZ)
        {
            return new UniversalCoords(chunkX, chunkZ, blockX, blockY, blockZ);
        }

        public static UniversalCoords FromChunk(int chunkX, int chunkZ)
        {
            return new UniversalCoords(chunkX, chunkZ);
        }

        public static UniversalCoords FromPackedChunk(int packedChunk)
        {
            return new UniversalCoords(packedChunk);
        }

        public static AbsWorldCoords ToAbsWorld(UniversalCoords coords)
        {
            return new AbsWorldCoords(coords);
        }

        public static int FromChunkToPackedChunk(int chunkX, int chunkZ)
        {
            return (short)chunkX << 16 | (short)chunkZ & 0xFFFF;
        }

        public static int FromPackedChunkToX(int packedChunk)
        {
            return (short)(packedChunk >> 16);
        }

        public static int FromPackedChunkToZ(int packedChunk)
        {
            return (short)(packedChunk & 0xFFFF);
        }
    }
}
