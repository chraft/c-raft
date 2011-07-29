using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Entity;
using Chraft.World;

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
        public Client KilledBy { get; set; }
        public EntityDeathEventArgs(EntityBase entity, Client killedBy)
            : base(entity)
        {
            KilledBy = killedBy;
        }
    }
    public class EntitySpawnEventArgs : EntityEventArgs 
    {
        public Vector3 Location { get; set; }

        public EntitySpawnEventArgs(EntityBase entity, Vector3 Location)
            : base(entity)
        {
            this.Location = Location;
        }
    }
    public class EntityMoveEventArgs : EntityEventArgs 
    {
        public Vector3 NewPosition { get; set; }
        public Vector3 OldPosition { get; private set; }

        public EntityMoveEventArgs(EntityBase entity, Vector3 newPosition, Vector3 oldPosition) 
            : base(entity)
        {
            NewPosition = newPosition;
            OldPosition = oldPosition;
        }
    }
    public class EntityDamageEventArgs : EntityEventArgs
    {
        public short Damage { get; set; }
        public Client DamagedBy { get; set; }

        public EntityDamageEventArgs(EntityBase entity, short damage, Client damagedBy)
            : base(entity)
        {
            Damage = damage;
            DamagedBy = damagedBy;
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
