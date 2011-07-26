using System;
using System.Threading;
using System.Reflection;
using Chraft.Properties;
using Chraft.Commands;

namespace Chraft
{
	internal static class Program
	{
		private static Server Server;
        private static bool Stoped = false;

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

		public static void Exit()
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
                if (Server == null) return;
                string[] inputParts = input.Split();
                ServerCommand Cmd;
                try
                {
                    Cmd = Server.ServerCommandHandler.Find(inputParts[0]) as ServerCommand;
                    Cmd.Use(Server, inputParts);
                }
                catch (CommandNotFoundException e) { Server.Logger.Log(Logger.LogLevel.Info, e.Message); }
                catch (Exception e)
                {
                    Server.Logger.Log(Logger.LogLevel.Error, "There was an error while executing the command.");
                    Server.Logger.Log(e);
                }
            }
		}

		private static void UnhandledException_Handler(object sender, UnhandledExceptionEventArgs e)
		{
			Server.Logger.Log((Exception)e.ExceptionObject);
		}

	}
}
