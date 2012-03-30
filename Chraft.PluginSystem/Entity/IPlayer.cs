using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.World;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;

namespace Chraft.PluginSystem.Entity
{
    public interface IPlayer : ILivingEntity
    {
        string Name { get; }
        IClient GetClient();
        IInventory GetInventory();
        string DisplayName { get; set; }
        
        bool IsMuted { get; set; }

        float EyeHeight { get; }
        bool Ready { get; set; }
        byte GameMode { get; set; }
        float FoodSaturation { get; set; }
        short Food { get; set; }

        void InitializePosition();
        void InitializeInventory();
        void InitializeHealth();
        void TouchedFire();
        void TouchedLava();
        void MoveTo(AbsWorldCoords absCoords);
        void MoveTo(AbsWorldCoords absCoords, float yaw, float pitch);

        void StartCrouching();
        void StopCrouching();
        void StartSprinting();
        void StopSprinting();
        void Attack(ILivingEntity target);

        void Damage(DamageCause cause, short damageAmount, IEntityBase hitBy = null, params object[] args);

        short GetWeaponDamage();
        void DamageArmor(short damage);

        void SynchronizeEntities();
        IChunk GetCurrentChunk();

        void MarkToSave();
        void SetHealth(short health);
        void DropActiveSlotItem();
        bool CanUseCommand(ICommand command);
        bool CanUseCommand(string command);
        string GetPlayerPrefix();
        string GetPlayerSuffix();
    }
}
