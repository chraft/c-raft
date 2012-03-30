using Chraft.PluginSystem.Server;

namespace Chraft.PluginSystem.Net
{
    public interface IPacket
    {
        void SetShared(ILogger logger, int length);
    }
}
