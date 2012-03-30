using Chraft.Utilities.Blocks;

namespace Chraft.PluginSystem.World.Blocks
{
    public interface IBlockHelper
    {
        bool IsGrowable(byte blockId);
        bool IsGrowable(BlockData.Blocks blockType);
        bool IsFertile(byte blockId);
        bool IsFertile(BlockData.Blocks blockType);
        bool IsPlowed(byte blockId);
        bool IsPlowed(BlockData.Blocks blockType);
        bool IsAir(byte blockId);
        bool IsAir(BlockData.Blocks blockType);
        bool IsLiquid(byte blockId);
        bool IsLiquid(BlockData.Blocks blockType);
        bool IsSolid(byte blockId);
        bool IsSolid(BlockData.Blocks blockType);
        bool IsSingleHit(byte blockId);
        bool IsSingleHit(BlockData.Blocks blockType);
        bool IsOpaque(byte blockId);
        bool IsOpaque(BlockData.Blocks blockType);
        byte Opacity(byte blockId);
        byte Opacity(BlockData.Blocks blockType);
        byte Luminance(byte blockId);
        byte Luminance(BlockData.Blocks blockType);
        bool IsIgnitable(byte blockId);
        bool IsIgnitable(BlockData.Blocks blockType);
        short BurnEfficiency(byte blockId);
        short BurnEfficiency(BlockData.Blocks blockType);
        bool IsWaterProof(byte blockId);
        bool IsWaterProof(BlockData.Blocks blockType);

    }
}
