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

            foreach (var node in Config.Descendants(parentNode).Where(node => node.Name == nodeName))
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
        /// loads a permission file
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
        /// Saves the permissions file, will create file if it does not exist
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
