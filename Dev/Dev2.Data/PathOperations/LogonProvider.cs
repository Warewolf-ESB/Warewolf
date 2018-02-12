using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using System;
using System.Runtime.InteropServices;

namespace Dev2.Data.PathOperations
{
    class LogonProvider : IDev2LogonProvider
    {
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;

        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_PROVIDER_WINNT50 = 3;
        public bool LoggedOn { get; set; }

        [DllImport("advapi32.dll", EntryPoint = "LogonUserW", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword, 
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle token);

        public bool DoLogon(string lpszUsername, string lpszDomain, string lpszPassword, out SafeTokenHandle phToken)
        {
            var loggedOn = false;
            phToken = new SafeTokenHandle();
            try
            {
                loggedOn = LogonUser(lpszUsername, lpszDomain, lpszPassword, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeToken);
                phToken = safeToken;
                if (!loggedOn)
                {
                    loggedOn = LogonUser(lpszUsername, lpszDomain, lpszPassword, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_WINNT50, out SafeTokenHandle safeTokenHandle);
                    phToken = safeTokenHandle;
                }
                return loggedOn;
            }
            catch (Exception ex)
            {
                Dev2Logger.Error(ex.Message, ex, GlobalConstants.WarewolfError);
            }
            finally
            {
                Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
            }
            return loggedOn;
        }
    }
}
