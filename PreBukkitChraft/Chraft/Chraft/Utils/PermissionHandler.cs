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
        private Dictionary<string ,string > _usersOutValue = new Dictionary<string, string>();
        public PermissionHandler(Server server)
        {
            Users = new PermissionConfiguration(server, "resources/users.ini");
            Groups = new PermissionConfiguration(server, "resources/groups.ini");
        }

        public bool CanUseCommand(string playerName, string command)
        {
           
           

            if (PlayerExists(playerName))
            {
                var userCommandList = (from c in _usersOutValue where c.Key == "commands" select c.Value).FirstOrDefault();
                if (userCommandList != null)
                    if (userCommandList.Split(',').Any(userCommands => userCommands.ToLower() == command.ToLower() || userCommands.ToLower() == "*"))
                    {
                        return true;
                    }
            }
            else
            {
                //assume default
                if (GroupExists("default"))
                {
                    var defaultCommand = (from u in _groupOutValue where u.Key == "commands" select u.Value).FirstOrDefault();
                    if (defaultCommand != null)
                        return defaultCommand.Split(',').Any(defaultCommands => defaultCommands.ToLower() == command.ToLower() || defaultCommands.ToLower() == "*");
                }
            }

            var userGroups = (from c in _usersOutValue where c.Key == "groups" select c.Value).FirstOrDefault();
            if (userGroups != null)
                foreach (var groupCommand in userGroups.Split(',').Select(userGroup => (from c in Groups._iniFileContent[userGroup] where c.Key == "commands" select c.Value).FirstOrDefault()))
                {
                    if (groupCommand.Split(',').Any(groupCommands => groupCommands.ToLower() == command.ToLower() || groupCommands.ToLower() == "*"))
                    {
                        return true;
                    }
                    continue;
                }
          // var inheritance = from i in userGroups 
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
                var userGroup =(from c in _usersOutValue where c.Key == "groups" select c.Value).FirstOrDefault();
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

        public string[] GetGroupInheritance(string groupName)
        {
            return GroupExists(groupName) ? (from g in _groupOutValue where g.Key == "inherit" select g.Value).FirstOrDefault().Split(',') : null;
        }

        public bool PlayerExists(string playerName)
        {
            return Users._iniFileContent.TryGetValue(playerName, out _usersOutValue);
        }

        public bool GroupExists(string groupName)
        {
            return Groups._iniFileContent.TryGetValue(groupName, out _groupOutValue);
        }

    }
}