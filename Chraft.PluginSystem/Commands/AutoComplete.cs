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
using System.Linq;
using Chraft.PluginSystem.Net;

namespace Chraft.PluginSystem.Commands
{
    public static class AutoComplete
    {
        public static string GetPlayers(IClient client, string pattern)
        {
            var parts = pattern.Split(' ');
            if (parts.Length > 1)
                pattern = parts[parts.Length - 1];
            var s = (from a in client.GetServer().GetClients() where a.Username.StartsWith(pattern, StringComparison.OrdinalIgnoreCase) select a).ToList();

            if (!s.Any())
                return string.Empty;
            var sb = new System.Text.StringBuilder();

            if (s.Count() > 1)
            {
                foreach (var c in s)
                    sb.Append(c.Username).Append('\0');
            }
            else
            {
                sb.Append(s[0].Username);
            }
            return sb.ToString();
        }
    }
}
