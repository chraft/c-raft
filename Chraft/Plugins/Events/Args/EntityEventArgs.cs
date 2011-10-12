using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.Net;
using Chraft.World;
using Chraft.Utils;

namespace Chraft.Plugins.Events.Args
{
    /// <summary>
    /// The base EventArgs for an Entity Event.
    /// </summary>
    public class EntityEventArgs : ChraftEventArgs
    {
        public EntityBase Entity { get; private set; }
        public EntityEventArgs(EntityBase entity)
        {
            Entity = entity;
        }
    }

    public class EntityDeathEventArgs : EntityEventArgs 
    {
        public Player KilledBy { get; set; }
        public EntityDeathEventArgs(EntityBase entity, Player killedBy)
            : base(entity)
        {
            KilledBy = killedBy;
        }
    }
    public class EntitySpawnEventArgs : EntityEventArgs 
    {
        public AbsWorldCoords Location { get; set; }

        public EntitySpawnEventArgs(EntityBase entity, AbsWorldCoords Location)
            : base(entity)
        {
            this.Location = Location;
        }
    }
    public class EntityMoveEventArgs : EntityEventArgs 
    {
        public AbsWorldCoords NewPosition { get; set; }
        public AbsWorldCoords OldPosition { get; private set; }

        public EntityMoveEventArgs(EntityBase entity, AbsWorldCoords newPosition, AbsWorldCoords oldPosition) 
            : base(entity)
        {
            NewPosition = newPosition;
            OldPosition = oldPosition;
        }
    }
    public class EntityDamageEventArgs : EntityEventArgs
    {
        public short Damage { get; set; }
        public Player DamagedBy { get; set; }
        public DamageCause Cause { get; set; }

        public EntityDamageEventArgs(EntityBase entity, short damage, Player damagedBy, DamageCause cause)
            : base(entity)
        {
            Damage = damage;
            DamagedBy = damagedBy;
            Cause = cause;
        }
    }
    public class EntityAttackEventArgs : EntityEventArgs
    {
        public int Damage { get; set; }
        public EntityBase EntityToAttack { get; set; }

        public EntityAttackEventArgs(EntityBase entity, int damage, EntityBase entityToAttack)
            : base(entity)
        {
            Damage = damage;
            EntityToAttack = entityToAttack;
        }
    }
}
