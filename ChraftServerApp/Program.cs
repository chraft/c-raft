
namespace ChraftServer
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
