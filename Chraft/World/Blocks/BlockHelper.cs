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
        private static List<byte> _growableBlocks;
        private static Dictionary<byte, BlockBase> _blocks;

        static BlockHelper()
        {
            Init();
        }

        private static void Init()
        {
            _blocks = new Dictionary<byte, BlockBase>();
            _growableBlocks = new List<byte>();
            BlockBase block;

            foreach (Type t in from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.GetInterfaces().Contains(typeof(IBlockBase)) && !t.IsAbstract
                               select t)
            {
                block = (BlockBase) t.GetConstructor(Type.EmptyTypes).Invoke(null);
                _blocks.Add((byte)block.Type, block);
                if (block is IBlockGrowable)
                    _growableBlocks.Add((byte)block.Type);
            }
        }

        public static BlockBase Instance(byte blockId)
        {
            BlockBase block = null;
            if (_blocks.ContainsKey(blockId))
                return _blocks[blockId];
            return block;
        }

        public static bool IsGrowable(byte blockId)
        {
            return _growableBlocks.Contains(blockId);
        }

        public static bool IsGrowable(BlockData.Blocks blockType)
        {
            return _growableBlocks.Contains((byte)blockType);
        }
    }
}
