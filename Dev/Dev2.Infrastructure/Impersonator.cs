/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading;
using Dev2.Common.Interfaces;
using Warewolf.Security.Encryption;

namespace Dev2
{
    public class Impersonator : IDisposable, IImpersonator
    {
        
        const int LOGON32_PROVIDER_DEFAULT = 0;
        const int LOGON32_LOGON_INTERACTIVE = 2;


        #region DllImports

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool LogonUser(string lpszUsername, string lpszDomain, string lpszPassword, int dwLogonType, int dwLogonProvider, out IntPtr phToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern int DuplicateToken(IntPtr hToken, int impersonationLevel, out IntPtr hNewToken);

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        static extern bool RevertToSelf();

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool CloseHandle(IntPtr handle);

        #endregion

        WindowsImpersonationContext _impersonationContext;

        #region Impersonate

        public bool Impersonate(string userName, string domain, string password)
        {
            var token = IntPtr.Zero;
            var tokenDuplicate = IntPtr.Zero;

            if(RevertToSelf() && LogonUser(userName, domain, password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out token) && DuplicateToken(token, 2, out tokenDuplicate) != 0)
            {
                var tempWindowsIdentity = new WindowsIdentity(tokenDuplicate);
                _impersonationContext = tempWindowsIdentity.Impersonate();
                if(_impersonationContext != null)
                {
                    ClaimsPrincipal principal = new WindowsPrincipal(tempWindowsIdentity);
                    Thread.CurrentPrincipal = principal;
                    CloseHandle(token);
                    CloseHandle(tokenDuplicate);
                    return true;
                }
            }
            if(token != IntPtr.Zero)
            {
                CloseHandle(token);
            }
            if(tokenDuplicate != IntPtr.Zero)
            {
                CloseHandle(tokenDuplicate);
            }
            return false;
        }

        #endregion

        #region Undo

        public void Undo()
        {
            if(_impersonationContext != null)
            {
                _impersonationContext.Undo();
                _impersonationContext.Dispose();
            }
        }

        public bool ImpersonateForceDecrypt(string userName, string domain, string decryptIfEncrypted)
        {
            return Impersonate(userName, domain, DpapiWrapper.DecryptIfEncrypted(decryptIfEncrypted));
        }

        #endregion

        #region IDisposable

        ~Impersonator()
        {
            Dispose(false);
        }

        bool _disposed;
        
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        
        protected virtual void Dispose(bool disposing)
        {
            if(!_disposed)
            {
                if(disposing)
                {
                    Undo();
                }
                
                _disposed = true;
            }
        }

        #endregion
    }
}
