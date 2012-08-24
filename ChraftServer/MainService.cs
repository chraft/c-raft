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
using System.Linq;
using System.Reflection;
using System.ServiceProcess;
using System.Threading.Tasks;
using Chraft;
using Chraft.Commands;
using Chraft.PluginSystem.Args;
using Chraft.PluginSystem.Commands;
using Chraft.PluginSystem.Event;
using Chraft.PluginSystem.Server;
using Chraft.Utils;
using Chraft.Utilities.Config;
using Chraft.PluginSystem;

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
                        ChraftConfig.Port = Convert.ToInt32(args[++i]);
                        break;
                    case "-ip":
                        ChraftConfig.IPAddress = args[++i];
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
            Server.Logger.Log(LogLevel.Debug, ((Exception)e.ExceptionObject).StackTrace);
        }

        public void Run(string[] args)
        {
            AppDomain.CurrentDomain.UnhandledException += UnhandledException_Handler;
            //Configure service current directory and managers
            Environment.CurrentDirectory = AppDomain.CurrentDomain.BaseDirectory;
            if (!Directory.Exists("Converter"))
                Directory.CreateDirectory("Converter");

            //run as service
            if (args.Any(a => a.Equals("-service", StringComparison.InvariantCultureIgnoreCase)))
            {
                Run(this);
            }
            else
            {
                Version version = Assembly.GetExecutingAssembly().GetName().Version;
                if (!IsRunningInMono)
                    Console.Title = "C#raft v" + version;

                Console.WriteLine("C#raft v{0}", version);

                PlayerNBTConverter a = new PlayerNBTConverter();
                foreach (var s in Directory.GetFiles("Converter", "*.dat", SearchOption.TopDirectoryOnly))
                {
                    Console.WriteLine("Converting {0} to C#raft format", s);
                    a.ConvertPlayerNBT(s);
                }

                OnStart(args);


                while (true)
                {
                    string input = Console.ReadLine();
                    if (string.IsNullOrEmpty(input) || string.IsNullOrEmpty(input.Trim()))
                        continue;
                    if (Server == null) return;
                    string[] inputParts = input.Split();
                    var cleanedtokens = inputParts.Skip(1).ToArray();
                    try
                    {
                        var cmd = Server.ServerCommandHandler.Find(inputParts[0]) as IServerCommand;
                        if (cmd == null) return;
                        //todo - make this better
                        if (cmd is CmdStop)
                        {
                            Server.Stop();
                        }

                        //Event Start
                        ServerCommandEventArgs e = new ServerCommandEventArgs(Server, cmd, inputParts);
                        Server.PluginManager.CallEvent(Event.ServerCommand, e);
                        if (e.EventCanceled) { return; }
                        //Event End

                        cmd.Use(Server, inputParts[0], cleanedtokens);
                    }
                    catch (CommandNotFoundException e) { Server.Logger.Log(LogLevel.Info, e.Message); }
                    catch (Exception e)
                    {
                        Server.Logger.Log(LogLevel.Error, "There was an error while executing the command.");
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
