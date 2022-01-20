using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGOExternal.Classes
{
    internal class Utils
    {
        public static bool IsKeyPushedDown(System.Windows.Forms.Keys vKey)
        {
            return 0 != (WinAPI.GetAsyncKeyState(vKey) & 0x8000);
        }
    }
}
