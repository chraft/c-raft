using Chraft.World;

namespace Chraft.WorldGen
{
    public interface IChunkGenerator
    {
        Chunk ProvideChunk(int x, int z, Chunk chunk, bool recalculate);
    }
}
