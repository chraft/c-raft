using System;

namespace MineChraft
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (Client game = new Client())
            {
                game.Run();
            }
        }
    }
#endif
}

