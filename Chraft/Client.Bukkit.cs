using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.Interfaces;
using org.bukkit;
using java.net;
using org.bukkit.entity;
using org.bukkit.util;
using org.bukkit.inventory;
using org.bukkit.command;
using org.bukkit.block;
using org.bukkit.material;

namespace Chraft
{
	public partial class Client : Player
	{
		public void chat(string str)
		{
			OnChat(str);
		}

		public InetSocketAddress getAddress()
		{
			throw new NotImplementedException();
		}

		public Location getCompassTarget()
		{
			throw new NotImplementedException();
		}

		public string getDisplayName()
		{
			return DisplayName;
		}

		public bool isOnline()
		{
			// HACK: Properly determine this
			return true;
		}

		public bool isSneaking()
		{
			return IsSneaking;
		}

		public void kickPlayer(string str)
		{
			Kick(str);
		}

		public void loadData()
		{
			Load();
		}

		public bool performCommand(string str)
		{
			if (str == null)
				return false;
			return Server.dispatchCommand(this, str);
		}

		public void saveData()
		{
			Save();
		}

		public void sendRawMessage(string str)
		{
			// TODO: Determine the difference between sendMessage and sendRawMessage
			SendMessage(str);
		}

		public void setCompassTarget(Location l)
		{
			throw new NotImplementedException();
		}

		public void setDisplayName(string str)
		{
			DisplayName = str;
		}

		public void setSneaking(bool b)
		{
			throw new NotImplementedException();
		}

		[Obsolete]
		public void updateInventory()
		{
		}

		public bool isOp()
		{
			throw new NotImplementedException();
		}

		public void sendMessage(string str)
		{
			SendMessage(str);
		}

		public void damage(int i, org.bukkit.entity.Entity e)
		{
			throw new NotImplementedException();
		}

		public void damage(int i)
		{
			throw new NotImplementedException();
		}

		public double getEyeHeight(bool b)
		{
			throw new NotImplementedException();
		}

		public double getEyeHeight()
		{
			throw new NotImplementedException();
		}

		public Location getEyeLocation()
		{
			throw new NotImplementedException();
		}

		public int getHealth()
		{
			throw new NotImplementedException();
		}

		public int getLastDamage()
		{
			throw new NotImplementedException();
		}

		public java.util.List getLastTwoTargetBlocks(java.util.HashSet hs, int i)
		{
			throw new NotImplementedException();
		}

		public java.util.List getLineOfSight(java.util.HashSet hs, int i)
		{
			throw new NotImplementedException();
		}

		public int getMaximumAir()
		{
			throw new NotImplementedException();
		}

		public int getMaximumNoDamageTicks()
		{
			throw new NotImplementedException();
		}

		public int getNoDamageTicks()
		{
			throw new NotImplementedException();
		}

		public int getRemainingAir()
		{
			throw new NotImplementedException();
		}

		public Block getTargetBlock(java.util.HashSet hs, int i)
		{
			throw new NotImplementedException();
		}

		public Vehicle getVehicle()
		{
			return Vehicle;
		}

		public bool isInsideVehicle()
		{
			return Vehicle != null;
		}

		public bool leaveVehicle()
		{
			throw new NotImplementedException();
		}

		public void setHealth(int i)
		{
			throw new NotImplementedException();
		}

		public void setLastDamage(int i)
		{
			throw new NotImplementedException();
		}

		public void setMaximumAir(int i)
		{
			throw new NotImplementedException();
		}

		public void setMaximumNoDamageTicks(int i)
		{
			throw new NotImplementedException();
		}

		public void setNoDamageTicks(int i)
		{
			throw new NotImplementedException();
		}

		public void setRemainingAir(int i)
		{
			throw new NotImplementedException();
		}

		public Arrow shootArrow()
		{
			throw new NotImplementedException();
		}

		public Egg throwEgg()
		{
			throw new NotImplementedException();
		}

		public Snowball throwSnowball()
		{
			throw new NotImplementedException();
		}

		public PlayerInventory getInventory()
		{
			return Inventory;
		}

		public ItemStack getItemInHand()
		{
            return null;
			//return (ItemStackBukkit)Inventory.ActiveItem;
		}

		public string getName()
		{
			return Username;
		}

		public int getSleepTicks()
		{
			return SleepTicks >= 0 ? SleepTicks : 0;
		}

		public bool isSleeping()
		{
			return SleepTicks >= 0;
		}

		public void setItemInHand(ItemStack @is)
		{
			Inventory.setItemInHand(@is);
		}

		public override float getFallDistance()
		{
			return (float)(LeftGround.Y - Y);
		}
	}
}
