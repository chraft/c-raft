using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Chraft;

namespace ChraftStarter
{
    class Program
    {
        static void Main(string[] args)
        {
            //Quick and esey workaround.
            //Could make Chraft.Program a public static class and avoid this chain of one line methods.
            Starter.Start(args);
        }
    }
}
