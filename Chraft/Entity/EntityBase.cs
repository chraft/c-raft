using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Chraft;
using Chraft.Net;
using Chraft.Utils;
using Chraft.World;
using Chraft.World.NBT;
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
        //public double X { get; set; }
        //public double Y { get; set; }
        //public double Z { get; set; }
        public float Yaw { get; set; }
        public float Pitch { get; set; }
        public short Health { get; set; }
        public short MaxHealth { get; set; }
        public sbyte PackedYaw { get { return (sbyte)(Yaw / 360.0f * 256.0f % 256.0f); } }
        public sbyte PackedPitch { get { return (sbyte)(Pitch / 360.0f * 256.0f % 256.0f); } }
        public Server Server { get; private set; }
        public int TimeInWorld;
        public Vector3 Position;

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
            EntityMoveEventArgs e = new EntityMoveEventArgs(this, newPosition, Position);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_MOVE, e);
            if (e.EventCanceled) return;
            newPosition = e.NewPosition;
            //End Event

            sbyte dx = (sbyte)(32 * (newPosition.X - Position.X));
            sbyte dy = (sbyte)(32 * (newPosition.Y - Position.Y));
            sbyte dz = (sbyte)(32 * (newPosition.Z - Position.Z));
            Position = newPosition;
            
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Equals(this))
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
            //TODO: Fix bug that sets the users Position.Y to .0 insted of .5
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                //if (!c.Equals(this))
                    c.SendTeleportTo(this);
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
            Yaw = yaw;
            Pitch = pitch;
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Equals(this))
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
            EntityMoveEventArgs e = new EntityMoveEventArgs(this, newPosition, Position);
            Server.PluginManager.CallEvent(Plugins.Events.Event.ENTITY_MOVE, e);
            if (e.EventCanceled) return;
            newPosition = e.NewPosition;
            //End Event

            sbyte dx = (sbyte)(32 * (newPosition.X - Position.X));
            sbyte dy = (sbyte)(32 * (newPosition.Y - Position.Y));
            sbyte dz = (sbyte)(32 * (newPosition.Z - Position.Z));
            Position = newPosition;
            Yaw = yaw;
            Pitch = pitch;
            foreach (Client c in Server.GetNearbyPlayers(World, Position.X, Position.Y, Position.Z))
            {
                if (!c.Equals(this))
                    c.SendMoveRotateBy(this, dx, dy, dz, PackedYaw, PackedPitch);
            }
        }

        public bool Equals(EntityBase other)
        {
            return other.EntityId == EntityId;
        }
    }
}
