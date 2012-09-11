using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Chraft.PluginSystem.Server
{
    public interface IPluginLogger
    {
        /// <summary>
        /// Logs to the server log for the sending plugin. 
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <param name="format">The format.</param>
        /// <param name="arguments">The arguments.</param>
        void Log(LogLevel level, string pluginName, string format, params object[] arguments);

        /// <summary>
        /// Logs to the server log for the sending plugin. 
        /// </summary>
        /// <param name="level">The level.</param>
        /// <param name="pluginName">Name of the plugin.</param>
        /// <param name="message">The message.</param>
        void Log(LogLevel level, string pluginName, string message);

        /// <summary>
        /// Logs to the server log for the sending plugin. 
        /// </summary>
        /// <param name="ex">The ex.</param>
        void Log(Exception ex);
    }
}