using Chraft.World;

namespace Chraft.WorldGen
{
    public interface IChunkGenerator
    {
        void ProvideChunk(int x, int z, Chunk chunk, bool recalculate);
    }
}
