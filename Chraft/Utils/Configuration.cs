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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Chraft.Utils
{
    class Configuration
    {

        public Server Server { get; private set; }
        public Logger Logger { get { return Server.Logger; } }
        private XDocument Config { get; set; }

        public Configuration(Server server)
            : this(server, null)
        {
        }

        public Configuration(Server server, string filename)
        {
            Server = server;
            Config = Load(filename);
        }

        public ArrayList GetList(string nodeName, string parentNode = null)
        {
            var list = new ArrayList();

            foreach (var node in Config.Descendants(parentNode ?? "").Where(node => node.Name == nodeName))
            {
                list.Add(node.Value);
            }
            return list;
        }

        public bool GetBoolean()
        {
            return true;
        }

        public string GetString()
        {
            return "";
        }

        public int GetInt()
        {
            return 0;
        }
        public XElement GetNodeByAttribute(string attributename)
        {
            return null;
        }

        /// <summary>
        /// loads a configuration file
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>bool whether load was successful</returns>
        public XDocument Load(string filename)
        {
            if (File.Exists(filename))
            {
                try
                {
                    return XDocument.Load(filename);
                }
                catch
                {
                    return null;
                }

            }
            return null;
        }

        /// <summary>
        /// Saves the configuration file, will create file if it does not exist
        /// </summary>
        /// <param name="filename"></param>
        /// <returns>bool whether file created successfully</returns>
        private bool Save(string filename)
        {

            try
            {

                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
