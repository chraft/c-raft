using System;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Utils;
using Chraft.World;
using Chraft.Plugins.Events.Args;

namespace Chraft.Entity
{
    /// <summary>
    /// Represents an entity, including clients (players), item drops, mobs, and vehicles.
    /// </summary>
    public abstract partial class EntityBase : IEquatable<EntityBase>
    {
        public int EntityId { get; private set; }
        public WorldManager World { get; set; }
  
        public BoundingBox BoundingBox { get; set; }
  
        /// <summary>
        /// Returns a Nullable <see cref="BoundingBox"/> that determines how, in addition to the BoundingBox, 
        /// the <paramref name="entity"/> collides with this instance.
        /// </summary>
        /// <returns>
        /// The collision box.
        /// </returns>
        /// <param name='entity'>
        /// Entity to produce collision box based on.
        /// </param>
        public virtual BoundingBox? GetCollisionBox(EntityBase entity)
        {
            return null;
        }
        
        short _health;
        /// <summary>
        /// Current entity Health represented as "halves of a heart", e.g. Health == 9 is 4.5 hearts. This value is clamped between 0 and EntityBase.MaxHealth.
        /// </summary>
        public virtual short Health
        {
            get { return _health; }
            set { _health = MathExtensions.Clamp(value, (short)0, this.MaxHealth); }
        }
        /// <summary>
        /// MaxHealth for this entity represented as "halves of a heart".
        /// </summary>
        public virtual short MaxHealth { get { return 20; } }
        /// <summary>
        /// Rotation around the X-axis
        /// </summary>
        public double Pitch { get; set; }

        /// <summary>
        /// Rotation around the Y-axis.
        /// </summary>
        public double Yaw { get; set; }

        public sbyte PackedPitch { get { return (sbyte)(this.Pitch / 360.0 * 256.0 % 256.0); } }

        public sbyte PackedYaw { get { return (sbyte)(this.Yaw / 360.0 * 256.0 % 256.0); } }

        public Server Server { get; private set; }
        public int TimeInWorld;
        //public Location Position { get; set; }
        public AbsWorldCoords Position { get; set; }

        public EntityBase(Server server, int entityId)
        {
            this.Server = server;
            this.EntityId = entityId;
            this.TimeInWorld = 0;
        }

        protected void EnsureServer(Server server)
        {
            if (Server == null)
                Server = server;
        }
        
        
        public virtual void Update()
        {
            
        }

        /// <summary>
        /// Move less than four blocks to the given destination and update all affected clients.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        public virtual void MoveTo(AbsWorldCoords absCoords)
        {
            AbsWorldCoords newPosition = absCoords;
                     
            //Event
            EntityMoveEventArgs e = new EntityMoveEventArgs(this, newPosition, Position);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_MOVE, e);
            if (e.EventCanceled) return;
            newPosition = e.NewPosition;
            //End Event

            sbyte dx = (sbyte)(32 * (newPosition.X - Position.X));
            sbyte dy = (sbyte)(32 * (newPosition.Y - Position.Y));
            sbyte dz = (sbyte)(32 * (newPosition.Z - Position.Z));
            Position = newPosition; // TODO: this doesn't prevent changing the Position by more than 4 blocks

            OnMoveTo(dx, dy, dz);
        }

        public virtual void OnMoveTo(sbyte x, sbyte y, sbyte z)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                c.SendMoveBy(this, x, y, z);
            }
        }

        /// <summary>
        /// Teleport the entity to the given destination and update all affected clients.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        public virtual bool TeleportTo(AbsWorldCoords absCoords)
        {
            Position = absCoords;
            
            OnTeleportTo(absCoords);
            return true;
        }

        public virtual void OnTeleportTo(AbsWorldCoords absCoords)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, absCoords))
            {
                c.SendTeleportTo(this);
            }
        }

        /// <summary>
        /// Rotate the entity to the given absolute rotation.
        /// </summary>
        /// <param name="yaw">Target yaw, absolute.</param>
        /// <param name="pitch">Target pitch, absolute.</param>
        public void RotateTo(float yaw, float pitch)
        {
            this.Yaw = yaw;
            this.Pitch = pitch;

            OnRotateTo();
        }

        public virtual void OnRotateTo()
        {
            foreach (Client c in Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                c.SendRotateBy(this, PackedYaw, PackedPitch);
            }
        }

        /// <summary>
        /// Move less than four blocks to the given destination and rotate.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        /// <param name="yaw">The absolute yaw to which entity should change.</param>
        /// <param name="pitch">The absolute pitch to which entity should change.</param>
        public virtual void MoveTo(AbsWorldCoords absCoords, float yaw, float pitch)
        {
            //Vector3 newPosition = new Vector3(absCoords.X, absCoords.Y, absCoords.Z);
            AbsWorldCoords newPosition = absCoords;

            //Event
            EntityMoveEventArgs e = new EntityMoveEventArgs(this, newPosition, Position);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_MOVE, e);
            if (e.EventCanceled) return;
            newPosition = e.NewPosition;
            //End Event

            sbyte dx = (sbyte)(32 * (newPosition.X - Position.X));
            sbyte dy = (sbyte)(32 * (newPosition.Y - Position.Y));
            sbyte dz = (sbyte)(32 * (newPosition.Z - Position.Z));
            Position = newPosition;
            this.Yaw = yaw;
            this.Pitch = pitch;

            OnMoveRotateTo(dx, dy, dz);
            
        }

        public virtual void OnMoveRotateTo(sbyte x, sbyte y, sbyte z)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                c.SendMoveRotateBy(this, x, y, z, PackedYaw, PackedPitch);
            }
        }

        public bool Equals(EntityBase other)
        {
            return other.EntityId == EntityId;
        }
    }
    public enum DamageCause
    {
        Contact,
        EntityAttack,
        Projectile,
        Suffocation,
        Fall,
        Fire,
        FireBurn,
        Lava,
        Drowning,
        BlockExplosion,
        EntityExplosion,
        Void,
        Lightning,
        Custom
    }
}
