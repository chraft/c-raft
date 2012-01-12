using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.Utils.Config
{
    public class ChraftConfig
    {
        public static int Port { get; internal set; }
        public static string IPAddress { get; internal set; }
        public static string MOTD { get; set; }
        public static int SightRadius { get; internal set; }
        public static string WorldSeed { get; internal set; }
        public static int SpawnX { get; set; }
        public static int SpawnY { get; set; }
        public static int SpawnZ { get; set; }
        public static string LogFileFormat { get; internal set; }
        public static string LogConsoleFormat { get; internal set; }
        public static string LogFile { get; internal set; }
        public static int LogFileLevel { get; internal set; }
        public static int LogConsoleLevel { get; internal set; }
        public static string PluginFolder { get; internal set; }
        public static string WorldsFolder { get; internal set; }
        public static string DefaultWorldName { get; set; }
        public static string PlayersFolder { get; internal set; }
        public static int AnimalSpawnInterval { get; set; }
        public static string ItemsFile { get; internal set; }
        public static sbyte DefaultStackSize { get; set; }
        public static string RecipesFile { get; internal set; }
        public static bool LoadFromSave { get; internal set; }
        public static bool IrcEnabled { get; internal set; }
        public static string IrcServer { get; internal set; }
        public static string IrcChannel { get; internal set; }
        public static string IrcNickname { get; internal set; }
        public static int IrcPort { get; internal set; }
        public static string AllowedChatChars { get; internal set; }
        public static int MaxPlayers { get; internal set; }
        public static string ServerName { get; internal set; }
        public static int WeatherChangeFrequency { get; set; }
        public static string ContainersFolder { get; internal set; }
        public static string SmeltingRecipesFile { get; internal set; }
        public static bool UseOfficalAuthentication { get; internal set; }
        private static Configuration _config;

        public static void Load()
        {
            const string serverSetup = "ServerSetup";
            const string loggingSetup = "LoggingSetup";
            const string folderSetup = "FolderSetup";
            const string generalSetup = "GeneralSetup";
            const string ircSetup = "IrcSetup";

            _config = new Configuration("Chraft.config");

            //ServerSetup
            Port = _config.GetInt(serverSetup, "Port", 25565);
            IPAddress = _config.GetString(serverSetup, "IPAddress", "0.0.0.0");
            MOTD = _config.GetString(serverSetup, "MOTD", "Welcome to c#raft");
            UseOfficalAuthentication = _config.GetBoolean(serverSetup, "UseOfficalAuthentication", true);
            MaxPlayers = _config.GetInt(serverSetup, "MaxPlayers", 100);
            ServerName = _config.GetString(serverSetup, "ServerName", "C#raft");
            SightRadius = _config.GetInt(serverSetup, "SightRadius", 8);
            WorldSeed = _config.GetString(serverSetup, "WorldSeed", "1419875491758983");
            SpawnX = _config.GetInt(serverSetup, "SpawnX", 0);
            SpawnY = _config.GetInt(serverSetup, "SpawnY", 128);
            SpawnZ = _config.GetInt(serverSetup, "SpawnZ", 0);
            DefaultWorldName = _config.GetString(serverSetup, "DefaultWorldName", "Default");
            AnimalSpawnInterval = _config.GetInt(serverSetup, "AnimalSpawnInterval", 3000);
            LoadFromSave = _config.GetBoolean(serverSetup, "LoadFromSave", true);
            WeatherChangeFrequency = _config.GetInt(serverSetup, "WeatherChangeFrequency", 1);

            //logging setup
            LogFileFormat = _config.GetString(loggingSetup, "LogfileFormat", "{0:yyyy-MM-dd HH:mm:ss} [{1}] {2}");
            LogConsoleFormat = _config.GetString(loggingSetup, "LogConsoleFormat", "{0:HH:mm:ss} [{1}] {2}");
            LogFile = _config.GetString(loggingSetup, "LogFile", "server.log");
            LogFileLevel = _config.GetInt(loggingSetup, "LogFileLevel", -1);
            LogConsoleLevel = _config.GetInt(loggingSetup, "LogConsoleLevel", 3);

            //folder setup
            PluginFolder = _config.GetString(folderSetup, "PluginFolder", "Plugins");
            WorldsFolder = _config.GetString(folderSetup, "WorldsFolder", "Worlds");
            PlayersFolder = _config.GetString(folderSetup, "PlayersFolder", "Players");
            ContainersFolder = _config.GetString(folderSetup, "ContainersFolder", "Containers");

            //general setup
            SmeltingRecipesFile = _config.GetString(generalSetup, "SmeltingRecipesFile", "Resources/Smelting.dat");
            ItemsFile = _config.GetString(generalSetup, "ItemsFile", "Resources/Items.csv");
            DefaultStackSize = (sbyte)_config.GetInt(generalSetup, "DefaultStackSize", 64);
            RecipesFile = _config.GetString(generalSetup, "RecipesFile", "Resources/Recipes.dat");

            //irc setup
            AllowedChatChars = _config.GetString(ircSetup, "AllowedChatChars",
                                                 @"abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUV_ -=+~!@#$%^&amp;*()1234567890\[]{}|;':"",./&lt;&gt;?áéíóúäëïöüÁÉÍÓÚÄËÏÖÜÆæ");
            IrcEnabled = _config.GetBoolean(ircSetup, "IrcEnabled", true);
            IrcServer = _config.GetString(ircSetup, "IrcServer", "irc.esper.net");
            IrcChannel = _config.GetString(ircSetup, "IrcChannel", "#C#raft");
            IrcNickname = _config.GetString(ircSetup, "IrcNickname", "ChraftIrcBot");
            IrcPort = _config.GetInt(ircSetup, "IrcPort", 6667);
        }
    }
}
