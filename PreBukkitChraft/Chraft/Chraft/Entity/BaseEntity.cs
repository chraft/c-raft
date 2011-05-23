using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using Chraft;
using Chraft.Net;
using Chraft.Utils;

namespace Chraft.Entity
{

	public class BaseEntity
	{
		public int EntityId { get; private set; }
		public double X { get; set; }
		public double Y { get; set; }
		public double Z { get; set; }
		public float Yaw { get; set; }
		public float Pitch { get; set; }

		public BaseEntity(int entityId)
		{
			EntityId = entityId;
		}

		public virtual void MoveTo(double x, double y, double z)
		{
			sbyte dx = (sbyte)(32 * (X - x));
			sbyte dy = (sbyte)(32 * (Y - y));
			sbyte dz = (sbyte)(32 * (Z - z));
			X = x;
			Y = y;
			Z = z;
			foreach (Client c in Program.GetNearbyPlayers(X, Y, Z))
			{
				if (c != this)
					c.SendMoveBy(this, dx, dy, dz);
			}
		}

		public void TeleportTo(double x, double y, double z)
		{
			throw new NotImplementedException();
		}

		public void RotateTo(float yaw, float pitch)
		{
			byte dyaw = (byte)((Yaw - yaw) / 360.0f * 256.0f % 256);
			byte dpitch = (byte)((Pitch - pitch) / 360.0f * 256.0f % 256);
			Yaw = yaw;
			Pitch = pitch;
			foreach (Client c in Program.GetNearbyPlayers(X, Y, Z))
			{
				if (c != this)
					c.SendRotateBy(this, dyaw, dpitch);
			}
		}

		public virtual void MoveTo(double x, double y, double z, float yaw, float pitch)
		{
			sbyte dx = (sbyte)(32 * (X - x));
			sbyte dy = (sbyte)(32 * (Y - y));
			sbyte dz = (sbyte)(32 * (Z - z));
			byte dyaw = (byte)((Yaw - yaw) / 360.0f * 256.0f % 256);
			byte dpitch = (byte)((Pitch - pitch) / 360.0f * 256.0f % 256);
			X = x;
			Y = y;
			Z = z;
			Yaw = yaw;
			Pitch = pitch;
			foreach (Client c in Program.GetNearbyPlayers(X, Y, Z))
			{
				if (c != this)
					c.SendMoveRotateBy(this, dx, dy, dz, dyaw, dpitch);
			}
		}
	}
}
