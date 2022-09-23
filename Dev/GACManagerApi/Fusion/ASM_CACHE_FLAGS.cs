using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GACManagerApi.Fusion
{

    [Flags]
    public enum ASM_CACHE_FLAGS
    {
        ASM_CACHE_ZAP = 0x01,
        ASM_CACHE_GAC = 0x02,
        ASM_CACHE_DOWNLOAD = 0x04,
        ASM_CACHE_ROOT = 0x08,
        ASM_CACHE_ROOT_EX = 0x80
    }
}