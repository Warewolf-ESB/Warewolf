using System;
using System.Runtime.InteropServices;

namespace Warewolf.Studio.Core
{
    public class CookieHelper
    {
        public static void Clear() => InternetSetOption(IntPtr.Zero, InternetOptionEndBrowserSession, IntPtr.Zero, 0);
        const int InternetOptionEndBrowserSession = 42;

        [DllImport("wininet.dll", SetLastError = true)]
        static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
    }
}