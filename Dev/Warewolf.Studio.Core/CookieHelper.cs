using System;
using System.Runtime.InteropServices;

namespace Warewolf.Studio.Core
{
    public class CookieHelper
    {
        public const int InternetOptionEndBrowserSession = 42;
        [DllImport("wininet.dll", SetLastError = true)]
        public static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
    }
}