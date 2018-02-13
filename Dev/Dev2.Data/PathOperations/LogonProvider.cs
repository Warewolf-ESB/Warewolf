using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using System;
using System.Runtime.InteropServices;
using Warewolf.Resource.Errors;
using Dev2.Data.HelperClasses;

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

        public SafeTokenHandle DoLogon(IActivityIOPath path)
        {
            var lpszUsername = OperationsHelper.ExtractUserName(path);
            var lpszDomain = OperationsHelper.ExtractDomain(path);
            var lpszPassword = path.Password;
            var lpszPath = path.Path;
            try
            {
                var loggedOn = LogonUser(lpszUsername, lpszDomain, lpszPassword, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeToken);
                if (loggedOn)
                {
                    return safeToken;
                }
                loggedOn = LogonUser(lpszUsername, lpszDomain, lpszPassword, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_WINNT50, out SafeTokenHandle safeTokenHandle);
                if (loggedOn)
                {
                    return safeTokenHandle;
                }
            }

            catch (Exception ex)
            {
                Dev2Logger.Error(ex.Message, ex, GlobalConstants.WarewolfError);
            }
            finally
            {
                var ex = Marshal.GetExceptionForHR(Marshal.GetHRForLastWin32Error());
                Dev2Logger.Error(ex.Message, GlobalConstants.Warewolf);
            }
            throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, lpszUsername, lpszPath));
        }
    }
}
