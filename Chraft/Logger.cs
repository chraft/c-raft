using System;
using Chraft.Properties;
using System.IO;
using Chraft.Plugins.Events.Args;
using Chraft.Plugins.Events;

namespace Chraft
{
	public class Logger
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

        public void Log(LogLevel level, string format, params object[] arguments)
        {
			Log(level, string.Format(format, arguments));
        }

        public void Log(LogLevel level, string message)
		{
            //Event
            LoggerEventArgs e = new LoggerEventArgs(this, level, message);
            Server.PluginManager.CallEvent(Event.LOGGER_LOG, e);
            if (e.EventCanceled) return;
            level = e.LogLevel;
            message = e.LogMessage;
            //End Event

            LogToConsole(level, message);
			LogToFile(level, message);
		}

		private void LogToConsole(LogLevel level, string message)
		{
			if ((int)level >= Settings.Default.LogConsoleLevel)
				Console.WriteLine(Settings.Default.LogConsoleFormat, DateTime.Now, level.ToString().ToUpper(), message);
		}

		private void LogToFile(LogLevel level, string message)
		{
			if ((int)level >= Settings.Default.LogFileLevel && WriteLog != null)
				WriteLog.WriteLine(Settings.Default.LogFileFormat, DateTime.Now, level.ToString().ToUpper(), message);
		}

		public void Log(Exception ex)
		{
            //Event
            LoggerEventArgs e = new LoggerEventArgs(this, LogLevel.Debug, ex.ToString(), ex);
            Server.PluginManager.CallEvent(Event.LOGGER_LOG, e);
            if (e.EventCanceled) return;
            //End Event

			Log(LogLevel.Debug, ex.ToString());
		}


		public enum LogLevel : int
		{
			Trivial = -1,
			Debug = 0,
			Info = 1,
			Warning = 2,
			Caution = 3,
			Notice = 4,
			Error = 5,
			Fatal = 6
		}
	}
}