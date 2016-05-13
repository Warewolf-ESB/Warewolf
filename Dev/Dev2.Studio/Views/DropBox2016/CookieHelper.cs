using System;
using System.Runtime.InteropServices;

namespace Dev2.Views.DropBox2016
{
    internal class CookieHelper
    {
        public static void Clear()
        {
            InternetSetOption(IntPtr.Zero, InternetOptionEndBrowserSession, IntPtr.Zero, 0);
        }
        private const int InternetOptionEndBrowserSession = 42;

        [DllImport("wininet.dll", SetLastError = true)]
        private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
    }
}