using System;
using System.Activities.Statements;
using System.Runtime.InteropServices;

class CookieHelper
{

    public static void Clear()
    {
        InternetSetOption(IntPtr.Zero, INTERNET_OPTION_END_BROWSER_SESSION, IntPtr.Zero, 0);
    }
    private const int INTERNET_OPTION_END_BROWSER_SESSION = 42;

    [DllImport("wininet.dll", SetLastError = true)]
    private static extern bool InternetSetOption(IntPtr hInternet, int dwOption, IntPtr lpBuffer, int lpdwBufferLength);
}