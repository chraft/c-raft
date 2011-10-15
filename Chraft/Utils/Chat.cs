using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Chraft.Utils
{
    //Cheat enum since enums can't be strings.
    public static class ChatColor
    {
        public static string Black = "§0",
        DarkBlue = " §1",
        DarkGreen = " §2",
        DarkTeal = " §3",
        DarkRed = " §4",
        Purple = " §5",
        Gold = " §6",
        Gray = " §7",
        DarkGray = " §8",
        Blue = " §9",
        BrightGreen = " §a",
        Teal = " §b",
        Red = " §c",
        Pink = " §d",
        Yellow = " §e",
        White = " §f";
    }
    
    internal static class Chat
    {
        internal const string DISALLOWED = @"[^0-9a-zA-Z""!-/:-@\[-_{-~⌂ÇüéâäàåçêëèïîìÄÅÉæÆôöòûùÿÖÜø£Ø×ƒáíóúñÑªº¿®¬½¼¡«»§ ]";
		internal const string CENSOR = "|";
		internal const string FORMAT = "{0}: {1}";

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
