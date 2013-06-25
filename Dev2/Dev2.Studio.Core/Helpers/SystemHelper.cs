using System;

namespace Dev2.Studio.Core.Helpers
{
    public enum PlatformTarget
    {
        Unknown,
        X86,
        X64
    };

    public static class SystemHelper
    {
        public static PlatformTarget CurrentPlatform()
        {
            switch(IntPtr.Size)
            {
                case 4:
                    return PlatformTarget.X86;
                case 8:
                    return PlatformTarget.X64;
                default:
                    return PlatformTarget.Unknown;
            }
        }
    }
}
