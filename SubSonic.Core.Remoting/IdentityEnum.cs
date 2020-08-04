using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SubSonic.Core.Remoting
{
    [Flags]
    public enum IdentityEnum
    {
        UNKNOWN = 0,
        DISCONNECTED_FULL = 1,
        DISCONNECTED_REM = 2,
        IN_IDTABLE = 4,
        CONTEXT_BOUND = 0x10,
        WELLKNOWN = 0x100,
        SERVER_SINGLECALL = 0x200,
        SERVER_SINGLETON = 0x400,
        
    }
}
