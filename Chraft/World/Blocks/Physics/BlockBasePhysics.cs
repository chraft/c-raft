using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Net.Packets;
using Chraft.Utils;

namespace Chraft.World.Blocks.Physics
{
    public abstract class BlockBasePhysics
    {
        public int EntityId { get; protected set; }
        public WorldManager World { get; protected set; }
        public Location Position { get; protected set; }
        public bool IsPlaying { get; protected set; }
        public Vector3 Velocity { get; protected set; }
        public AddObjectVehiclePacket.ObjectType Type;

        protected BlockBasePhysics(WorldManager world, Location pos)
        {
            World = world;
            Position = pos;
            EntityId = world.Server.AllocateEntity();

            CreateEntityPacket entity = new CreateEntityPacket { EntityId = EntityId };
            foreach (var nearbyPlayer in World.Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                nearbyPlayer.SendPacket(entity);
            }
        }

        public virtual void Start()
        {
            if (IsPlaying)
                return;
            AddObjectVehiclePacket obj = new AddObjectVehiclePacket
                                             {
                                                 EntityId = EntityId,
                                                 Type = Type,
                                                 FireBallThrowerEid = 0,
                                                 X = Position.X,
                                                 Y = Position.Y,
                                                 Z = Position.Z
                                             };
            foreach (var nearbyPlayer in World.Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                nearbyPlayer.SendPacket(obj);
            }
            IsPlaying = true;
        }

        public virtual void Simulate()
        {
        }

        public virtual void Stop()
        {
            IsPlaying = false;
            BlockBasePhysics unused = null;
            World.PhysicsBlocks.TryRemove(EntityId, out unused);
            DestroyEntityPacket entity = new DestroyEntityPacket { EntityId = EntityId };
            foreach (var nearbyPlayer in World.Server.GetNearbyPlayers(World, new AbsWorldCoords(Position.X, Position.Y, Position.Z)))
            {
                nearbyPlayer.SendPacket(entity);
            }
            OnStop();
        }

        protected virtual void OnStop()
        {
        }
    }
}
