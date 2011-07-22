using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Net
{
	public class IncrementStatisticPacket : Packet
	{
		public int Statistic { get; set; }
		public byte Amount { get; set; }

		public override void Read(BigEndianStream stream)
		{
			Statistic = stream.ReadInt();
			Amount = stream.ReadByte();
		}

		public override void Write(BigEndianStream stream)
		{
			stream.Write(Statistic);
			stream.Write(Amount);
		}

		public enum Statistics : int
		{
			StartGame = 1000,
			CreateWorld = 1001,
			LoadWorld = 1002,
			JoinMultiplayer = 1003,
			LeaveGame = 1004,
			PlayOneMinute = 1100,
			WalkOneCm = 2000,
			SwimOneCm = 2001,
			FallOneCm = 2002,
			ClimbOneCm = 2003,
			FlyOneCm = 2004,
			DiveOneCm = 2005,
			MinecartOneCm = 2006,
			BoatOneCm = 2007,
			PigOneCm = 2008,
			Jump = 2010,
			Drop = 2011,
			DamageDealt = 2020,
			DamageTaken = 2021,
			Deaths = 2022,
			MobKills = 2023,
			PlayerKills = 2024,
			FishCaught = 2025,
			MineBlock = 16777216,	// Note: Add an item ID to this value
			CraftItem = 16842752,	// Note: Add an item ID to this value
			UseItem = 16908288,		// Note: Add an item ID to this value
			BreakItem = 16973824	// Note: Add an item ID to this value
		}
	}
}
