using System;
using Chraft.Properties;
using System.IO;

namespace Chraft
{
	public class Logger
	{
		private StreamWriter WriteLog;

		internal Logger(string file)
		{
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