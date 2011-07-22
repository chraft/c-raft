using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Chraft;
using Chraft.Net;
using Chraft.Utils;
using Chraft.World;
using org.bukkit.entity;

namespace Chraft.Entity
{
	/// <summary>
	/// Represents an entity, including clients (players), item drops, mobs, and vehicles.
	/// </summary>
	public abstract partial class EntityBase : IEquatable<EntityBase>
	{
		public int EntityId { get; private set; }
		public WorldManager World { get; set; }	
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public float Yaw { get; set; }
        public float Pitch { get; set; }
		public short Health { get; set; }
		public short MaxHealth { get; set; }
        public sbyte PackedYaw { get { return (sbyte)(Yaw / 360.0f * 256.0f % 256.0f); } }
		public sbyte PackedPitch { get { return (sbyte)(Pitch / 360.0f * 256.0f % 256.0f); } }
		public Server Server { get; private set; }
        public int TimeInWorld;
		public org.bukkit.entity.Entity Passenger { get; set; }
		public org.bukkit.entity.Vehicle Vehicle { get; set; }
		
        public EntityBase(Server server, int entityId)
		{
			this.Server = Server;
			this.EntityId = entityId;
            this.TimeInWorld = 0;
		}

		public void Mount(EntityBase vehicle)
		{
			vehicle.Passenger = this;
			Vehicle = vehicle;
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
			sbyte dx = (sbyte)(32 * (x - X));
			sbyte dy = (sbyte)(32 * (y - Y));
			sbyte dz = (sbyte)(32 * (z - Z));
			X = x;
			Y = y;
			Z = z;
			foreach (Client c in Server.GetNearbyPlayers(World, X, Y, Z))
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
			X = x;
			Y = y;
			Z = z;
			foreach (Client c in Server.GetNearbyPlayers(World, X, Y, Z))
			{
				if (!c.Equals(this))
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
			foreach (Client c in Server.GetNearbyPlayers(World, X, Y, Z))
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
			sbyte dx = (sbyte)(32 * (x - X));
			sbyte dy = (sbyte)(32 * (y - Y));
			sbyte dz = (sbyte)(32 * (z - Z));
			X = x;
			Y = y;
			Z = z;
			Yaw = yaw;
			Pitch = pitch;
			foreach (Client c in Server.GetNearbyPlayers(World, X, Y, Z))
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
