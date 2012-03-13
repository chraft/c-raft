
using Chraft.Utilities;

namespace Chraft.PluginSystem
{
    public interface IMob : ILivingEntity
    {
        MobType Type { get; set; }
       
        short AttackStrength { get; }
        
        int AttackRange { get; }
        
        int SightRange { get; }

        int MaxSpawnedPerGroup { get; }

        bool Hunter { get; }
        bool Hunting { get; }

        void InteractWith(IClient client, IItemStack item);

        void Despawn();
    }
}