using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CSGOExternal.Classes
{
    internal class InternalProcess
    {
        public static bool IsDebug
        {
            get 
            {
#if DEBUG
                return true;
#else
                return false;
#endif
            }
        }
    }
}
