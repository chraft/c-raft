using System;
using System.Collections.Generic;
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
        
        public virtual bool Collidable { get { return false; } }
        
        public virtual bool Pushable { get { return false; } }

        /// <summary>
        /// Whether or not this entity prevents mobs from spawning if bounding boxes collide
        /// </summary>
        public virtual bool PreventMobSpawning { get { return false; } }
        
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
        /// <summary>
        /// The update frequency for this entity in ticks.
        /// </summary>
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
                BlockPosition = UniversalCoords.FromAbsWorld(_position);
                // Set the bounding box based on the new position
                double halfWidth = this.Width / 2.0;
                this.BoundingBox = new BoundingBox(new AbsWorldCoords(_position.X - halfWidth, _position.Y, _position.Z - halfWidth), new AbsWorldCoords(_position.X + halfWidth, _position.Y + Height, _position.Z + halfWidth));
            }
        }

        private UniversalCoords _blockPosition;
        public UniversalCoords BlockPosition
        {
            get { return _blockPosition; }
            private set { _blockPosition = value; }
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
            int currentWorldTick = this.World.WorldTicks;
            // This loop allows the entity to catchup to the current world tick in case lagging
            while (currentWorldTick - this.StartTick >= this.TicksInWorld)
            {
                this.TicksInWorld++;
                if (this.TicksInWorld % this.UpdateFrequency == 0)
                {
                    this.DoUpdate();
                }
                loopCount++;
            }
            
            if (loopCount > LagWarningThreshold)
            {
                Server.Logger.Log(Logger.LogLevel.Warning, "Entity {0}'s ({1}) update method was behind by {2} ticks.", this.EntityId, this.GetType().Name, loopCount-1);
            }
        }
        
        /// <summary>
        /// Performs the update logic for this entity - called every tick, or in the case of the entity lagging behind the WorldTick multiple times for a tick.
        /// </summary>
        protected virtual void DoUpdate()
        {
            
        }
        
        protected virtual void PushOutOfBlocks(AbsWorldCoords absWorldCoords)
        {
            UniversalCoords coords = UniversalCoords.FromAbsWorld(absWorldCoords);

            byte? blockId = World.GetBlockId(coords);

            if (blockId == null)
                return;

            BlockBase blockClass = BlockHelper.Instance((byte)blockId);
            if (blockClass.IsOpaque && blockClass.IsSolid)
            {
                // The offset within World (int) coords
                Vector3 coordsOffset = new Vector3(absWorldCoords.X - (double)coords.WorldX, absWorldCoords.Y - (double)coords.WorldY, absWorldCoords.Z - (double)coords.WorldZ);
                
                double adjustment = double.MaxValue;
                Direction? moveDirection = null;
                
                // Calculate the smallest distance needed to move the entity out of the block
                coords.ForAdjacent((aCoord, direction) =>
                {
                    byte? adjBlockId = World.GetBlockId(aCoord);

                    if (adjBlockId == null)
                        return;

                    var adjacentBlockClass = BlockHelper.Instance((byte)adjBlockId);
                    
                    if (!(adjacentBlockClass.IsOpaque && adjacentBlockClass.IsSolid))
                    {
                        switch (direction)
                        {
                            case Direction.South:
                                if (coordsOffset.X < adjustment)
                                {
                                    moveDirection = Direction.South;
                                    adjustment = coordsOffset.X;
                                }
                                break;
                            case Direction.North:
                                if (1.0 - coordsOffset.X < adjustment)
                                {
                                    moveDirection = Direction.North;
                                    adjustment = 1.0 - coordsOffset.X;
                                }
                                break;
                            case Direction.Down:
                                if (coordsOffset.Y < adjustment)
                                {
                                    moveDirection = Direction.Down;
                                    adjustment = coordsOffset.Y;
                                }
                                break;
                            case Direction.Up:
                                if (1.0 - coordsOffset.Y < adjustment)
                                {
                                    moveDirection = Direction.Up;
                                    adjustment = 1.0 - coordsOffset.Y;
                                }
                                break;                                
                            case Direction.East:
                                if (coordsOffset.Z < adjustment)
                                {
                                    moveDirection = Direction.East;
                                    adjustment = coordsOffset.Z;
                                }
                                break;
                            case Direction.West:
                                if (coordsOffset.Z < adjustment)
                                {
                                    moveDirection = Direction.West;
                                    adjustment = 1.0 - coordsOffset.Z;
                                }
                                break;
                        }
                    }
                });
                
                double motion = this.Server.Rand.NextDouble() * 0.2 + 0.1;
                if (moveDirection.HasValue)
                {
                    if (moveDirection.Value == Direction.South)
                        this.Velocity.X = -motion;
                    else if (moveDirection.Value == Direction.North)
                        this.Velocity.X = motion;
                    else if (moveDirection.Value == Direction.Down)
                        this.Velocity.Y = -motion;
                    else if (moveDirection.Value == Direction.Up)
                        this.Velocity.Y = motion;
                    else if (moveDirection.Value == Direction.East)
                        this.Velocity.Z = -motion;
                    else if (moveDirection.Value == Direction.West)
                        this.Velocity.Z = motion;
                }
            }
        }

        public virtual List<StructBlock> GetNearbyBlocks()
        {
            BoundingBox touchCheckBoundingBox = this.BoundingBox.Contract(new Vector3(0.001, 0.001, 0.001));

            List<StructBlock> touchedBlocks = new List<StructBlock>();

            // Notify blocks of collisions with an entity
            UniversalCoords minCoords = UniversalCoords.FromAbsWorld(touchCheckBoundingBox.Minimum.X, touchCheckBoundingBox.Minimum.Y, touchCheckBoundingBox.Minimum.Z);
            UniversalCoords maxCoords = UniversalCoords.FromAbsWorld(touchCheckBoundingBox.Maximum.X, touchCheckBoundingBox.Maximum.Y, touchCheckBoundingBox.Maximum.Z);
            if (this.World.ChunkExists(minCoords) && this.World.ChunkExists(maxCoords))
            {
                for (int x = minCoords.WorldX; x <= maxCoords.WorldX; x++)
                {
                    for (int y = minCoords.WorldY; y <= maxCoords.WorldY; y++)
                    {
                        for (int z = minCoords.WorldZ; z <= maxCoords.WorldZ; z++)
                        {
                            var block = this.World.GetBlock(x, y, z);
                            touchedBlocks.Add(block);
                        }
                    }
                }
            }
            return touchedBlocks;
        }

        public virtual void TouchNearbyBlocks()
        {
            // TODO: notify blocks of collisions + play sounds
            BoundingBox touchCheckBoundingBox = this.BoundingBox.Contract(new Vector3(0.001, 0.001, 0.001));

            Vector3 thisPosition = this.Position.ToVector();

            // Notify blocks of collisions with an entity
            UniversalCoords minCoords = UniversalCoords.FromAbsWorld(touchCheckBoundingBox.Minimum.X, touchCheckBoundingBox.Minimum.Y, touchCheckBoundingBox.Minimum.Z);
            UniversalCoords maxCoords = UniversalCoords.FromAbsWorld(touchCheckBoundingBox.Maximum.X, touchCheckBoundingBox.Maximum.Y, touchCheckBoundingBox.Maximum.Z);
            if (this.World.ChunkExists(minCoords) && this.World.ChunkExists(maxCoords))
            {
                for (int x = minCoords.WorldX; x <= maxCoords.WorldX; x++)
                {                 
                    for (int z = minCoords.WorldZ; z <= maxCoords.WorldZ; z++)
                    {
                        Chunk chunk = World.GetChunkFromWorld(x, z, false, false);

                        if (chunk == null)
                            continue;

                        for (int y = minCoords.WorldY; y <= maxCoords.WorldY; y++)
                        {
                            var block = chunk.GetBlock(x, y, z);
                            if (block.Type > 0)
                            {
                                var blockClass = BlockHelper.Instance(block.Type);

                                #region Calculate closest face of block (precedence, x, z, y)
                                Vector3 v = thisPosition - new Vector3(x, y, z);

                                double vx = Math.Abs(v.X);
                                double vy = Math.Abs(v.Y);
                                double vz = Math.Abs(v.Z);

                                BlockFace face = BlockFace.North;

                                if (vx <= vy && vx <= vz)
                                {
                                    if (v.X > 0.0)
                                    {
                                        face = BlockFace.North;
                                    }
                                    else
                                    {
                                        face = BlockFace.South;
                                    }
                                }
                                else if (vz <= vx && vz <= vy)
                                {
                                    if (v.Z > 0.0)
                                    {
                                        face = BlockFace.West;
                                    }
                                    else
                                    {
                                        face = BlockFace.East;
                                    }
                                }
                                else if (vy <= vx && vy <= vz)
                                {
                                    if (v.Y > 0.0)
                                    {
                                        face = BlockFace.Up;
                                    }
                                    else
                                    {
                                        face = BlockFace.Down;
                                    }
                                }
                                #endregion

                                blockClass.Touch(this, block, face);
                            }
                        }
                    }
                }
            }
            
            

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

            TouchNearbyBlocks();
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
            Server.PluginManager.CallEvent(Plugins.Events.Event.EntityMove, e);
            if (e.EventCanceled) return;
            newPosition = e.NewPosition;
            //End Event

            sbyte oldPacketPosX = (sbyte)Math.Floor(Position.X * 32.0);
            sbyte oldPacketPosY = (sbyte)Math.Floor(Position.Y * 32.0);
            sbyte oldPacketPosZ = (sbyte)Math.Floor(Position.Z * 32.0);

            sbyte newPacketPosX = (sbyte)Math.Floor(newPosition.X * 32.0);
            sbyte newPacketPosY = (sbyte)Math.Floor(newPosition.Y * 32.0);
            sbyte newPacketPosZ = (sbyte)Math.Floor(newPosition.Z * 32.0);

            sbyte dx = (sbyte)(newPacketPosX - oldPacketPosX);
            sbyte dy = (sbyte)(newPacketPosY - oldPacketPosY);
            sbyte dz = (sbyte)(newPacketPosZ - oldPacketPosZ);
            Position = newPosition; // TODO: this doesn't prevent changing the Position by more than 4 blocks

            OnMoveTo(dx, dy, dz);
        }

        public virtual void OnMoveTo(sbyte x, sbyte y, sbyte z)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                if (!ToSkip(c))
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
                if (!ToSkip(c))
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
                if (!ToSkip(c))
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
            Server.PluginManager.CallEvent(Plugins.Events.Event.EntityMove, e);
            if (e.EventCanceled) return;
            newPosition = e.NewPosition;
            //End Event

            sbyte oldPacketPosX = (sbyte)Math.Floor(Position.X * 32.0);
            sbyte oldPacketPosY = (sbyte)Math.Floor(Position.Y * 32.0);
            sbyte oldPacketPosZ = (sbyte)Math.Floor(Position.Z * 32.0);

            sbyte newPacketPosX = (sbyte)Math.Floor(newPosition.X * 32.0);
            sbyte newPacketPosY = (sbyte)Math.Floor(newPosition.Y * 32.0);
            sbyte newPacketPosZ = (sbyte)Math.Floor(newPosition.Z * 32.0);

            sbyte dx = (sbyte)(newPacketPosX - oldPacketPosX);
            sbyte dy = (sbyte)(newPacketPosY - oldPacketPosY);
            sbyte dz = (sbyte)(newPacketPosZ - oldPacketPosZ);

            Position = newPosition;
            this.Yaw = yaw;
            this.Pitch = pitch;

            OnMoveRotateTo(dx, dy, dz);
            
        }

        public virtual void OnMoveRotateTo(sbyte x, sbyte y, sbyte z)
        {
            foreach (Client c in Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                if (!ToSkip(c))
                    c.SendMoveRotateBy(this, x, y, z, PackedYaw, PackedPitch);
            }
        }

        public virtual bool ToSkip(Client c)
        {
            return false;
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
        Cactus,
        Custom
    }
}
