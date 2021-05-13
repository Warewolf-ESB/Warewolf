/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common;
using Dev2.Data.Interfaces;
using Dev2.PathOperations;
using System;
using System.Runtime.InteropServices;
using Warewolf.Resource.Errors;
using Dev2.Data.Util;

namespace Dev2.Data.PathOperations
{
    public interface ILoginApi
    {
        bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle token);
    }
    class Win32LoginApi : ILoginApi
    {
        [DllImport("advapi32.dll", EntryPoint = "LogonUserW", SetLastError = true, CharSet = CharSet.Unicode)]
        static extern bool LogonUserW(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle token);
        public bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out SafeTokenHandle token)
        {
            return LogonUserW(lpszUsername, lpszDomain, lpszPassword, dwLogonType, dwLogonProvider, out token);
        }
    }
    class LogonProvider : IDev2LogonProvider
    {
        const int LOGON32_LOGON_INTERACTIVE = 2;
        const int LOGON32_LOGON_NETWORK = 3;

        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_PROVIDER_WINNT50 = 3;
        public bool LoggedOn { get; set; }

        private readonly ILoginApi _loginApi;
        public LogonProvider()
            : this(new Win32LoginApi())
        {
        }
        public LogonProvider(ILoginApi loginApi)
        {
            _loginApi = loginApi;
        }

        public SafeTokenHandle DoLogon(IActivityIOPath path)
        {
            var lpszUsername = OperationsHelper.ExtractUserName(path);
            var lpszDomain = OperationsHelper.ExtractDomain(path);
            var lpszPassword = path.Password;
            var lpszPath = path.Path;
            try
            {
                var loggedOn = _loginApi.LogonUser(lpszUsername, lpszDomain, lpszPassword, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out SafeTokenHandle safeToken);
                if (loggedOn)
                {
                    return safeToken;
                }
                loggedOn = _loginApi.LogonUser(lpszUsername, lpszDomain, lpszPassword, LOGON32_LOGON_NETWORK, LOGON32_PROVIDER_WINNT50, out SafeTokenHandle safeTokenHandle);
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
                Dev2Logger.Info(ex.Message, GlobalConstants.Warewolf);
            }
            throw new Exception(string.Format(ErrorResource.FailedToAuthenticateUser, lpszUsername, lpszPath));
        }
    }
}
