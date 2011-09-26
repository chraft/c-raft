using System;
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using Chraft;
using Chraft.Commands;
using Chraft.Properties;

namespace ChraftServer
{
    public partial class MainService : ServiceBase
    {
        private Server Server { get; set; }
        private Task ServerRunTask { get; set; }
        private bool IsStopping { get; set; }

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
            ServerRunTask = Task.Factory.StartNew(RunServer);
        }

        private void RunServer()
        {
            IsStopping = false;
            while (!IsStopping)
            {
                (Server = new Server()).Run();
            }
        }


        protected override void OnStop()
        {
            IsStopping = true;
            if (Server != null)
                Server.Stop();
            ServerRunTask.Wait(10000);
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

            //run as service
            if (args.Any(a => a.Equals("-service", StringComparison.InvariantCultureIgnoreCase)))
            {
                Run(this);
            }
            else
            {
                if (!IsRunningInMono)
                {
                    Console.Title = "C#raft v" + Assembly.GetExecutingAssembly().GetName().Version;
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
        }

        public static bool IsRunningInMono
        {
            get { return (Type.GetType("Mono.Runtime") != null); }
        }
    }
}
