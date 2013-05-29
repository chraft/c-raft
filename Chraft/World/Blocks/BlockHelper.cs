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
using System.Linq;
using System.Reflection;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities;
using Chraft.Utilities.Blocks;
using Chraft.World.Blocks.Base;

namespace Chraft.World.Blocks
{
    public sealed class BlockHelper : IBlockHelper
    {
        private static ConcurrentDictionary<byte, BlockBase> _blocks;
        private static ConcurrentDictionary<byte, byte> _growableBlocks;
        private static ConcurrentDictionary<byte, byte> _fertileBlocks;
        private static ConcurrentDictionary<byte, byte> _plowedBlocks;
        private static ConcurrentDictionary<byte, byte> _airBlocks;
        private static ConcurrentDictionary<byte, byte> _liquidBlocks;
        private static ConcurrentDictionary<byte, byte> _solidBlocks;
        private static ConcurrentDictionary<byte, byte> _singleHitBlocks;
        private static ConcurrentDictionary<byte, byte> _blocksOpacity;
        private static ConcurrentDictionary<byte, byte> _blocksLuminance;
        private static ConcurrentDictionary<byte, short> _blocksBurnEfficiency;
        private static ConcurrentDictionary<byte, byte> _waterProofBlocks;
        private static ConcurrentDictionary<byte, byte> _interactiveBlocks;

        private static readonly BlockHelper _instance = new BlockHelper();

        public static BlockHelper Instance { get { return _instance; } }


        private BlockHelper()
        {
            Init();
        }

        private static void Init()
        {
            _blocks = new ConcurrentDictionary<byte, BlockBase>();
            _growableBlocks = new ConcurrentDictionary<byte, byte>();
            _fertileBlocks = new ConcurrentDictionary<byte, byte>();
            _plowedBlocks = new ConcurrentDictionary<byte, byte>();
            _airBlocks = new ConcurrentDictionary<byte, byte>();
            _liquidBlocks = new ConcurrentDictionary<byte, byte>();
            _solidBlocks = new ConcurrentDictionary<byte, byte>();
            _singleHitBlocks = new ConcurrentDictionary<byte, byte>();
            _blocksOpacity = new ConcurrentDictionary<byte, byte>();
            _blocksLuminance = new ConcurrentDictionary<byte, byte>();
            _blocksBurnEfficiency = new ConcurrentDictionary<byte, short>();
            _waterProofBlocks = new ConcurrentDictionary<byte, byte>();
            _interactiveBlocks = new ConcurrentDictionary<byte, byte>();

            BlockBase block;
            byte blockId;
            foreach (Type t in from t in Assembly.GetExecutingAssembly().GetTypes()
                               where t.GetInterfaces().Contains(typeof(IBlockBase)) && !t.IsAbstract
                               select t)
            {
                block = (BlockBase)t.GetConstructor(Type.EmptyTypes).Invoke(null);
                blockId = (byte)block.Type;
                _blocks.TryAdd(blockId, block);
                if (block is IBlockGrowable)
                    _growableBlocks.TryAdd(blockId, blockId);
                if (block.IsFertile)
                    _fertileBlocks.TryAdd(blockId, blockId);
                if (block.IsPlowed)
                    _plowedBlocks.TryAdd(blockId, blockId);
                if (block.IsAir)
                    _airBlocks.TryAdd(blockId, blockId);
                if (block.IsLiquid)
                    _liquidBlocks.TryAdd(blockId, blockId);
                if (block.IsSolid)
                    _solidBlocks.TryAdd(blockId, blockId);
                if (block.IsSingleHit)
                    _singleHitBlocks.TryAdd(blockId, blockId);
                if (block.IsWaterProof)
                    _waterProofBlocks.TryAdd(blockId, blockId);
                _blocksOpacity.TryAdd(blockId, block.Opacity);
                _blocksLuminance.TryAdd(blockId, block.Luminance);
                _blocksBurnEfficiency.TryAdd(blockId, block.BurnEfficiency);
                if (block is IBlockInteractive)
                    _interactiveBlocks.TryAdd(blockId, blockId);
            }
        }

        /// <summary>
        /// Use the block instance only if you are going to call its method.
        /// To access the properties use the appropriate BlockHelper method when possible
        /// </summary>
        /// <param name="blockId"></param>
        /// <returns></returns>
        internal BlockBase CreateBlockInstance(byte blockId)
        {
            if (_blocks.ContainsKey(blockId))
                return _blocks[blockId];
            return null;
        }

        public bool IsGrowable(byte blockId)
        {
            return _growableBlocks.ContainsKey(blockId);
        }

        public bool IsGrowable(BlockData.Blocks blockType)
        {
            return _growableBlocks.ContainsKey((byte)blockType);
        }

        public bool IsFertile(byte blockId)
        {
            return _fertileBlocks.ContainsKey(blockId);
        }

        public bool IsFertile(BlockData.Blocks blockType)
        {
            return _fertileBlocks.ContainsKey((byte)blockType);
        }

        public bool IsPlowed(byte blockId)
        {
            return _plowedBlocks.ContainsKey(blockId);
        }

        public bool IsPlowed(BlockData.Blocks blockType)
        {
            return _plowedBlocks.ContainsKey((byte)blockType);
        }

        public bool IsAir(byte blockId)
        {
            return _airBlocks.ContainsKey(blockId);
        }

        public bool IsAir(BlockData.Blocks blockType)
        {
            return _airBlocks.ContainsKey((byte)blockType);
        }

        public bool IsLiquid(byte blockId)
        {
            return _liquidBlocks.ContainsKey(blockId);
        }

        public bool IsLiquid(BlockData.Blocks blockType)
        {
            return _liquidBlocks.ContainsKey((byte)blockType);
        }

        public bool IsSolid(byte blockId)
        {
            return _solidBlocks.ContainsKey(blockId);
        }

        public bool IsSolid(BlockData.Blocks blockType)
        {
            return _solidBlocks.ContainsKey((byte)blockType);
        }

        public bool IsSingleHit(byte blockId)
        {
            return _singleHitBlocks.ContainsKey(blockId);
        }

        public bool IsSingleHit(BlockData.Blocks blockType)
        {
            return _singleHitBlocks.ContainsKey((byte)blockType);
        }

        public bool IsOpaque(byte blockId)
        {
            byte opacity;
            _blocksOpacity.TryGetValue(blockId, out opacity);
            return (opacity == 0xF);
        }

        public bool IsOpaque(BlockData.Blocks blockType)
        {
            byte opacity;
            _blocksOpacity.TryGetValue((byte)blockType, out opacity);
            return (opacity == 0xF);
        }

        public byte Opacity(byte blockId)
        {
            byte opacity;
            _blocksOpacity.TryGetValue(blockId, out opacity);
            return opacity;
        }

        public byte Opacity(BlockData.Blocks blockType)
        {
            byte opacity;
            _blocksOpacity.TryGetValue((byte)blockType, out opacity);
            return opacity;
        }

        public byte Luminance(byte blockId)
        {
            byte luminance;
            _blocksLuminance.TryGetValue(blockId, out luminance);
            return luminance;
        }

        public byte Luminance(BlockData.Blocks blockType)
        {
            byte luminance;
            _blocksLuminance.TryGetValue((byte)blockType, out luminance);
            return luminance;
        }

        public bool IsIgnitable(byte blockId)
        {
            short burnEfficiency;
            _blocksBurnEfficiency.TryGetValue(blockId, out burnEfficiency);
            return (burnEfficiency > 0);
        }

        public bool IsIgnitable(BlockData.Blocks blockType)
        {
            short burnEfficiency;
            _blocksBurnEfficiency.TryGetValue((byte)blockType, out burnEfficiency);
            return (burnEfficiency > 0);
        }

        public short BurnEfficiency(byte blockId)
        {
            short burnEfficiency;
            _blocksBurnEfficiency.TryGetValue(blockId, out burnEfficiency);
            return burnEfficiency;
        }

        public short BurnEfficiency(BlockData.Blocks blockType)
        {
            short burnEfficiency;
            _blocksBurnEfficiency.TryGetValue((byte)blockType, out burnEfficiency);
            return burnEfficiency;
        }

        public bool IsWaterProof(byte blockId)
        {
            return _waterProofBlocks.ContainsKey(blockId);
        }

        public bool IsWaterProof(BlockData.Blocks blockType)
        {
            return _waterProofBlocks.ContainsKey((byte)blockType);
        }

        public bool IsInteractive(byte blockId)
        {
            return _interactiveBlocks.ContainsKey(blockId);
        }

        public bool IsInteractive(BlockData.Blocks blockType)
        {
            return _interactiveBlocks.ContainsKey((byte)blockType);
        }
    }
}
