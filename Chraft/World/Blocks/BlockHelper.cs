using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    public class BlockHelper
    {
        private bool _initialized;
        public BlockHelper()
        {
            if (!_initialized)
                Init();
        }

        private ConcurrentDictionary<byte, BlockBase> _blocks;
        private void Init()
        {
            _blocks = new ConcurrentDictionary<byte, BlockBase>();
            BlockBase block;

            foreach (Type t in from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.GetInterfaces().Contains(typeof(IBlockBase)) && !t.IsAbstract
                               select t)
            {
                block = (BlockBase) t.GetConstructor(Type.EmptyTypes).Invoke(null);
                _blocks.TryAdd((byte)block.Type, block);
            }
            _initialized = true;
        }

        public BlockBase Instance(byte blockId)
        {
            BlockBase block = null;
            _blocks.TryGetValue(blockId, out block);
            return block;
        }

        public bool IsGrowable(byte blockId)
        {
            return (Instance(blockId) is IBlockGrowable);
        }

        public bool IsGrowable(BlockData.Blocks blockType)
        {
            return IsGrowable((byte)blockType);
        }
    }
}
