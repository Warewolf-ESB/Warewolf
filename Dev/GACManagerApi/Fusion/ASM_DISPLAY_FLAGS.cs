using System;
// ReSharper disable InconsistentNaming

namespace GACManagerApi.Fusion
{
    [Flags]
    public enum ASM_DISPLAY_FLAGS
    {
        ASM_DISPLAYF_VERSION = 0x01,
        ASM_DISPLAYF_CULTURE = 0x02,
        ASM_DISPLAYF_PUBLIC_KEY_TOKEN = 0x04,
        ASM_DISPLAYF_PROCESSORARCHITECTURE = 0x20
    }
}