using Chraft.WorldGen;
using Chraft.World;

namespace Chraft.WorldGen
{
    public enum GeneratorType
    {
        Custom 
    }
    public class ChunkProvider
    {
        private WorldManager _World;

        public ChunkProvider(WorldManager world)
        {
            _World = world;
        }
        
        public IChunkGenerator GetNewGenerator(GeneratorType type, long seed)
        {
            switch(type)
            {
                case GeneratorType.Custom: return new CustomChunkGenerator(_World, seed);
                default: return new CustomChunkGenerator(_World, seed);
            }
        }
    }
}