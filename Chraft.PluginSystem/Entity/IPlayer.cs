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
        IClient GetClient();
        IInventory GetInventory();
        string DisplayName { get; set; }
        
        bool IsMuted { get; set; }

        bool Ready { get; set; }
        GameMode GameMode { get; set; }
        float FoodSaturation { get; set; }
        short Food { get; set; }

        void InitializePosition();
        void InitializeInventory();
        void InitializeHealth();

        void StartCrouching();
        void StopCrouching();
        void StartSprinting();
        void StopSprinting();

        void DamageArmor(short damage);

        void SynchronizeEntities();
        IChunk GetCurrentChunk();

        void MarkToSave();
        void SetHealth(short health);
        void AddHealth(short health);
        void DropActiveSlotItem();
        bool CanUseCommand(ICommand command);
        bool CanUseCommand(string command);
        string GetPlayerPrefix();
        string GetPlayerSuffix();
        void AddExperience(short amount);

        bool IsHungry();
        bool EatFood(short food, float saturation);
    }

    public enum GameMode : byte
    {
        Normal = 0,
        Creative = 1,
        Adventure = 2
    }
}
