using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CSGOExternal.Classes
{
    internal class WinAPI
    {
        [DllImport("user32.dll")]
        public static extern bool SetWindowDisplayAffinity(IntPtr hwnd, DisplayAffinity affinity);

        [DllImport("user32.dll")]
        public static extern int GetAsyncKeyState(Keys vKeys);

        public enum DisplayAffinity : uint
        {
            None = 0,
            Monitor = 1
        }
    }
}
