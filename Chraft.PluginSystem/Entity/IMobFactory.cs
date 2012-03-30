using System;
using Chraft.PluginSystem.Server;
using Chraft.PluginSystem.World;
using Chraft.Utilities.Misc;

namespace Chraft.PluginSystem.Entity
{
    public interface IMobFactory
    {
        Type GetMobClass(IWorldManager world, MobType type);
        IMob CreateMob(IWorldManager world, IServer server, MobType type, IMetaData data = null);
    }
}