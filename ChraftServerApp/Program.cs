using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using Chraft.Commands;
using Chraft.Properties;

namespace Chraft.ServerApp
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		static void Main(string[] args)
		{
			var svc = new MainService();
			svc.Run(args);
		}
	}
}
