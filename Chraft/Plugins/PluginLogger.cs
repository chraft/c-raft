using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft.PluginSystem.Server;

namespace Chraft.Plugins
{
    public class PluginLogger : IPluginLogger
    {
        private readonly ILogger _logger;

        internal PluginLogger(ILogger logger)
        {
            _logger = logger;
        }

        #region Implementation of ILogger

        public void Log(LogLevel level, string pluginName, string format, params object[] arguments)
        {
            Log(level, pluginName, string.Format(format, arguments));
        }

        public void Log(LogLevel level, string pluginName, string message)
        {
            _logger.Log(level, String.Format("[{0}] {1}", pluginName, message));
        }

        public void Log(Exception ex)
        {
            _logger.Log(ex);
        }

        #endregion
    }
}