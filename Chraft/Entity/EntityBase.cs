using System;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.Utils;
using Chraft.World;
using Chraft.Plugins.Events.Args;
using Chraft.World.Blocks;

namespace Chraft.Entity
{
    /// <summary>
    /// Represents an entity, including clients (players), item drops, mobs, and vehicles.
    /// </summary>
    public abstract partial class EntityBase : IEquatable<EntityBase>
    {
        /// <summary>
        /// The out of date warning threshold in Ticks (e.g. if 5, warning will output to console if 5 ticks behind world tick).
        /// </summary>
        public static int LagWarningThreshold = 5; 
        
        public int EntityId { get; private set; }
        public WorldManager World { get; set; }
  
        public bool NoClip { get; set; }
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
        
        public bool HasCollided { get; set; }
        public bool HasCollidedHorizontally { get; set; }
        public bool HasCollidedVertically { get; set; }
        public bool OnGround { get; set; }
        public float FallDistance { get; set; }
        public EntityBase RiddenBy { get; set; }
        
        public float Height { get; set; }
        public float Width { get; set; }
        
        public Vector3 Velocity;
        
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
        public int TicksInWorld;
        public int StartTick;
        public int UpdateFrequency;
        //public Location Position { get; set; }
        private AbsWorldCoords _position;
        public AbsWorldCoords Position 
        { 
            get
            {
                return _position;
            }
            set
            {
                _position = value;
                // Set the bounding box based on the new position
                double halfWidth = this.Width / 2.0;
                this.BoundingBox = new BoundingBox(new AbsWorldCoords(_position.X - halfWidth, _position.Y, _position.Z - halfWidth), new AbsWorldCoords(_position.X + halfWidth, _position.Y + Height, _position.Z + halfWidth));
            }
        }

        public EntityBase(Server server, int entityId)
        {
            this.Server = server;
            this.EntityId = entityId;
            this.TicksInWorld = 0;
            this.Width = 0.6f;
            this.Height = 1.8f;
            this.UpdateFrequency = 1;
        }

        protected void EnsureServer(Server server)
        {
            if (Server == null)
                Server = server;
        }
        
        /// <summary>
        /// The main Update method for Entities - called by the WorldManager each tick.
        /// </summary>
        /// <remarks>
        /// This method will automatically catch up with the current WorldTicks to ensure the entity
        /// is in synch with the world
        /// </remarks>
        public void Update()
        {
            if (this.TicksInWorld == 0)
                this.StartTick = this.World.WorldTicks;
            
            int loopCount = 0;
            // This loop allows the entity to catchup to the current world tick in case lagging
            while (this.World.WorldTicks - this.StartTick >= this.TicksInWorld)
            {
                this.TicksInWorld++;
                if (this.TicksInWorld % this.UpdateFrequency == 0)
                {
                    this.DoUpdate();
                }
                loopCount++;
            }
            
            if (loopCount > EntityBase.LagWarningThreshold)
            {
                Console.WriteLine("WARNING! Entity {0}'s ({1}) update method was behind by {2} ticks.", this.EntityId, this.GetType().Name, loopCount-1);
            }
        }
        
        /// <summary>
        /// Performs the update logic for this entity - called every tick, or in the case of the entity lagging behind the WorldTick multiple times for a tick.
        /// </summary>
        protected virtual void DoUpdate()
        {
            
        }
        
        /// <summary>
        /// Applies the specified velocity to this entity.
        /// </summary>
        /// <param name='velocity'>
        /// Velocity.
        /// </param>
        public virtual void ApplyVelocity(Vector3 velocity)
        {
            if (this.NoClip)
            {
                this.BoundingBox = this.BoundingBox + velocity;
                _position = new AbsWorldCoords(this.Position.ToVector() + velocity);
                return;
            }
            
            Vector3 initialVelocity = velocity;
            
            // TODO: if sneaking and onground prevent falling off edges
            
            this.BoundingBox = this.BoundingBox.OffsetWithClipping(ref velocity, this.World.GetCollidingBoundingBoxes(this, this.BoundingBox + velocity));
            
            // Set the new position to the centre point of the base of the BoundingBox
            _position = new AbsWorldCoords((this.BoundingBox.Minimum.X + this.BoundingBox.Maximum.X) / 2.0, this.BoundingBox.Minimum.Y, (this.BoundingBox.Minimum.Z + this.BoundingBox.Maximum.Z) / 2.0);
            
            #region Update Collision States
            this.HasCollidedHorizontally = initialVelocity.X != velocity.X || initialVelocity.Z != velocity.Z;
            this.HasCollidedVertically = initialVelocity.Y != velocity.Y;
            this.HasCollided = this.HasCollidedHorizontally || this.HasCollidedVertically;
            this.OnGround = this.HasCollidedVertically && initialVelocity.Y < 0.0;
            AddFallingDistance(velocity.Y, this.OnGround);
            
            if (initialVelocity.X != velocity.X)
            {
                Velocity.X = 0.0;
            }
            if (initialVelocity.Y != velocity.Y)
            {
                Velocity.Y = 0.0;
            }
            if (initialVelocity.Z != velocity.Z)
            {
                Velocity.Z = 0.0;
            }
            #endregion
            
            // TODO: notify blocks of collisions + play sounds
            
            // TODO: check for proximity to fire
        }
        
        /// <summary>
        /// Adds the falling distance to this entity.
        /// </summary>
        /// <param name='distance'>
        /// Distance (negative for falling down, positive for floating up).
        /// </param>
        /// <param name='onGround'>
        /// On ground?
        /// </param>
        public virtual void AddFallingDistance(double distance, bool onGround)
        {
            if (onGround)
            {
                if (this.FallDistance > 0.0f)
                {
                    Fall(this.FallDistance);
                    this.FallDistance = 0.0f;
                }
            }
            else if (distance < 0.0)
            {
                this.FallDistance -= (float)distance; // if falling down, distance will be negative (therefore -= to get a positive FallDistance)
            }
        }
        
        /// <summary>
        /// Process any logic for when this entity falls the specified distance.
        /// </summary>
        /// <param name='distance'>
        /// Distance fallen (e.g. 3.3f).
        /// </param>
        public virtual void Fall(float distance)
        {
            // If this entity is being ridden by another, make it fall as well
            if (this.RiddenBy != null)
                this.RiddenBy.Fall(distance);
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
            this.Yaw = yaw % 360.0f;
            this.Pitch = pitch % 360.0f;

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
