using System.Collections.Generic;

namespace Chraft
{
    public class ClientPermission
    {
        public string Username { get; set; }
        public List<string> Groups { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public bool? CanBuild { get; set; }
        public List<string> AllowedPermissions { get; set; }
        public List<string> DeniedPermissions { get; set; }
    }

    public class GroupPermission
    {
        public string GroupName { get; set; }
        public List<string> InheritedGroups { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public bool? CanBuild { get; set; }
        public bool? IsDefault { get; set; }
        public List<string> AllowedPermissions { get; set; }
        public List<string> DeniedPermissions { get; set; }
    } 
}