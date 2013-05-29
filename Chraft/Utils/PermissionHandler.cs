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
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Chraft.Commands;
using Chraft.Net;
using Chraft.Entity;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Server;


namespace Chraft.Utils
{
    public class PermissionHandler : IPermissions
    {
    
        private static XDocument _permissionXml;
        private const string Permfile = "Resources/Permissions.xml";
        private static Server _server;

        public PermissionHandler(Server server)
        {
            _server = server;
            _permissionXml = Load(Permfile);
        }

        public ClientPermission LoadClientPermission(Client client)
        {
            //TODO - use ConfigurationClass for loading things
            var p = new ClientPermission
                        {
                            Groups = new List<string>(),
                            AllowedPermissions = new List<string>(),
                            DeniedPermissions = new List<string>()
                        };
            var preAllowList = new List<string>();
            var preDisallowedList = new List<string>();
            var perm = _permissionXml.Descendants("Users").Descendants("User").FirstOrDefault(n => ((string) n.Attribute("Name")).ToLower() == client.Username.ToLower());

            //default group we grab the first with default attrbute defined
            var gperm = _permissionXml.Descendants("Groups").Descendants("Group").FirstOrDefault(n => (string) n.Attribute("IsDefault") == "true");
            if (gperm == null)
            {
                //no default defined
                _server.Logger.Log(LogLevel.Warning,
                                   "Required default group is not defined in permissions file. Add IsDefault=\"true\" to a group");
                return null;
            }
            if (perm != null)
            {
                if (perm.Attribute("Groups") == null)
                {
                    p.Groups.Add((string) gperm.Attribute("Name"));
                }
                else
                {
                    p.Groups.AddRange(perm.Attribute("Groups").Value.Split(','));
                }
                p.Prefix = perm.Element("Prefix") == null ? string.Empty : perm.Element("Prefix").Value;
                p.Suffix = perm.Element("Suffix") == null ? string.Empty : perm.Element("Suffix").Value;
                p.CanBuild = bool.Parse(perm.Element("CanBuild").Value);
                foreach (var element in perm.Element("Permission").Elements())
                {
                    if (element.Name == "Allowed")
                    {
                        preAllowList.Add(element.Value);
                    }
                    if (element.Name == "Disallowed")
                    {
                        preDisallowedList.Add(element.Value);
                    }
                }
                if (p.Groups != null)
                {
                    foreach (
                        var el in
                            p.Groups.Select(
                                s =>
                                _permissionXml.Descendants("Groups").Descendants("Group").Where(
                                    n => ((string) n.Attribute("Name")).ToLower() == s.ToLower())).SelectMany(groupPerm => groupPerm)
                        )
                    {
                        if (string.IsNullOrEmpty(p.Prefix))
                        {
                            p.Prefix = el.Element("Prefix") == null ? string.Empty : el.Element("Prefix").Value;
                        }
                        if (string.IsNullOrEmpty(p.Suffix))
                        {
                            p.Suffix = el.Element("Suffix") == null ? string.Empty : el.Element("Suffix").Value;
                        }
                        if (p.CanBuild == null)
                        {
                            p.CanBuild = bool.Parse(el.Element("CanBuild") == null ? null : el.Element("Suffix").Value);
                        }
                        foreach (var element in el.Element("Permission").Elements())
                        {
                            if (element.Name == "Allowed")
                            {
                                preAllowList.Add(element.Value);
                            }
                            if (element.Name == "Denied")
                            {
                                preDisallowedList.Add(element.Value);
                            }
                        }
                        //TODO - Inheritance and Dictionise this 
                    }
                }
            }
            else
            {
                p.Groups.Add((string) gperm.Attribute("Name"));
                p.Prefix = gperm.Element("Prefix") == null ? string.Empty : gperm.Element("Prefix").Value;
                p.Suffix = gperm.Element("Suffix") == null ? string.Empty : gperm.Element("Suffix").Value;
                bool bCanBuild;
                bool.TryParse((string) gperm.Element("CanBuild"), out bCanBuild);
                p.CanBuild = bCanBuild;
                if (gperm.Element("Permission") != null)
                {
                    foreach (var element in gperm.Element("Permission").Elements())
                    {
                        if (element.Name == "Allowed")
                        {
                            preAllowList.Add(element.Value);
                        }
                        if (element.Name == "Denied")
                        {
                            preDisallowedList.Add(element.Value);
                        }
                    }
                }
            }
            p.AllowedPermissions = RemoveDuplicates(preAllowList);
            p.DeniedPermissions = RemoveDuplicates(preDisallowedList);
            return p;
        }

        private static List<string> RemoveDuplicates(IEnumerable<string> inputList)
        {
            var uniqueStore = new Dictionary<string, int>();
            var finalList = new List<string>();
            foreach (string currValue in inputList.Where(currValue => !uniqueStore.ContainsKey(currValue)))
            {
                uniqueStore.Add(currValue, 0);
                finalList.Add(currValue);
            }
            return finalList;
        }

        public bool HasPermission(Player player, ICommand command)
        {
            return player.Permissions.AllowedPermissions.Contains(command.Permission.ToLower()) &&
                   !player.Permissions.DeniedPermissions.Contains(command.Permission.ToLower());
        }

        /// <summary>
        /// Check if a player has permission to use a command
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="permissionNode"></param>
        /// <returns>bool</returns>
        public bool HasPermission(string playerName, string permissionNode)
        {
            var client = _server.GetClients(playerName).FirstOrDefault() as Client;
            return client != null &&
                   (client.Owner.Permissions.AllowedPermissions.Contains("*") ||
                    client.Owner.Permissions.AllowedPermissions.Contains(permissionNode.ToLower()) &&
                    !client.Owner.Permissions.DeniedPermissions.Contains(permissionNode.ToLower()));
        }


        /// <summary>
        /// Check if a player is in a group
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="groupName"></param>
        /// <returns>bool</returns>
        public bool IsInGroup(string playerName, string groupName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault() as Client;
            return client != null && client.Owner.Permissions.Groups.Contains(groupName.ToLower());
        }


        public bool IsInGroup(Client client, string groupName)
        {
            return client.Owner.Permissions.Groups.Contains(groupName.ToLower());
        }

        /// <summary>
        /// Return the suffix of a specific player
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>value or null</returns>
        public string GetPlayerSuffix(string playerName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault() as Client;
            return client != null ? client.Owner.Permissions.Suffix : string.Empty;
        }

        public string GetPlayerSuffix(Player player)
        {
            return player.Permissions.Suffix;
        }

        /// <summary>
        /// Return the prefix of a specific player
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>value or null</returns>
        public string GetPlayerPrefix(string playerName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault() as Client;
            return client != null ? client.Owner.Permissions.Prefix : string.Empty;
        }

        public string GetPlayerPrefix(Player player)
        {
            return player.Permissions.Prefix;
        }

        /// <summary>
        /// Return the prefix of a specific group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>value or null</returns>
        public string GetGroupPrefix(string groupName)
        {
            throw new NotImplementedException();
            // return GroupExists(groupName) ? (from u in _groupOutValue where u.Key == "prefix" select u.Value).FirstOrDefault() : null;
        }

        /// <summary>
        /// Returns the suffix of a specific group
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>value or null</returns>
        public string GetGroupSuffix(string groupName)
        {
            throw new NotImplementedException();
            //   return GroupExists(groupName) ? (from u in _groupOutValue where u.Key == "suffix" select u.Value).FirstOrDefault() : null;
        }

        /// <summary>
        /// Get the list of groups a group inherits
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>string[] list of groups</returns>
        public string[] GetGroupInheritance(string groupName)
        {
            throw new NotImplementedException();
            //return GroupExists(groupName) ? (from g in _groupOutValue where g.Key == "inherit" select g.Value).FirstOrDefault().Split(',') : null;
        }

        /// <summary>
        /// Checks if a player has a users.ini value
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>bool</returns>
        [Obsolete("Deprecated", true)]
        public bool PlayerExists(string playerName)
        {
            throw new NotImplementedException();
            // return Users._iniFileContent.TryGetValue(playerName.ToLower(), out _usersOutValue);
        }

        /// <summary>
        /// Checks if a group has a groups.ini value
        /// </summary>
        /// <param name="groupName"></param>
        /// <returns>bool</returns>
        private bool GroupExists(string groupName)
        {
            var count =
                _permissionXml.Descendants("Groups").Descendants("Group").Count(n => n.Attribute("Name").Value.ToLower() == groupName.ToLower());
            return count > 0;
        }

        /// <summary>
        /// Checks if the player is allowed to build
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>bool</returns>
        public bool? CanPlayerBuild(string playerName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault() as Client;
            if (client != null)
            {
                return client.Owner.Permissions.CanBuild;
            }
            return false;
        }

        public bool? CanPlayerBuild(Client client)
        {
            return client.Owner.Permissions.CanBuild;
        }

        /// <summary>
        /// Gets the list of groups assinged to a player
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>string[] of groups</returns>
        public IEnumerable<string> GetPlayerGroups(string playerName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault() as Client;
            return client != null ? client.Owner.Permissions.Groups : null;
        }

        public IEnumerable<string> GetPlayerGroups(Client client)
        {
            return client.Owner.Permissions.Groups;
        }

        /// <summary>
        /// Loads permission file into an xdoc
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        private XDocument Load(string fileName)
        {
            if (File.Exists(fileName))
            {
                try
                {
                    return XDocument.Load(fileName);
                }
                catch
                {
                    return null;
                }

            }
            return null;
        }

        /// <summary>
        /// Saves the configuration file, will create file if it does not exist
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>bool whether file created successfully</returns>
        private bool Save(string filename)
        {

            try
            {
                _permissionXml.Save(filename);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}