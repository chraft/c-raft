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
using System;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Chraft.Net;
using Chraft.Net.Packets;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Entity;
using Chraft.PluginSystem.Item;
using Chraft.PluginSystem.Net;
using Chraft.PluginSystem.Server;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utilities.Misc;
using Chraft.World;
using Chraft.Interfaces;
using Chraft.Utils;
using Chraft.World.Blocks;
using Chraft.World.Blocks.Base;
using Chraft.World.Paths;

namespace Chraft.Entity
{
	public abstract partial class Mob : LivingEntity, IMob
	{
        public MobType Type { get; set; }

        public short MinExp { get; protected set; }
        public short MaxExp { get; protected set; }


        /// <summary>
        /// The amount of damage this Mob can inflict
        /// </summary>
        public virtual short AttackStrength
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// Targets within this range will take damage
        /// </summary>
        public virtual int AttackRange
        {
            get
            {
                return 0;
            }
        }

        /// <summary>
        /// How far the mob can see (i.e. if aggressive a player will be hunted if seen within this range)
        /// </summary>
        public virtual int SightRange 
        {
            get
            {
                return 16;
            }
        }

	    public virtual int MaxSpawnedPerGroup
	    {
            get
            {
                return 4;
            }

	    }

        public virtual LivingEntity Target { get; set; }

        protected List<PathCoordinate> PathCoordinates { get; set; } 
    
        protected Mob(WorldManager world, int entityId, MobType type, MetaData data)
			: base(world.Server, entityId, data)
		{
            this.Type = type;
            this.World = world;
            this.MobUpdateFrequency = 1;
            this.Speed = 0.1;
		}

        protected virtual void DoInteraction(IClient client, IItemInventory item)
        {
        }

        protected override void DoUpdate()
        {
            base.DoUpdate();
            if (this.TicksInWorld % MobUpdateFrequency == 0)
            {
                DoMobUpdate();

                if (IsJumping)
                {
                    // If in liquid Velocity.Y += 0.04;
                    this.Jump();
                }
                if (this.Velocity > Vector3.Origin)
                {
                    //if (IsJumping)
                    //    Velocity.Y += 0.04;

                    // TODO: this.MoveTo might be called to frequently - this needs to be reviewed
                    
                    //Server.Logger.Log(LogLevel.Info, "Velocity BEFORE ApplyVelocity (XYZ): " + Velocity.ToString());
                    var coords = ApplyVelocity(this.Velocity);
                    //Server.Logger.Log(LogLevel.Info, "Velocity AFTER ApplyVelocity (XYZ): " + Velocity.ToString());
                    
                    //Server.Logger.Log(LogLevel.Info, String.Format("Mob pos: {0}, New Pos: {1}", this.Position, coords));
                    this.MoveTo(coords, (float)this.Yaw, (float)this.Pitch);

                    Velocity.X *= 0.8;
                    Velocity.Y *= 0.8;
                    Velocity.Z *= 0.8;
                    Velocity.Y -= 0.02; // Gravity
                    //Server.Logger.Log(LogLevel.Debug, this.Velocity.ToString());

                    double friction = 0.98;
                    if (OnGround)
                    {
                        friction = 0.58;

                        // TODO: adjust based on Block friction
                    }
                    //Velocity.X *= friction;
                    ////Velocity.Y *= 0.98;
                    //Velocity.Z *= friction;

                    if (OnGround)
                        Velocity.Y *= -0.5;
                }
            }
        }

        protected override void DoDeath(EntityBase killedBy)
        {
            base.DoDeath(killedBy);

            if (killedBy != null && killedBy is Player)
                DropExperienceOrbs();
        }


        /// <summary>
        /// Number of ticks between DoMobUpdate
        /// </summary>
	    protected int MobUpdateFrequency { get; set; }

        protected double Speed { get; set; }
	    private double _strafingMovement = 0.0;
        private double _forwardMovement = 0.0;
	    private Vector3? _nextPosition = null;
	    private int _pathPosition = 0;
        protected virtual void DoMobUpdate()
        {
            // Get the next position in the path that is further than twice the width of the current entity
            double width = this.Width*2.0;
            double widthSqr = width*width;

            if (PathCoordinates == null || PathCoordinates.Count == 0)
            {
                PathCoordinates = GetNewPath();
                _pathPosition = 0;
            }
            else
            {
                if (this._nextPosition == null || _nextPosition.Value.DistanceSquared(new Vector3(this.Position.X, _nextPosition.Value.Y, this.Position.Z)) < widthSqr)
                {
                    PathCoordinate pathCoordinate = PathCoordinates[0];
                    if (pathCoordinate == null)
                    {
                        PathCoordinates = null;
                    }
                    else
                    {
                        _pathPosition++;
                        PathCoordinates.RemoveAt(0);

                        Vector3? nextPosition = pathCoordinate.AsEntityPosition(this).ToVector();
                        
                        while (nextPosition != null &&
                               nextPosition.Value.DistanceSquared(new Vector3(this.Position.X, nextPosition.Value.Y,
                                                                              this.Position.Z)) < widthSqr)
                        {
                            if (PathCoordinates.Count == 0)
                            {
                                nextPosition = null;
                                PathCoordinates = null;
                            }
                            else
                            {
                                pathCoordinate = PathCoordinates[0];
                                PathCoordinates.RemoveAt(0);
                                nextPosition = pathCoordinate.AsEntityPosition(this).ToVector();
                                _pathPosition++;
                            }
                        }
                        _nextPosition = nextPosition;
                        //Server.Logger.Log(LogLevel.Info, "Path position {0}", _pathPosition);
                        //if (_nextPosition != null)
                        //    Server.Logger.Log(LogLevel.Info, _nextPosition.Value.ToString());

                    }
                }

                IsJumping = false;

                if (_nextPosition != null)
                {
                    double currentY = Math.Floor(BoundingBox.Minimum.Y + 0.5);

                    Vector3 difference = _nextPosition.Value -
                                         new Vector3(this.Position.X, currentY, this.Position.Z);

                    //Server.Logger.Log(LogLevel.Info, "Velocity to nextPosition (XYZ): " + Velocity.ToString());

                    double newYaw = ((Math.Atan2(difference.Z, difference.X) * 180.0) / Math.PI) - 90.0;
                    double yawMovement = (newYaw - Yaw) % 360.0;
                    _forwardMovement = Speed;
                    for (; yawMovement < -180F; yawMovement += 360F)
                    {
                    }
                    for (; yawMovement >= 180F; yawMovement -= 360F)
                    {
                    }

                    Yaw += yawMovement.Clamp(-30.0, 30.0);

                    if (Target != null)
                    {
                        double xDiff = Target.Position.X - Position.X;
                        double zDiff = Target.Position.Z - Position.Z;
                        double previousYaw = Yaw;
                        Yaw = Math.Atan2(zDiff, xDiff).ToDegrees() - 90.0;
                        double turnDirection = ((previousYaw - Yaw /*+ 90.0*/)).ToRadians();
                        _strafingMovement = -Math.Sin(turnDirection) * Speed * 1.0;
                        _forwardMovement = Math.Cos(turnDirection) * Speed * 1.0;
                        //Server.Logger.Log(LogLevel.Debug, "PrevYaw: {0}, NewYaw: {1}, Turn: {2}", previousYaw, Yaw, turnDirection);
                    }

                    if (difference.Y > 0.0)
                    {
                        IsJumping = true;
                    }
                }

                if (Target != null)
                {
                    this.FaceEntity(Target, 30, 30);
                    //Server.Logger.Log(LogLevel.Debug, "FacingYaw: {0}", Yaw);
                }

                _strafingMovement *= 0.98;
                _forwardMovement *= 0.98;

                #region Calculate Friction
                double blockFrictionFactor = 0.91;
                if (OnGround)
                {
                    blockFrictionFactor = 0.55;

                    StructBlock block = (StructBlock)World.GetBlock(UniversalCoords.FromAbsWorld(this.Position.X, this.Position.Y - 1, this.Position.Z));
                    if (block.Type > 0)
                    {
                        BlockBase blockInstance = BlockHelper.Instance.CreateBlockInstance(block.Type);

                        if (blockInstance != null)
                        {
                            blockFrictionFactor = blockInstance.Slipperiness * 0.91;
                        }
                    }
                }

                double f3 = 0.163 / (blockFrictionFactor * blockFrictionFactor * blockFrictionFactor);
                double f4 = OnGround ? 1.0 * f3 : 0.02;
                #endregion
                
                ApplyStrafingToVelocity(_strafingMovement, _forwardMovement, f4);
                
                foreach (var e in World.GetEntitiesWithinBoundingBoxExcludingEntity(this, BoundingBox.Expand(new Vector3(0.2, 0.0, 0.2))))
                {
                    EntityBase entity = e as EntityBase;
                    if (entity.Pushable)
                    {
                        entity.ApplyEntityCollision(this);
                    }
                }
            }
        }

        /// <summary>
        /// Applies left/right/forward movement based on the current entity's yaw
        /// </summary>
        /// <param name="strafMovement"></param>
        /// <param name="forwardMovement"></param>
        /// <param name="friction"></param>
        protected void ApplyStrafingToVelocity(double strafMovement, double forwardMovement, double friction)
        {
            double distance = Math.Sqrt(strafMovement*strafMovement + forwardMovement*forwardMovement);
            if (distance < 0.01)
            {
                return;
            }
            if (distance < 1.0)
            {
                distance = 1.0;
            }

            distance = friction / distance;
            strafMovement *= distance;
            forwardMovement *= distance;

            // Based on current yaw and xMovement and zMovement, determine the velocity along X / Z axis
            double yawSin = Math.Sin(Yaw.ToRadians());
            double yawCos = Math.Cos(Yaw.ToRadians());

            this.Velocity.X += strafMovement * yawCos - forwardMovement * yawSin;
            this.Velocity.Z += forwardMovement * yawCos + strafMovement * yawSin;
        }

        /// <summary>
        /// Returns a path for the Mob to follow
        /// </summary>
        /// <returns></returns>
        protected virtual List<PathCoordinate> GetNewPath()
        {
            return null;
        }

	    /// <summary>
        /// Looks for a player to attack
        /// </summary>
        /// <returns></returns>
        protected Player FindPlayerToAttack()
        {
            Player player = World.GetClosestPlayer(this.Position, this.SightRange) as Player;
            if (player != null && CanSee(player))
            {
                return player;
            }
            return null;
        }

        /// <summary>
        /// When a player interacts with a mob (right-click) with an item / hand
        /// </summary>
        /// <param name="client">The client that is interacting</param>
        /// <param name="item">The item being used (could be Void e.g. Hand)</param>
        public void InteractWith(IClient client, IItemInventory item)
        {
            // TODO: create a plugin event for this action

            DoInteraction(client, item);
        }

        public override void Attack(ILivingEntity target)
        {
            if (target == null)
                return;
            target.Damage(DamageCause.EntityAttack, AttackStrength, this);
        }

        public void Despawn()
        {
            Server.RemoveEntity(this);
            // TODO: do we need to notify clients of the removal of the entity?
        }


        protected virtual double BlockPathWeight(UniversalCoords coords)
        {
            return 0.0;
        }

        /// <summary>
        /// In addition to <see cref="LivingEntity.CanSpawnHere"/> determine if the BlockPathWeight of the location is suitable for spawning
        /// </summary>
        /// <returns>True if the <see cref="Mob"/> can spawn at <see cref="EntityBase.Position"/>; otherwise False</returns>
        public override bool CanSpawnHere()
        {
            return base.CanSpawnHere() && BlockPathWeight(this.BlockPosition) >= 0.0F;
        }

        protected void DropExperienceOrbs()
        {
            short minExp = (MinExp < 0 ? (short)0 : MinExp);
            short maxExp = (MaxExp > short.MaxValue ? short.MaxValue : MaxExp);

            if (maxExp < 1 || maxExp < minExp)
                return;

            short exp = (short)(minExp + Server.Rand.Next(0, maxExp - minExp));

            if (exp < 1)
                return;

            var orb = new ExpOrbEntity(Server, Server.AllocateEntity(), exp);
            orb.Position = Position;
            Server.AddEntity(orb);
        }
	}
}
