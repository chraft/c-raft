using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Utils
{
    interface IPermissions
    {
        bool CanUseCommand(string playerName, string command);
        bool IsInGroup(string playerName, string groupName);
        string GetPlayerPrefix(string playerName);
        string GetPlayerSuffix(string playerName);
        string GetGroupPrefix(string playerName);
        string GetGroupSuffix(string playerName);
        string[] GetGroupInheritance(string groupName);
    }
}
