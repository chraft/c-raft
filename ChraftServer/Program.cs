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
using System.Collections;
using System.Configuration.Install;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Mono.Unix.Native;

namespace ChraftServer
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        private static void Main(string[] args)
        {
            Environment.CurrentDirectory = Path.GetDirectoryName(MainService.IsRunningInMono ? GetExecutablePath() : typeof (Program).Assembly.Location);
            //Install command-line argument
            if (args.Any(a => a.Equals("-install", StringComparison.InvariantCultureIgnoreCase)))
            {
                InstallService(args);
            }
            else if (args.Any(a => a.Equals("-uninstall", StringComparison.InvariantCultureIgnoreCase)))
            {
                UninstallService(args);
            }
            else if (args.Any(a => new[] { "/?", "-?", "/help" }.Any(helpArg => helpArg.Equals(a, StringComparison.InvariantCultureIgnoreCase))))
            {
                ShowUsage();
            }
            else
            {
                var svc = new MainService();
                svc.Run(args);
            }
        }

        private static void InstallService(string[] args)
        {
            using (var ti = new TransactedInstaller())
            {
                using (var pi = new ProjectInstaller())
                {
                    ti.Installers.Add(pi);
                    ti.Context = new InstallContext("", null);
                    string path = Assembly.GetExecutingAssembly().Location;
                    ti.Context.Parameters["assemblypath"] = path;
                    ti.Install(new Hashtable());
                }
            }
        }

        private static void UninstallService(string[] args)
        {
            using (var ti = new TransactedInstaller())
            {
                using (var pi = new ProjectInstaller())
                {
                    ti.Installers.Add(pi);
                    ti.Context = new InstallContext("", null);
                    string path = Assembly.GetExecutingAssembly().Location;
                    ti.Context.Parameters["assemblypath"] = path;
                    ti.Uninstall(null);
                }
            }
        }

        private static void ShowUsage()
        {
            Console.WriteLine("\r\nChraftServer Usage:\r\n");
            Console.WriteLine("\tChraftServer -help\r\n\t\tDisplay this help");
            Console.WriteLine("\tChraftServer -install\r\n\t\tInstall Chraft as a Windows Service");
            Console.WriteLine("\tChraftServer -uninstall\r\n\t\tUninstall Chraft Windows Service");
            Console.WriteLine("\tChraftServer\r\n\t\tRun Chraft from the console");
        }

        public static string GetExecutablePath()
        {
            var builder = new StringBuilder(8192);
            if (Syscall.readlink("/proc/self/exe", builder) >= 0)
                return builder.ToString();
            else
                return null;
        }
    }

}
