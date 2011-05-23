using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Entity
{
	public abstract partial class EntityBase : org.bukkit.entity.Entity, org.bukkit.entity.Vehicle
	{
		public bool eject()
		{
			if (Passenger == null)
				return false;
			if (Passenger is EntityBase)
				((EntityBase)Passenger).Vehicle = null;
			Passenger = null;
			return true;
		}

		public int getEntityId()
		{
			return EntityId;
		}

		public virtual float getFallDistance()
		{
			throw new NotImplementedException();
		}

		public int getFireTicks()
		{
			throw new NotImplementedException();
		}

		public org.bukkit.Location getLocation()
		{
			return new org.bukkit.Location(World, X, Y, Z);
		}

		public int getMaxFireTicks()
		{
			throw new NotImplementedException();
		}

		public java.util.List getNearbyEntities(double d1, double d2, double d3)
		{
			java.util.List retval = new java.util.ArrayList();
			foreach (EntityBase e in Server.GetEntities())
			{
				if (e.World == World && Math.Abs(X - e.X) <= d1 && Math.Abs(Y - e.Y) <= d2 && Math.Abs(Z - e.Z) <= d3)
					retval.add(e);
			}
			return retval;
		}

		public org.bukkit.entity.Entity getPassenger()
		{
			return Passenger;
		}

		public org.bukkit.Server getServer()
		{
			return Server;
		}

		public org.bukkit.util.Vector getVelocity()
		{
			throw new NotImplementedException();
		}

		public org.bukkit.World getWorld()
		{
			return World;
		}

		public bool isDead()
		{
			return Health <= 0;
		}

		public bool isEmpty()
		{
			return Passenger == null;
		}

		public void remove()
		{
			lock (Server.Entities)
				Server.Entities.Remove(this);
		}

		public void setFallDistance(float f)
		{
			throw new NotImplementedException();
		}

		public void setFireTicks(int i)
		{
			throw new NotImplementedException();
		}

		public bool setPassenger(org.bukkit.entity.Entity e)
		{
			if (Passenger == e)
				return false;
			Passenger = e;
			return true;
		}

		public void setVelocity(org.bukkit.util.Vector v)
		{
			throw new NotImplementedException();
		}

		public bool teleport(org.bukkit.entity.Entity e)
		{
			return teleport(e.getLocation());
		}

		public bool teleport(org.bukkit.Location l)
		{
			return TeleportTo(l.getX(), l.getY(), l.getZ());
		}

		[Obsolete]
		public void teleportTo(org.bukkit.entity.Entity e)
		{
			teleport(e);
		}

		[Obsolete]
		public void teleportTo(org.bukkit.Location l)
		{
			teleport(l);
		}
	}
}
