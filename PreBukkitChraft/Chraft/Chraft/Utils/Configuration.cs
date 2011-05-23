using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
namespace Chraft.Utils
{
    class Configuration
    {

        private Dictionary<string, Dictionary<string, string>> _iniFileContent;
        private readonly Regex _sectionRegex = new Regex(@"(?<=\[)(?<SectionName>[^\]]+)(?=\])");
        private readonly Regex _keyValueRegex = new Regex(@"(?<Key>[^=]+)=(?<Value>.+)");

        public Configuration() : this(null) { }

        public Configuration(string filename)
        {
            _iniFileContent = new Dictionary<string, Dictionary<string, string>>();
            if (filename != null) Load(filename);
        }

        public string GetValue(string sectionName, string key)
        {
            return _iniFileContent.ContainsKey(sectionName) && _iniFileContent[sectionName].ContainsKey(key)
                       ? _iniFileContent[sectionName][key]
                       : null;
        }


        public void SetValue(string sectionName, string key, string value)
        {
            if (!_iniFileContent.ContainsKey(sectionName)) _iniFileContent[sectionName] = new Dictionary<string, string>();
            _iniFileContent[sectionName][key] = value;
        }

        //get all section values
        public Dictionary<string, string> GetSection(string sectionName)
        {
            return _iniFileContent.ContainsKey(sectionName) ? new Dictionary<string, string>(_iniFileContent[sectionName]) : new Dictionary<string, string>();
        }

        //set a full section
        public void SetSection(string sectionName, IDictionary<string, string> sectionValues)
        {
            if (sectionValues == null) return;
            _iniFileContent[sectionName] = new Dictionary<string, string>(sectionValues);
        }

        //load file
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
                            currentSectionName = m.Groups["SectionName"].Value;
                        }
                        else
                        {
                            m = _keyValueRegex.Match(line);
                            if (m.Success)
                            {
                                string key = m.Groups["Key"].Value;
                                string value = m.Groups["Value"].Value;

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

        //save 
        public bool Save(string filename)
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

        public void CreateDefaultConfig()
        {
            var userValues = new Dictionary<string, string> { { "groups", "admin" }, { "commands", "list,home,time" } };
            var groupValues = new Dictionary<string, string> { { "prefix", "[Admin]" }, { "suffix", "" }, { "commands", "list,home,time" } };
            string exampleUsersFile = "data/exampleusers.ini";
            string exampleGroupsFiles = "data/examplegroups.ini";
            CreateConfigurationFile("Data", "users.ini");
            CreateConfigurationFile("Data", "groups.ini");

            if (!File.Exists(exampleUsersFile))
            {
                SetSection("ementalo", userValues);
                Save("data/exampleusers.ini");
                _iniFileContent = new Dictionary<string, Dictionary<string, string>>();
            }
            if (!File.Exists(exampleGroupsFiles))
            {
                SetSection("admin", groupValues);
                Save("data/examplegroups.ini");
            }

        }
    }
}
