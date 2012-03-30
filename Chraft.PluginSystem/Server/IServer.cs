using System.Collections.Generic;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.World;
using Chraft.PluginSystem.World.Blocks;
using Chraft.Utilities.Collision;
using Chraft.Utilities.Coords;

namespace Chraft.PluginSystem.Server
{
    public interface IServer
    {
        void AddChunkGenerator(string name, IChunkGenerator generator);
        ILogger GetLogger();
        IBlockHelper GetBlockHelper();
        void Broadcast(string message, IClient excludeClient = null, bool sendToIrc = true);
        void BroadcastSync(string message, IClient excludeClient = null, bool sendToIrc = true);
        IClient[] GetClients();
        IEnumerable<IClient> GetClients(string name);
        IClient[] GetAuthenticatedClients();
        IEntityBase[] GetEntities();
        void AddEntity(IEntityBase e, bool notifyNearbyClients = true);
        void RemoveEntity(IEntityBase e, bool notifyNearbyClients = true);
        void AddClient(IClient iClient);
        void RemoveClient(IClient iClient);
        void AddAuthenticatedClient(IClient iClient);
        void RemoveAuthenticatedClient(IClient iClient);
        void SendEntityToNearbyPlayers(IWorldManager world, IEntityBase entity);
        void SendRemoveEntityToNearbyPlayers(IWorldManager world, IEntityBase entity);
        IEnumerable<IClient> GetNearbyPlayers(IWorldManager world, UniversalCoords coords);
        IEnumerable<IEntityBase> GetNearbyEntities(IWorldManager world, UniversalCoords coords);
        Dictionary<int, IEntityBase> GetNearbyEntitiesDict(IWorldManager world, UniversalCoords coords);
        IEntityBase GetEntityById(int id);
        IEnumerable<ILivingEntity> GetNearbyLivings(IWorldManager world, UniversalCoords coords);
        IEnumerable<IEntityBase> GetEntitiesWithinBoundingBox(BoundingBox boundingBox);
        IWorldManager[] GetWorlds();
        IWorldManager GetDefaultWorld();
        void Stop();
        IItemDb GetItemDb();
        IPluginManager GetPluginManager();
        IMobFactory GetMobFactory();
        void BroadcastTimeUpdate(IWorldManager world);
    }
}
