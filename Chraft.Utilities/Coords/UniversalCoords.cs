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
using Chraft.Utilities.Misc;

namespace Chraft.Utilities.Coords
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


            BlockPackedCoords = (short)((worldY & 0xF) << 8 | (worldZ & 0xF) << 4 | (worldX & 0xF));
            ChunkPackedCoords = (short)chunkX << 16 | (short)chunkZ & 0xFFFF;
        }

        private UniversalCoords(int chunkX, int chunkZ, int blockX, int blockY, int blockZ)
        {
            WorldX = (chunkX << 4) + blockX;
            WorldY = (byte)blockY;
            WorldZ = (chunkZ << 4) + blockZ;

            BlockPackedCoords = (short)((blockY & 0xF) << 8 | blockZ << 4 | blockX);
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
        /// Determines whether a specified instance of <see cref="UniversalCoords"/> is equal to another specified <see cref="UniversalCoords"/>.
        /// </summary>
        /// <param name='left'>
        /// The first <see cref="UniversalCoords"/> to compare.
        /// </param>
        /// <param name='right'>
        /// The second <see cref="UniversalCoords"/> to compare.
        /// </param>
        /// <returns>
        /// <c>true</c> if <c>left</c> and <c>right</c> are equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator ==(UniversalCoords left, UniversalCoords right)
        {
            return left.WorldX == right.WorldX && left.WorldY == right.WorldY && left.BlockZ == right.WorldZ;
        }
        
        /// <summary>
        /// Determines whether a specified instance of <see cref="UniversalCoords"/> is not equal to another specified <see cref="UniversalCoords"/>.
        /// </summary>
        /// <param name='left'>
        /// The first <see cref="UniversalCoords"/> to compare.
        /// </param>
        /// <param name='right'>
        /// The second <see cref="UniversalCoords"/> to compare.
        /// </param>
        /// <returns>
        /// <c>true</c> if <c>left</c> and <c>right</c> are not equal; otherwise, <c>false</c>.
        /// </returns>
        public static bool operator !=(UniversalCoords left, UniversalCoords right)
        {
            return !(left == right);
        }
        
        /// <summary>
        /// Subtracts a <see cref="UniversalCoords"/> from a <see cref="UniversalCoords"/>, yielding a new <see cref="UniversalCoords"/>.
        /// </summary>
        /// <param name='left'>
        /// The <see cref="UniversalCoords"/> to subtract from (the minuend).
        /// </param>
        /// <param name='right'>
        /// The <see cref="UniversalCoords"/> to subtract (the subtrahend).
        /// </param>
        /// <returns>
        /// The <see cref="UniversalCoords"/> that is the <c>left</c> minus <c>right</c>.
        /// </returns>
        public static UniversalCoords operator -(UniversalCoords left, UniversalCoords right)
        {
            return UniversalCoords.FromWorld(left.WorldX - right.WorldX, left.WorldY - right.WorldY, left.WorldZ - right.WorldZ);
        }
  
        public static UniversalCoords operator +(UniversalCoords left, UniversalCoords right)
        {
            return UniversalCoords.FromWorld(left.WorldX + right.WorldX, left.WorldY + right.WorldY, left.WorldZ + right.WorldZ);
        }  
      
        /// <summary>
        /// The empty UniversalCoords (0,0,0).
        /// </summary>
        public static UniversalCoords Empty;
  
        /// <summary>
        /// Execute the action for each adjacent coordinate in the order: South, North, Down, Up, East, West.
        /// </summary>
        /// <param name='action'>
        /// Action.
        /// </param>
        public void ForAdjacent(Action<UniversalCoords, Direction> action)
        {
            action(UniversalCoords.FromWorld(this.WorldX + 1, this.WorldY, this.WorldZ), Direction.South);
            action(UniversalCoords.FromWorld(this.WorldX - 1, this.WorldY, this.WorldZ), Direction.North);
            if (this.BlockY > 0)
                action(UniversalCoords.FromWorld(this.WorldX, this.WorldY - 1, this.WorldZ), Direction.Down);
            if (this.BlockY < 127)
                action(UniversalCoords.FromWorld(this.WorldX, this.WorldY + 1, this.WorldZ), Direction.Up);
            action(UniversalCoords.FromWorld(this.WorldX, this.WorldY, this.WorldZ - 1), Direction.East);
            action(UniversalCoords.FromWorld(this.WorldX, this.WorldY, this.WorldZ + 1), Direction.West);
        }
        
        /// <summary>
        /// The distance between this coordinate and <paramref name="coords"/>
        /// </summary>
        /// <param name="coords"></param>
        /// <returns></returns>
        public double DistanceTo(UniversalCoords coords)
        {
            return System.Math.Sqrt(this.DistanceToSquared(coords));
        }
        
        /// <summary>
        /// Distance to coords squared. Quicker than <see cref="UniversalCoords.DistanceTo"/> but really only useful for comparing what might be closer than another coord.
        /// </summary>
        /// <returns>
        /// The to squared.
        /// </returns>
        /// <param name='coords'>
        /// Coords.
        /// </param>
        public double DistanceToSquared(UniversalCoords coords)
        {
            UniversalCoords diff = coords - this;
            return diff.WorldX * diff.WorldX + diff.WorldY * diff.WorldY + diff.WorldZ * diff.WorldZ;
        }
        
        public UniversalCoords Offset(int offsetX, int offsetY, int offsetZ)
        {
            return UniversalCoords.FromWorld(this.WorldX + offsetX, this.WorldY + offsetY, this.WorldZ + offsetZ);
        }
        
        public override int GetHashCode()
        {
            return (int)(WorldY & 0xff | (WorldX & 0x7fff) << 8 | (WorldZ & 0x7fff) << 24 | (WorldX >= 0 ? 0 : 0x80000000) | (WorldZ >= 0 ? 0 : 0x8000));
        }
        
        public override string ToString()
        {
            return string.Format("[WorldX={0}, WorldY={1}, WorldZ={2}]", WorldX, WorldY, WorldZ);
        }
                    
        public static UniversalCoords FromWorld(int worldX, int worldY, int worldZ)
        {
            return new UniversalCoords(worldX, worldY, worldZ);
        }

        public static UniversalCoords FromAbsWorld(double worldX, double worldY, double worldZ)
        {
            return new UniversalCoords((int)System.Math.Floor(worldX), (int)System.Math.Floor(worldY), (int)System.Math.Floor(worldZ));
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
