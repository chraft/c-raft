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
        public virtual short MaxHealth { get { return 10; } }
        public sbyte PackedPitch { get { return Position.PackedPitch; } }
        public sbyte PackedYaw { get { return Position.PackedYaw; } }
        public Server Server { get; private set; }
        public int TimeInWorld;
        public Location Position { get; set; }

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

        /// <summary>
        /// Move less than four blocks to the given destination and update all affected clients.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        public virtual void MoveTo(double x, double y, double z)
        {
            Vector3 newPosition = new Vector3(x, y, z);

            //Event
            EntityMoveEventArgs e = new EntityMoveEventArgs(this, newPosition, Position.Vector);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_MOVE, e);
            if (e.EventCanceled) return;
            newPosition = e.NewPosition;
            //End Event

            sbyte dx = (sbyte)(32 * (newPosition.X - Position.X));
            sbyte dy = (sbyte)(32 * (newPosition.Y - Position.Y));
            sbyte dz = (sbyte)(32 * (newPosition.Z - Position.Z));
            Position.Vector = newPosition; // TODO: this doesn't prevent changing the Position by more than 4 blocks

            
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Owner.Equals(this))
                    c.SendMoveBy(this, dx, dy, dz);
            }
        }

        /// <summary>
        /// Teleport the entity to the given destination and update all affected clients.
        /// </summary>
        /// <param name="x">The X coordinate of the target.</param>
        /// <param name="y">The Y coordinate of the target.</param>
        /// <param name="z">The Z coordinate of the target.</param>
        public virtual bool TeleportTo(double x, double y, double z)
        {
            Position.X = x;
            Position.Y = y;
            Position.Z = z;
            
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Owner.Equals(this))
                    c.SendTeleportTo(this);
                else
                {
                    c.SendPacket(new PlayerPositionRotationPacket
                    {
                        X = x,
                        Y = y + Player.EyeGroundOffset,
                        Z = z,
                        Yaw = (float)c.Owner.Position.Yaw,
                        Pitch = (float)c.Owner.Position.Pitch,
                        Stance = c.Stance,
                        OnGround = false
                    }
                    );
                }
            }
            return true;
        }

        /// <summary>
        /// Rotate the entity to the given absolute rotation.
        /// </summary>
        /// <param name="yaw">Target yaw, absolute.</param>
        /// <param name="pitch">Target pitch, absolute.</param>
        public void RotateTo(float yaw, float pitch)
        {
            Position.Yaw = yaw;
            Position.Pitch = pitch;
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Owner.Equals(this))
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
        public virtual void MoveTo(double x, double y, double z, float yaw, float pitch)
        {
            Vector3 newPosition = new Vector3(x, y, z);

            //Event
            EntityMoveEventArgs e = new EntityMoveEventArgs(this, newPosition, Position.Vector);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_MOVE, e);
            if (e.EventCanceled) return;
            newPosition = e.NewPosition;
            //End Event

            sbyte dx = (sbyte)(32 * (newPosition.X - Position.X));
            sbyte dy = (sbyte)(32 * (newPosition.Y - Position.Y));
            sbyte dz = (sbyte)(32 * (newPosition.Z - Position.Z));
            Position.Vector = newPosition;
            Position.Yaw = yaw;
            Position.Pitch = pitch;
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Owner.Equals(this))
                    c.SendMoveRotateBy(this, dx, dy, dz, PackedYaw, PackedPitch);
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
