using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Chraft.Utils
{
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

    public enum Colors
    {
        Black = 0x0,
        DarkBlue = 0x1,
        DarkGreen = 0x2,
        DarkAqua = 0x3,
        DarkRed = 0x4,
        DarkPurple = 0x5,
        Gold = 0x6,
        Gray = 0x7,
        DarkGray = 0x8,
        Blue = 0x9,
        Green = 0xA,
        Aqua = 0xB,
        Red = 0xC,
        LightPurple = 0xD,
        Yellow = 0xE,
        White = 0xF
    }
}
