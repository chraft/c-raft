using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Chraft.World.Blocks.Interfaces;

namespace Chraft.World.Blocks
{
    public static class BlockHelper
    {
        static BlockHelper()
        {
            Init();
        }

        private static ConcurrentDictionary<byte, BlockBase> _blocks;
        private static void Init()
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
        }

        public static BlockBase Instance(byte blockId)
        {
            BlockBase block = null;
            _blocks.TryGetValue(blockId, out block);
            return block;
        }

        public static bool IsGrowable(byte blockId)
        {
            return (Instance(blockId) is IBlockGrowable);
        }

        public static bool IsGrowable(BlockData.Blocks blockType)
        {
            return IsGrowable((byte)blockType);
        }
    }
}
