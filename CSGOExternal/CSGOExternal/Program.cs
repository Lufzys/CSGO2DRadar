using CSGOExternal.Classes;
using CSGOExternal.Classes.SDK;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGOExternal
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.RealTime;
            Console.WriteLine(Game.Init().ToString());
            Cheat.Init();
        }
    }
}
