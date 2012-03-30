using Chraft.Utilities.Misc;

namespace Chraft.PluginSystem.Entity
{
    public interface IMetaData
    {
        bool Sheared { get; }

        WoolColor WoolColor { get; }

        bool IsOnFire { get; }

        bool IsCrouched { get; }

        bool IsRiding { get; }

        bool IsSprinting { get; }

        bool IsSitting { get; }

        bool IsAggressive { get; }
        
        bool IsTamed { get; }

        string TamedBy { get; }
        int Health { get; }
    }
}