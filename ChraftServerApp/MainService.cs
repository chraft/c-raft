using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Chraft.Commands;
using Chraft.Properties;

namespace Chraft.ServerApp
{
	public partial class MainService : ServiceBase
	{
		private Server Server { get; set; }

		public MainService()
		{
			InitializeComponent();
		}

		protected override void OnStart(string[] args)
		{
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
			Task.Factory.StartNew(RunServer);
		}

		private void RunServer()
		{
			(Server = new Server()).Run();
		}


		protected override void OnStop()
		{
			if (Server != null)
				Server.Stop();
		}

		private void UnhandledException_Handler(object sender, UnhandledExceptionEventArgs e)
		{
			Server.Logger.Log((Exception)e.ExceptionObject);
		}

		public void Run(string[] args)
		{
			AppDomain.CurrentDomain.UnhandledException += UnhandledException_Handler;

			//Configure service current directory and managers
			Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;

			if (args.Any(a => a.Equals("-console", StringComparison.InvariantCultureIgnoreCase)))
			{
				//If -console specified as argument, run as console application

				if (!IsRunningInMono)
				{
					Console.Title = "C#raft v" + Assembly.GetExecutingAssembly().GetName().Version.ToString();
				}
				OnStart(args);
				while (true)
				{
					string input = Console.ReadLine();
					if (Server == null) return;
					string[] inputParts = input.Split();
					try
					{
						var cmd = Server.ServerCommandHandler.Find(inputParts[0]) as ServerCommand;
						if (cmd is CmdStop)
						{
							//TODO: Clean this up
							break;
						}
						cmd.Use(Server, inputParts);
					}
					catch (CommandNotFoundException e) { Server.Logger.Log(Logger.LogLevel.Info, e.Message); }
					catch (Exception e)
					{
						Server.Logger.Log(Logger.LogLevel.Error, "There was an error while executing the command.");
						Server.Logger.Log(e);
					}
				}
				OnStop();
			}
			else
			{
				//Otherwise, run as normal windows service
				Run(this);
			}
		}

		public static bool IsRunningInMono
		{
			get { return (Type.GetType("Mono.Runtime") != null); }
		}
	}
}
