﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GACManagerApi.Fusion
{
    [Flags]
    public enum ASM_DISPLAY_FLAGS
    {
        ASM_DISPLAYF_VERSION = 0x01,
        ASM_DISPLAYF_CULTURE = 0x02,
        ASM_DISPLAYF_PUBLIC_KEY_TOKEN = 0x04,
        ASM_DISPLAYF_PUBLIC_KEY = 0x08,
        ASM_DISPLAYF_CUSTOM = 0x10,
        ASM_DISPLAYF_PROCESSORARCHITECTURE = 0x20,
        ASM_DISPLAYF_LANGUAGEID = 0x40,
        ASM_DISPLAYF_RETARGET = 0x80,
        ASM_DISPLAYF_CONFIG_MASK = 0x100,
        ASM_DISPLAYF_MVID = 0x200,

        ASM_DISPLAYF_FULL =
            ASM_DISPLAYF_VERSION |
            ASM_DISPLAYF_CULTURE |
            ASM_DISPLAYF_PUBLIC_KEY_TOKEN |
            ASM_DISPLAYF_RETARGET |
            ASM_DISPLAYF_PROCESSORARCHITECTURE,

        
        ALL =
            ASM_DISPLAYF_VERSION |
            ASM_DISPLAYF_CULTURE |
            ASM_DISPLAYF_PUBLIC_KEY_TOKEN |
            ASM_DISPLAYF_PUBLIC_KEY |
            ASM_DISPLAYF_CUSTOM |
            ASM_DISPLAYF_PROCESSORARCHITECTURE | 
            ASM_DISPLAYF_LANGUAGEID |
            ASM_DISPLAYF_RETARGET | 
            ASM_DISPLAYF_CONFIG_MASK |
            ASM_DISPLAYF_CONFIG_MASK
    }
}