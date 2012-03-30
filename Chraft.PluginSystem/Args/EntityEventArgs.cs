#region C#raft License
// This file is part of C#raft. Copyright C#raft Team 
// 
// C#raft is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as
// published by the Free Software Foundation, either version 3 of the
// License, or (at your option) any later version.
// 
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
// 
// You should have received a copy of the GNU Affero General Public License
// along with this program. If not, see <http://www.gnu.org/licenses/>.
#endregion

using Chraft.PluginSystem.Entity;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Misc;

namespace Chraft.PluginSystem.Args
{
    /// <summary>
    /// The base EventArgs for an Entity Event.
    /// </summary>
    public class EntityEventArgs : ChraftEventArgs
    {
        public IEntityBase Entity { get; private set; }
        public EntityEventArgs(IEntityBase entity)
        {
            Entity = entity;
        }
    }

    public class EntityDeathEventArgs : EntityEventArgs 
    {
        public IEntityBase KilledBy { get; set; }
        public EntityDeathEventArgs(IEntityBase entity, IEntityBase killedBy)
            : base(entity)
        {
            KilledBy = killedBy;
        }
    }
    public class EntitySpawnEventArgs : EntityEventArgs 
    {
        public AbsWorldCoords Location { get; set; }

        public EntitySpawnEventArgs(IEntityBase entity, AbsWorldCoords Location)
            : base(entity)
        {
            this.Location = Location;
        }
    }
    public class EntityMoveEventArgs : EntityEventArgs 
    {
        public AbsWorldCoords NewPosition { get; set; }
        public AbsWorldCoords OldPosition { get; private set; }

        public EntityMoveEventArgs(IEntityBase entity, AbsWorldCoords newPosition, AbsWorldCoords oldPosition) 
            : base(entity)
        {
            NewPosition = newPosition;
            OldPosition = oldPosition;
        }
    }
    public class EntityDamageEventArgs : EntityEventArgs
    {
        public short Damage { get; set; }
        public IEntityBase DamagedBy { get; set; }
        public DamageCause Cause { get; set; }

        public EntityDamageEventArgs(IEntityBase entity, short damage, IEntityBase damagedBy, DamageCause cause)
            : base(entity)
        {
            Damage = damage;
            DamagedBy = damagedBy;
            Cause = cause;
        }
    }
    public class EntityAttackEventArgs : EntityEventArgs
    {
        public short Damage { get; set; }
        public IEntityBase EntityToAttack { get; set; }

        public EntityAttackEventArgs(IEntityBase entity, short damage, IEntityBase entityToAttack)
            : base(entity)
        {
            Damage = damage;
            EntityToAttack = entityToAttack;
        }
    }
}
