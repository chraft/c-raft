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
using Chraft.Net.Packets;
using Chraft.Utilities;
using Chraft.Utilities.Coords;
using Chraft.Utilities.Math;
using Chraft.Utils;

namespace Chraft.World.Blocks.Physics
{
    public abstract class BaseFallingPhysics : IBlockPhysics
    {
        public int EntityId { get; protected set; }
        public WorldManager World { get; protected set; }
        public AbsWorldCoords Position { get; protected set; }
        public bool IsPlaying { get; protected set; }
        public Vector3 Velocity { get; protected set; }
        public AddObjectVehiclePacket.ObjectType Type;

        protected BaseFallingPhysics(WorldManager world, AbsWorldCoords pos)
        {
            World = world;
            Position = pos;
            EntityId = world.Server.AllocateEntity();

            CreateEntityPacket entity = new CreateEntityPacket { EntityId = EntityId };
            World.Server.SendPacketToNearbyPlayers(World,
                                                   UniversalCoords.FromAbsWorld(Position),
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
                                                 Data = 0,
                                                 X = Position.X,
                                                 Y = Position.Y,
                                                 Z = Position.Z
                                             };
            World.Server.SendPacketToNearbyPlayers(World, 
                                                   UniversalCoords.FromAbsWorld(Position), 
                                                   obj);
            
            IsPlaying = true;
        }

        public virtual void Simulate()
        {
        }

        public virtual void Stop(bool forceStop = false)
        {
            IsPlaying = false;
            BaseFallingPhysics unused = null;
            World.PhysicsBlocks.TryRemove(EntityId, out unused);
            DestroyEntityPacket entity = new DestroyEntityPacket { EntitiesCount = 1, EntitiesId = new []{EntityId}};
            World.Server.SendPacketToNearbyPlayers(World,
                                                   UniversalCoords.FromAbsWorld(Position),
                                                   entity);
            if (!forceStop)
                OnStop();
        }

        protected virtual void OnStop()
        {
        }
    }
}
