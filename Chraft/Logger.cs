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
using System;
using System.IO;
using Chraft.PluginSystem;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Server;
using Chraft.Plugins.Events;
using Chraft.Utilities.Config;

namespace Chraft
{
	public class Logger : ILogger
	{
		private StreamWriter WriteLog;
        private Server Server;

		internal Logger(Server server, string file)
		{
            Server = server;
			try
			{
				WriteLog = new StreamWriter(file, true);
				WriteLog.AutoFlush = true;
			}
			catch
			{
				WriteLog = null;
			}
		}

		~Logger()
		{
			try
			{
				WriteLog.Close();
			}
			catch
			{
			}
		}

        public void LogOnOneLine(LogLevel level, string format, bool header, params object[] arguments)
        {
            LogOnOneLine(level, string.Format(format, arguments), header);
        }

        public void LogOnOneLine(LogLevel level, string message, bool header)
        {
            //Event
            LoggerEventArgs e = new LoggerEventArgs(this, level, message);
            Server.PluginManager.CallEvent(Event.LoggerLog, e);
            // do not allow cancellation or altering of log messages
            //End Event

            LogToConsole(level, message, false, header);
            LogToFile(level, message, false, header);
        }

	    public void Log(LogLevel level, string format, params object[] arguments)
        {
			Log(level, string.Format(format, arguments));
        }

        public void Log(LogLevel level, string message)
		{
            //Event
            LoggerEventArgs e = new LoggerEventArgs(this, level, message);
            Server.PluginManager.CallEvent(Event.LoggerLog, e);
            // do not allow cancellation or altering of log messages
            //End Event

            LogToConsole(level, message, true);
			LogToFile(level, message, true);
		}

		private void LogToConsole(LogLevel level, string message, bool newLine, bool header = true)
		{
            if ((int)level >= ChraftConfig.LogConsoleLevel)
            {
                if (newLine)
                    Console.WriteLine(ChraftConfig.LogConsoleFormat, DateTime.Now, level.ToString().ToUpper(), message);
                else
                {
                    if (header)
                        Console.Write(ChraftConfig.LogConsoleFormat, DateTime.Now, level.ToString().ToUpper(), message);
                    else
                        Console.Write("{0}", message);
                }
            }
		}

		private void LogToFile(LogLevel level, string message, bool newLine, bool header = true)
		{
            if ((int)level >= ChraftConfig.LogFileLevel && WriteLog != null)
            {
                if (newLine)
                    WriteLog.WriteLine(ChraftConfig.LogFileFormat, DateTime.Now, level.ToString().ToUpper(), message);
                else
                {
                    if (header)
                        WriteLog.Write(ChraftConfig.LogFileFormat, DateTime.Now, level.ToString().ToUpper(), message);
                    else
                        WriteLog.Write("{0}", message);
                }
            }
		}

		public void Log(Exception ex)
		{
            //Event
            LoggerEventArgs e = new LoggerEventArgs(this, LogLevel.Debug, ex.ToString(), ex);
            Server.PluginManager.CallEvent(Event.LoggerLog, e);
            // do not allow cancellation or altering of log messages
            //End Event

			Log(LogLevel.Debug, ex.ToString());
		}
	}
}