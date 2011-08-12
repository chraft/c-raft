using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Chraft.Commands;


namespace Chraft.Utils
{
    public class PermissionHandler : IPermissions
    {
        private Configuration PermissionConfig;
        private static XDocument PermissionXml;
        private const string Permfile = "resources/Permissions.xml";
        private Dictionary<string, string> _groupOutValue = new Dictionary<string, string>();
        private Dictionary<string, string> _usersOutValue = new Dictionary<string, string>();
        private static Server _server;

        public PermissionHandler(Server server)
        {
            _server = server;
            PermissionConfig = new Configuration(server, Permfile);
            PermissionXml = PermissionConfig.Load(Permfile);
        }

        public ClientPermission LoadClientPermission(Client client)
        {
            //TODO - use ConfigurationClass for loading things
            var p = new ClientPermission();
            var preAllowList = new List<string>();
            var preDisallowedList = new List<string>();
            var perm =
                PermissionXml.Descendants("Users").Descendants("User").Where(
                    n => n.Attribute("Name").Value.ToLower() == client.Username.ToLower());
            if (perm.Count() > 0)
            {
                foreach (var xElement in perm)
                {
                    p.Groups = xElement.Attribute("Groups") == null
                                   ? null
                                   : xElement.Attribute("Groups").Value.Split(',');
                    p.Prefix = xElement.Element("Prefix") == null ? string.Empty : xElement.Element("Prefix").Value;
                    p.Suffix = xElement.Element("Suffix") == null ? string.Empty : xElement.Element("Suffix").Value;
                    p.CanBuild = bool.Parse(xElement.Element("CanBuild").Value);
                    foreach (var element in xElement.Element("Permission").Elements())
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
                    foreach (var el in p.Groups.Select(s => PermissionXml.Descendants("Groups").Descendants("Group").Where(n => n.Attribute("Name").Value.ToLower() == s.ToLower())).SelectMany(
                                        groupPerm => groupPerm))
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
                            if (element.Name == "Disallowed")
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
                //default group we grab the first with default attrbute defined
                var gperm =
                    PermissionXml.Descendants("Groups").Descendants("Group").Where(
                        n => n.Attribute("IsDefault").Value.ToLower() == "true").FirstOrDefault();
                p.Groups = gperm.Attribute("Groups") == null
                                   ? null
                                   : gperm.Attribute("Groups").Value.Split(',');
                p.Prefix = gperm.Element("Prefix") == null ? string.Empty : gperm.Element("Prefix").Value;
                p.Suffix = gperm.Element("Suffix") == null ? string.Empty : gperm.Element("Suffix").Value;
                p.CanBuild = bool.Parse(gperm.Element("CanBuild").Value);
                foreach (var element in gperm.Element("Permission").Elements())
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
            }
            p.AllowedPermissions = RemoveDuplicates(preAllowList);
            p.DeniedPermissions = RemoveDuplicates(preDisallowedList);
            return p;

        }

        static List<string> RemoveDuplicates(IEnumerable<string> inputList)
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

        public bool HasPermission(Client client, Command command)
        {
            return client.Permissions.AllowedPermissions.Contains(command.Permission.ToLower()) && !client.Permissions.DeniedPermissions.Contains(command.Permission.ToLower());
        }

        /// <summary>
        /// Check if a player has permission to use a command
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="permissionNode"></param>
        /// <returns>bool</returns>
        public bool HasPermission(string playerName, string permissionNode)
        {
            var client = _server.GetClients(playerName).FirstOrDefault();
            return client != null && (client.Permissions.AllowedPermissions.Contains("*") || client.Permissions.AllowedPermissions.Contains(permissionNode.ToLower()) && !client.Permissions.DeniedPermissions.Contains(permissionNode.ToLower()));
        }


   /// <summary>
        /// Check if a player is in a group
        /// </summary>
        /// <param name="playerName"></param>
        /// <param name="groupName"></param>
        /// <returns>bool</returns>
        public bool IsInGroup(string playerName, string groupName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault();
            return client != null && client.Permissions.Groups.Contains(groupName.ToLower());
        }

   
        public bool IsInGroup(Client client, string groupName)
        {
            return client.Permissions.Groups.Contains(groupName.ToLower());
        }
        /// <summary>
        /// Return the suffix of a specific player
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>value or null</returns>
        public string GetPlayerSuffix(string playerName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault();
            return client != null ? client.Permissions.Suffix : string.Empty;
        }

        public string GetPlayerSuffix(Client client)
        {
            return client.Permissions.Suffix;
        }
        /// <summary>
        /// Return the prefix of a specific player
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>value or null</returns>
        public string GetPlayerPrefix(string playerName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault();
            return client != null ? client.Permissions.Prefix : string.Empty;
        }

         public string GetPlayerPrefix(Client client)
         {
             return client.Permissions.Prefix;
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
                PermissionXml.Descendants("Groups").Descendants("Group").Where(
                    n => n.Attribute("Name").Value.ToLower() == groupName.ToLower()).Count();
            return count > 0;
        }

        /// <summary>
        /// Checks if the player is allowed to build
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>bool</returns>
        public bool? CanPlayerBuild(string playerName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault();
            if (client != null)
            {
                return client.Permissions.CanBuild;
            }
            return false;
        }

        public bool? CanPlayerBuild(Client client)
        {
            return client.Permissions.CanBuild;
        }

        /// <summary>
        /// Gets the list of groups assinged to a player
        /// </summary>
        /// <param name="playerName"></param>
        /// <returns>string[] of groups</returns>
        public IEnumerable<string> GetPlayerGroups(string playerName)
        {
            var client = _server.GetClients(playerName).FirstOrDefault();
            return client != null ? client.Permissions.Groups : null;
        }

        public IEnumerable<string> GetPlayerGroups(Client client)
        {
            return client.Permissions.Groups;
        }
    }
}