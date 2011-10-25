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
        public AbsWorldCoords Position { get; protected set; }
        public bool IsPlaying { get; protected set; }
        public Vector3 Velocity { get; protected set; }
        public AddObjectVehiclePacket.ObjectType Type;

        protected BlockBasePhysics(WorldManager world, AbsWorldCoords pos)
        {
            World = world;
            Position = pos;
            EntityId = world.Server.AllocateEntity();

            CreateEntityPacket entity = new CreateEntityPacket { EntityId = EntityId };
            World.Server.SendPacketToNearbyPlayers(World,
                                                   UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z),
                                                   entity);
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
            World.Server.SendPacketToNearbyPlayers(World, 
                                                   UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z), 
                                                   obj);
            
            IsPlaying = true;
        }

        public virtual void Simulate()
        {
        }

        public virtual void Stop(bool forceStop = false)
        {
            IsPlaying = false;
            BlockBasePhysics unused = null;
            World.PhysicsBlocks.TryRemove(EntityId, out unused);
            DestroyEntityPacket entity = new DestroyEntityPacket { EntityId = EntityId };
            World.Server.SendPacketToNearbyPlayers(World,
                                                   UniversalCoords.FromAbsWorld(Position.X, Position.Y, Position.Z),
                                                   entity);
            if (!forceStop)
                OnStop();
        }

        protected virtual void OnStop()
        {
        }
    }
}
