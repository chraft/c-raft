using System.Collections.Generic;

namespace Chraft
{
    public class ClientPermission
    {
        public string[] Groups { get; set; }
        public string Prefix { get; set; }
        public string Suffix { get; set; }
        public bool? CanBuild { get; set; }
        public List<string> AllowedPermissions { get; set; }
        public List<string> DeniedPermissions { get; set; }
    }
}