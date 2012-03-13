using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Utilities;

namespace Chraft.PluginSystem
{
    public interface ILivingEntity : IEntityBase
    {
        string Name { get; }

        short Health { get; set; }

        short MaxHealth { get; }

        float EyeHeight { get; }
        bool CanDrown { get; }
        bool CanSuffocate { get; }
        short FireBurnTicks { get; }
        bool IsImmuneToFire { get; }
        bool IsDead { get; }
        bool IsEntityAlive { get; }
        bool Collidable { get; }
        bool Pushable { get; }
        bool PreventMobSpawning { get; }

        IMetaData GetMetaData();
        bool CanSee(ILivingEntity entity);

        string FacingDirection(byte compassPoints);
        void TouchedCactus();
        void TouchedLava();
        void TouchedFire();
        void StopFireBurnTimer();
        void CheckDrowning();
        void CheckSuffocation();
        void Attack(PluginSystem.ILivingEntity target);
        void Damage(DamageCause cause, short damageAmount, IEntityBase hitBy = null, params object[] args);

        bool CanSpawnHere();
    }
}
