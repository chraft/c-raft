using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;

namespace Chraft.Utils
{
	public class PermissionHandler : IPermissions
	{
		private PermissionConfiguration Users;
		private PermissionConfiguration Groups;
		private Dictionary<string, string> _groupOutValue = new Dictionary<string, string>();
		private Dictionary<string, string> _usersOutValue = new Dictionary<string, string>();
		public PermissionHandler(Server server)
		{
			Users = new PermissionConfiguration(server, "resources/users.ini");
			Groups = new PermissionConfiguration(server, "resources/groups.ini");
		}

		/// <summary>
		/// Check if a player has permission to use a command
		/// </summary>
		/// <param name="playerName"></param>
		/// <param name="command"></param>
		/// <returns>bool</returns>
		public bool CanUseCommand(string playerName, string command)
		{
			if (PlayerExists(playerName))
			{
				//Checks if they have something in users.ini first
				var userCommandList = (from c in _usersOutValue where c.Key == "commands" select c.Value).FirstOrDefault();
				if (!string.IsNullOrEmpty(userCommandList))
					if (userCommandList.Split(',').Any(userCommands => userCommands.ToLower() == command.ToLower() || userCommands.ToLower() == "*"))
					{
						return true;
					}
			}
			else
			{
				//assume default if the player is not in users.ini
				if (GroupExists("default"))
				{
					var defaultCommand = (from u in _groupOutValue where u.Key == "commands" select u.Value).FirstOrDefault();
					if (!string.IsNullOrEmpty(defaultCommand))
						return defaultCommand.Split(',').Any(defaultCommands => defaultCommands.ToLower() == command.ToLower() || defaultCommands.ToLower() == "*");
				}
			}

			//Now we check their groups
			var userGroups = (from c in _usersOutValue where c.Key == "groups" select c.Value).FirstOrDefault();
			if (!string.IsNullOrEmpty(userGroups))
			{
				//all their groups
				foreach (var groupCommand in userGroups.Split(',').Select(userGroup => (from c in Groups._iniFileContent[userGroup] where c.Key == "commands" select c.Value).FirstOrDefault()))
				{
					if (
						groupCommand.Split(',').Any(groupCommands => groupCommands.ToLower() == command.ToLower() || groupCommands.ToLower() == "*"))
					{
						return true;
					}
					continue;
				}

				// then if we are still going we check their inherited groups
				foreach (var userGroup in userGroups.Split(','))
				{
					foreach (var inherited in GetGroupInheritance(userGroup))
					{
						var inheritedCommand = (from u in Groups._iniFileContent[inherited] where u.Key == "commands" select u.Value).FirstOrDefault();
						if (string.IsNullOrEmpty(inheritedCommand)) continue;
						if (!inheritedCommand.Split(',').Any(inheritedCommands => inheritedCommands.ToLower() == command.ToLower() || inheritedCommands.ToLower() == "*"))
						{
							continue;
						}
						return true;
					}
				}
			}
			return false;
		}

		/// <summary>
		/// Check if a player is in a group
		/// </summary>
		/// <param name="playerName"></param>
		/// <param name="groupName"></param>
		/// <returns>bool</returns>
		public bool IsInGroup(string playerName, string groupName)
		{
			if (PlayerExists(playerName))
			{
				var userGroup = (from c in _usersOutValue where c.Key == "groups" select c.Value).FirstOrDefault();
				if (userGroup == null)
				{
					return false;
				}

				return userGroup.Split(',').Any(userGroups => userGroups.ToLower() == groupName.ToLower());
			}
			return false;
		}

		/// <summary>
		/// Return the suffix of a specific player
		/// </summary>
		/// <param name="playerName"></param>
		/// <returns>value or null</returns>
		public string GetPlayerSuffix(string playerName)
		{
			return PlayerExists(playerName) ? (from u in _usersOutValue where u.Key == "suffix" select u.Value).FirstOrDefault() : null;
		}

		/// <summary>
		/// Return the prefix of a specific player
		/// </summary>
		/// <param name="playerName"></param>
		/// <returns>value or null</returns>
		public string GetPlayerPrefix(string playerName)
		{
			return PlayerExists(playerName) ? (from u in _usersOutValue where u.Key == "prefix" select u.Value).FirstOrDefault() : null;
		}

		/// <summary>
		/// Return the prefix of a specific group
		/// </summary>
		/// <param name="groupName"></param>
		/// <returns>value or null</returns>
		public string GetGroupPrefix(string groupName)
		{
			return GroupExists(groupName) ? (from u in _groupOutValue where u.Key == "prefix" select u.Value).FirstOrDefault() : null;
		}

		/// <summary>
		/// Returns the suffix of a specific group
		/// </summary>
		/// <param name="groupName"></param>
		/// <returns>value or null</returns>
		public string GetGroupSuffix(string groupName)
		{
			return GroupExists(groupName) ? (from u in _groupOutValue where u.Key == "suffix" select u.Value).FirstOrDefault() : null;
		}

		/// <summary>
		/// Get the list of groups a group inherits
		/// </summary>
		/// <param name="groupName"></param>
		/// <returns>string[] list of groups</returns>
		public string[] GetGroupInheritance(string groupName)
		{
			return GroupExists(groupName) ? (from g in _groupOutValue where g.Key == "inherit" select g.Value).FirstOrDefault().Split(',') : null;
		}

		/// <summary>
		/// Checks if a player has a users.ini value
		/// </summary>
		/// <param name="playerName"></param>
		/// <returns>bool</returns>
		public bool PlayerExists(string playerName)
		{
			return Users._iniFileContent.TryGetValue(playerName.ToLower(), out _usersOutValue);
		}

		/// <summary>
		/// Checks if a group has a groups.ini value
		/// </summary>
		/// <param name="groupName"></param>
		/// <returns>bool</returns>
		private bool GroupExists(string groupName)
		{
			return Groups._iniFileContent.TryGetValue(groupName, out _groupOutValue);
		}

		/// <summary>
		/// Checks if the player is allowed to build
		/// </summary>
		/// <param name="playerName"></param>
		/// <returns>bool</returns>
		public bool CanPlayerBuild(string playerName)
		{
			if (!PlayerExists(playerName))
			{
				return (from b in Groups._iniFileContent["default"] where b.Key == "build" select b.Value).FirstOrDefault() == "true";
			}
			var buildValue = (from u in _usersOutValue where u.Key == "build" select u.Value).FirstOrDefault();
			if (!string.IsNullOrEmpty(buildValue))
			{
				return buildValue == "false" ? false : true;

			}
			foreach (var grp in GetPlayerGroups(playerName))
			{
				if ((from c in Groups._iniFileContent[grp] where c.Key == "build" select c.Value).FirstOrDefault() == "true")
					return true;
			}
			return false;
		}

		/// <summary>
		/// Gets the list of groups assinged to a player
		/// </summary>
		/// <param name="playerName"></param>
		/// <returns>string[] of groups</returns>
		public IEnumerable<string> GetPlayerGroups(string playerName)
		{
			return PlayerExists(playerName) ? (from c in _usersOutValue where c.Key == "groups" select c.Value).FirstOrDefault().Split(',') : new[] { "default" };
		}
	}
}