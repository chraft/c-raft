using System.Collections.Generic;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Collision;
using Chraft.Utilities.Coords;

namespace Chraft.PluginSystem.World
{
    public interface IWorldManager
    {
        IServer GetServer();
        byte? GetBlockId(int worldX, int worldY, int worldZ);
        byte? GetBlockId(UniversalCoords coords);
        
        IChunk CreateChunk(UniversalCoords coords);
        void AddChunk(IChunk iChunk);
        void RemoveChunk(IChunk c);
        IChunk GetChunk(UniversalCoords coords, bool create = false, bool load = false);
        IChunk GetChunkFromChunkSync(int chunkX, int chunkZ, bool create = false, bool load = false);
        IChunk GetChunkFromChunkAsync(int chunkX, int chunkZ, IClient client, bool create = false, bool load = false);
        IChunk GetChunkFromWorld(int worldX, int worldZ, bool create = false, bool load = false);
        IChunk GetChunkFromAbs(double absX, double absZ, bool create = false, bool load = false);
        IEnumerable<IEntityBase> GetEntitiesWithinBoundingBoxExcludingEntity(IEntityBase entity, BoundingBox boundingBox);
        int GetHeight(UniversalCoords coords);
        int GetHeight(int x, int z);
        bool IsSaving();
        IChunk[] GetChunks();
        IEnumerable<IStructBlock> GetBlocksInBoundingBox(BoundingBox boundingBox);
        IEnumerable<IStructBlock> GetBlocksBetweenCoords(UniversalCoords minimum, UniversalCoords maximum);
        IStructBlock GetBlock(UniversalCoords coords);
        IStructBlock GetBlock(int worldX, int worldY, int worldZ);
        byte? GetBlockData(UniversalCoords coords);
        byte? GetBlockData(int worldX, int worldY, int worldZ);
        double GetBlockLightBrightness(UniversalCoords coords);
        byte? GetFullBlockLight(UniversalCoords coords);
        byte? GetEffectiveLight(UniversalCoords coords);
        byte? GetBlockLight(UniversalCoords coords);
        byte? GetBlockLight(int worldX, int worldY, int worldZ);
        byte? GetSkyLight(UniversalCoords coords);
        byte? GetSkyLight(int worldX, int worldY, int worldZ);
        IEnumerable<IEntityBase> GetEntities();
        IPlayer GetClosestPlayer(AbsWorldCoords coords, double radius);
        long GetSeed();
        void SetBlockAndData(UniversalCoords coords, byte type, byte data, bool needsUpdate = true);
        void SetBlockAndData(int worldX, int worldY, int worldZ, byte type, byte data, bool needsUpdate = true);
        void SetBlockData(UniversalCoords coords, byte metaData, bool needsUpdate);
        void SetBlockData(int worldX, int worldY, int worldZ, byte data, bool needsUpdate = true);
        bool ChunkExists(UniversalCoords coords);
        bool ChunkExists(int chunkX, int chunkZ);

        int Time { get; set; }
        int AgeOfWorld { get; set; }
    }
}
