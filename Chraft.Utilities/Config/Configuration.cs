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

using System.IO;
using Nini.Config;

namespace Chraft.Utilities.Config
{
    public class Configuration : ConfigSourceBase
    {
        private ConfigCollection Sections { get; set; }
        public XmlConfigSource Config { get; set; }
        
        public Configuration(string filename)
        {
            Config = Load(filename);
            Config.AutoSave = true;
            Sections = Config.Configs;
        }

        public bool Contains(string section, string key)
        {
            if (string.IsNullOrEmpty(section))
                return Sections.Contains(key);
            return Sections[section].Contains(key);
        }

        public bool Contains(string key)
        {
            return Contains(null, key);
        }


        public string Get(string section, string key, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(defaultValue))
                return Sections[section].Get(key);
            return Sections[section].Get(key, defaultValue);
        }


        public string GetString(string section, string key, string defaultValue = null)
        {
            if (string.IsNullOrEmpty(defaultValue))
                return Sections[section].GetString(key);
            return Sections[section].GetString(key, defaultValue);
        }

        public int GetInt(string section, string key)
        {
            return Sections[section].GetInt(key);
        }

        public int GetInt(string section, string key, int defaultValue)
        {
            return Sections[section].GetInt(key, defaultValue);
        }

        public long GetLong(string section, string key)
        {
            return Sections[section].GetLong(key);
        }

        public long GetLong(string section, string key, long defaultValue)
        {
            return Sections[section].GetLong(key, defaultValue);
        }

        public bool GetBoolean(string section, string key)
        {
            return Sections[section].GetBoolean(key);
        }

        public bool GetBoolean(string section, string key, bool defaultValue)
        {
            return Sections[section].GetBoolean(key, defaultValue);
        }

        public float GetFloat(string section, string key)
        {
            return Sections[section].GetFloat(key);
        }

        public float GetFloat(string section, string key, float defaultValue)
        {
            return Sections[section].GetFloat(key, defaultValue);
        }

        public double GetDouble(string section, string key)
        {
            return Sections[section].GetDouble(key);
        }

        public double GetDouble(string section, string key, double defaultValue)
        {
            return Sections[section].GetDouble(key, defaultValue);
        }

        public string[] GetKeys(string section)
        {
            return Sections[section].GetKeys();
        }

        public string[] GetValues(string section)
        {
            return Sections[section].GetValues();
        }

        public void Set(string section, string key, object value)
        {
            Sections[section].Set(key, value);
        }

        public void Remove(string section, string key)
        {
            Sections[section].Remove(key);
        }

        /// <summary>
        /// loads a configuration file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>XmlConfig</returns>
        public XmlConfigSource Load(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    return new XmlConfigSource(filename);
                }
                catch
                {
                    return null;
                }

            }
            return new XmlConfigSource();
        }

        /// <summary>
        /// Saves the configuration file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>bool whether file created successfully</returns>
        public bool Save(string filename)
        {

            try
            {
                Config.Save(filename);
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
