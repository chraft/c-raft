using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
namespace Chraft.Utils
{
    class PermissionConfiguration
    {

        public Dictionary<string, Dictionary<string, string>> _iniFileContent;
        private readonly Regex _sectionRegex = new Regex(@"(?<=\[)(?<SectionName>[^\]]+)(?=\])");
        private readonly Regex _keyValueRegex = new Regex(@"(?<Key>[^=]+)=(?<Value>.+)");

        public Server Server { get; private set; }
        public Logger Logger { get { return Server.Logger; } }


        public PermissionConfiguration(Server server)
            : this(server, null)
        {
        }

        public PermissionConfiguration(Server server, string filename)
        {
            Server = server;
            _iniFileContent = new Dictionary<string, Dictionary<string, string>>();
            if (filename != null) Load(filename);
        }

        /// <summary>
        /// Gets an individual key value
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public string GetValue(string sectionName, string key)
        {
            return _iniFileContent.ContainsKey(sectionName) && _iniFileContent[sectionName].ContainsKey(key)
                       ? _iniFileContent[sectionName][key]
                       : null;
        }

        /// <summary>
        /// Sets an individual key value
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public void SetValue(string sectionName, string key, string value)
        {
            if (!_iniFileContent.ContainsKey(sectionName)) _iniFileContent[sectionName] = new Dictionary<string, string>();
            _iniFileContent[sectionName][key] = value;
        }

        /// <summary>
        /// Gets all sections from a permission file specified
        /// </summary>
        /// <param name="sectionName"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetSection(string sectionName)
        {
            return _iniFileContent.ContainsKey(sectionName) ? new Dictionary<string, string>(_iniFileContent[sectionName]) : new Dictionary<string, string>();
        }

        /// <summary>
        /// Sets a secion inside a permissions file
        /// </summary>
        /// <param name="sectionName"></param>
        /// <param name="sectionValues"></param>
        public void SetSection(string sectionName, IDictionary<string, string> sectionValues)
        {
            if (sectionValues == null) return;
            _iniFileContent[sectionName] = new Dictionary<string, string>(sectionValues);
        }

        /// <summary>
        /// loads a permission file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>bool whether load was successful</returns>
        public bool Load(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    var content = File.ReadAllLines(filename);
                    _iniFileContent = new Dictionary<string, Dictionary<string, string>>();
                    string currentSectionName = string.Empty;
                    foreach (var line in content)
                    {
                        Match m = _sectionRegex.Match(line);
                        if (m.Success)
                        {
                            currentSectionName = m.Groups["SectionName"].Value.ToLower();
                        }
                        else
                        {
                            m = _keyValueRegex.Match(line);
                            if (m.Success)
                            {
                                string key = m.Groups["Key"].Value.ToLower();
                                string value = m.Groups["Value"].Value.ToLower();

                                Dictionary<string, string> kvpList;
                                if (_iniFileContent.ContainsKey(currentSectionName))
                                {
                                    kvpList = _iniFileContent[currentSectionName];
                                }
                                else
                                {
                                    kvpList = new Dictionary<string, string>();
                                }
                                kvpList[key] = value;
                                _iniFileContent[currentSectionName] = kvpList;
                            }
                        }
                    }
                    return true;
                }
                catch
                {
                    return false;
                }

            }
            return false;
        }

        /// <summary>
        /// Saves the permissions file, will create file if it does not exist
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>bool whether file created successfully</returns>
        private bool Save(string filename)
        {
            var sb = new StringBuilder();
            if (_iniFileContent != null)
            {
                foreach (var sectionName in _iniFileContent)
                {
                    sb.AppendFormat("[{0}]\r\n", sectionName.Key);
                    foreach (var keyValue in sectionName.Value)
                    {
                        sb.AppendFormat("{0}={1}\r\n", keyValue.Key, keyValue.Value);
                    }
                }
            }
            try
            {
                File.WriteAllText(filename, sb.ToString());
                return true;
            }
            catch
            {
                return false;
            }
        }


        public void CreateConfigurationFile(string directory, string fileName)
        {
            string fullPath = directory + "/" + fileName;
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            if (!File.Exists(fullPath))
            {
                Logger.Log(Logger.LogLevel.Info, "Creating file " + fullPath);
                File.Create(fullPath);
            }
        }

        /// <summary>
        /// Creates default permissions files 
        /// </summary>
        public void CreateDefaultPermissionsConfig()
        {
            var userValues = new Dictionary<string, string> { { "groups", "admin" }, { "prefix", "" }, { "suffix", "" }, { "commands", "list,home,time" } };
            var adminGroupValues = new Dictionary<string, string> { { "prefix", "[admin]" }, { "suffix", "" }, { "commands", "*" }, { "inherit", "default" } };
            var defaultGroupValues = new Dictionary<string, string> { { "prefix", "[default]" }, { "suffix", "" }, { "commands", "list,spawn" }, { "inherit", "" } };
            const string usersFile = "resources/users.ini";
            const string groupsFile = "resources/groups.ini";
            string[] users = { "ementalo", "Zenexer", "PhonicUK" };

            if (!File.Exists(usersFile))
            {
                //Prepare usersfiles 
                foreach (var user in users)
                {
                    SetSection(user, userValues);
                }
                //try to save usersfile, this will create the file if it does not exist
                if (!Save(usersFile))
                {
                    Logger.Log(new IOException("Could not create file " + usersFile));
                }
                _iniFileContent = new Dictionary<string, Dictionary<string, string>>();
            }

            //prepare groupsfile
            if (!File.Exists(groupsFile))
            {
                SetSection("default", defaultGroupValues);
                SetSection("admin", adminGroupValues);

                //try to save groupsfile, this will create the file if it does not exist
                if (!Save(groupsFile))
                {
                    Logger.Log(new IOException("Could not create file " + groupsFile));
                }
            }
        }
    }
}
