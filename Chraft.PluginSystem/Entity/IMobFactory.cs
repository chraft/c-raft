using System;
using Chraft.Utilities;

namespace Chraft.PluginSystem
{
    public interface IMobFactory
    {
        Type GetMobClass(IWorldManager world, MobType type);
        IMob CreateMob(IWorldManager world, IServer server, MobType type, IMetaData data = null);
    }
}