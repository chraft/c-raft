using System;
using System.Threading;
using System.Reflection;
using Chraft.Properties;

namespace Chraft
{
	internal static class Program
	{
		private static Server Server;

		static Program()
		{
			AppDomain.CurrentDomain.UnhandledException += UnhandledException_Handler;
			AppDomain.CurrentDomain.ProcessExit += (CurrentDomain_ProcessExit);
		}

        public static bool RunningInMono()
        {
            return (Type.GetType("Mono.Runtime") != null);
        }

		[MTAThread]
		public static void Main(string[] args)
		{
            if (!RunningInMono())
            {
                Console.Title = "C#raft v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }

            for (int i = 0; i < args.Length; i++)
            {
                switch (args[i])
                {
                    case "-port":
                        Settings.Default.Port = Convert.ToInt32(args[++i]);
                        break;
                    case "-ip":
                        Settings.Default.IPAddress = args[++i];
                        break;
                }
            }

            StartInput();
			StartServer();
		}

		private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
		{
			if (Server != null)
				Server.Stop();
		}

		private static void Exit()
		{
			Server.Stop();
			Server = null;
		}

		private static void StartServer()
		{
			(Server = new Server()).Run();
		}

		private static void StartInput()
		{
			Thread thread = new Thread(Run);
			thread.IsBackground = false;
			thread.Start();
		}

		private static void Run()
		{
			while (true)
			{
                string input = Console.ReadLine();
                string[] inputParts = input.Split();

				switch (inputParts[0])
				{
				    case "stop":
                        Server.Logger.Log(Logger.LogLevel.Info, "Stopping Server...");
					    Exit();
					    return;
                    default:
                        Server.Logger.Log(Logger.LogLevel.Info, "Unrecognised command:", inputParts[0]);
                        break;
				}
			}
		}

		private static void UnhandledException_Handler(object sender, UnhandledExceptionEventArgs e)
		{
			Server.Logger.Log((Exception)e.ExceptionObject);
		}

	}
}
