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

using System.Text.RegularExpressions;

namespace Chraft.Utilities.Misc
{
    //Cheat enum since enums can't be strings.
    public static class ChatColor
    {
        public static string Black = "§0",
                             DarkBlue = "§1",
                             DarkGreen = "§2",
                             DarkTeal = "§3",
                             DarkRed = "§4",
                             Purple = "§5",
                             Gold = "§6",
                             Gray = "§7",
                             DarkGray = "§8",
                             Blue = "§9",
                             BrightGreen = "§a",
                             Teal = "§b",
                             Red = "§c",
                             Pink = "§d",
                             Yellow = "§e",
                             White = "§f",
                             Magic = "§k",
                             //adds the rotating character
                             Bold = "§l",
                             StrikeThrough = "§m",
                             UnderLine = "§n",
                             Italic = "§o",
                             Reset = "§r"; // resets all formats

    }
    
    public static class Chat
    {
        public const string DISALLOWED = @"[^0-9a-zA-Z""!-/:-@\[-_{-~⌂ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜø£Ø×ƒáíóúñÑªº¿®¬½¼¡«»§ ]";
        public const string CENSOR = "|";
		public const string FORMAT = "{0}: {1}";

		public static string CleanMessage(string message)
		{
			return Regex.Replace(message.Replace('&', '§').Replace("§§", "&"), DISALLOWED, CENSOR);
		}

		public static string Format(string username, string message)
		{
			return string.Format(FORMAT, username, message);
		}

		public static string[] Tokenize(string command)
		{
			return command.Split(new[] { ' ' });
		}
	}
}
