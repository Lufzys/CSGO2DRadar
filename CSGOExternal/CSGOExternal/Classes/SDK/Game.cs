using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CSGOExternal.Classes.SDK
{
    internal class Game
    {
        public static readonly string GameExecutableName = "csgo";
        public static int Client = -1, Engine = -1;

        public static bool Init()
        {
            var result = Memory.Initalize(GameExecutableName);
            if (result == Memory.Enums.InitalizeResult.OK)
            {
                while (Memory.GetModuleAddress("client.dll") == -1) { Thread.Sleep(500); }
                Client = Memory.GetModuleAddress("client.dll");
                while (Memory.GetModuleAddress("engine.dll") == -1) { Thread.Sleep(500); }
                Engine = Memory.GetModuleAddress("engine.dll");

                return true;
            }
            else
            {
                if (InternalProcess.IsDebug)
                    throw new Exception("Memory class can't initalized. Returned " + result.ToString());
                else
                    return false;
            }
        }
    }
}
