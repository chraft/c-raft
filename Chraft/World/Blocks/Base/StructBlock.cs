using Chraft.PluginSystem.World;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Coords;

namespace Chraft.World.Blocks.Base
{
    /// <summary>
    /// Represents a certain block in the world
    /// </summary>
    public struct StructBlock : IStructBlock
    {
        public byte _type;
        public UniversalCoords _coords;
        public byte _metaData;
        public IWorldManager _worldInterface;
        private WorldManager _world;

        public byte Type
        {
            get { return _type; } 
            set { _type = value; }
        }

        public UniversalCoords Coords
        {
            get { return _coords; }
            set { _coords = value; }
        }

        public byte MetaData
        {
            get { return _metaData; }
            set { _metaData = value; }
        }

        public IWorldManager WorldInterface{get { return _worldInterface; }}
        internal WorldManager World{get { return _world; }}

        public StructBlock(UniversalCoords coords, byte type, byte metaData, IWorldManager world)
        {
            _type = type;
            _coords = coords;
            _metaData = metaData;
            _world = world as WorldManager;
            _worldInterface = world;
        }

        public StructBlock(int worldX, int worldY, int worldZ, byte type, byte metaData, IWorldManager world)
        {
            _type = type;
            _coords = UniversalCoords.FromWorld(worldX, worldY, worldZ);
            _metaData = metaData;
            _world = world as WorldManager;
            _worldInterface = world;
        }

        public static readonly StructBlock Empty;

        public override string ToString()
        {
            return string.Format("Type {0}, Coords {1}", this.Type, this.Coords);
        }
    }
}