/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.AccessControl;
using System.Security.Principal;
using Dev2.Common;
using Microsoft.Win32.SafeHandles;

// ReSharper disable CheckNamespace

namespace Dev2
{
    // ReSharper restore CheckNamespace

    /// <summary>
    ///     Used for internal security reasons
    /// </summary>
    public sealed class SafeTokenHandle : SafeHandleZeroOrMinusOneIsInvalid
    {
        private SafeTokenHandle()
            : base(true)
        {
        }

        [DllImport("kernel32.dll")]
        [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]
        [SuppressUnmanagedCodeSecurity]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseHandle(IntPtr handle);

        protected override bool ReleaseHandle()
        {
            return CloseHandle(handle);
        }
    }


    public class FileIO
    {
// ReSharper disable InconsistentNaming
        private const int LOGON32_PROVIDER_DEFAULT = 0;
// ReSharper restore InconsistentNaming
        //This parameter causes LogonUser to create a primary token. 
// ReSharper disable InconsistentNaming
        private const int LOGON32_LOGON_INTERACTIVE = 2;

        #region Permissions

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto)]
        public static extern bool CloseHandle(IntPtr handle);


        /// <summary>
        ///     Extracts the name of the user.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string ExtractUserName(string path)
        {
            string result = string.Empty;

            int idx = path.IndexOf("\\", StringComparison.Ordinal);

            if (idx > 0)
            {
                result = path.Substring((idx + 1));
            }

            return result;
        }

        /// <summary>
        ///     Extracts the domain.
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string ExtractDomain(string path)
        {
            string result = string.Empty;

            int idx = path.IndexOf("\\", StringComparison.Ordinal);

            if (idx > 0)
            {
                result = path.Substring(0, idx);
            }

            return result;
        }

        /// <summary>
        ///     Checks the permissions.
        /// </summary>
        /// <param name="userAndDomain">The user and domain.</param>
        /// <param name="pass">The pass.</param>
        /// <param name="path">The path.</param>
        /// <param name="rights">The rights.</param>
        /// <returns></returns>
        /// <exception cref="System.Exception">Failed to authenticate with user [  + userAndDomain +  ] for resource [  + path +  ] </exception>
        public static bool CheckPermissions(string userAndDomain, string pass, string path, FileSystemRights rights)
        {
            bool result;

            // handle UNC path
            try
            {
                string user = ExtractUserName(userAndDomain);
                string domain = ExtractDomain(userAndDomain);
                SafeTokenHandle safeTokenHandle;
                bool loginOk = LogonUser(user, domain, pass, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT,
                    out safeTokenHandle);


                if (loginOk)
                {
                    using (safeTokenHandle)
                    {
                        var newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            // Do the operation here
                            result = CheckPermissions(newID, path, rights);

                            impersonatedUser.Undo(); // remove impersonation now
                        }
                    }
                }
                else
                {
                    // login failed
                    throw new Exception("Failed to authenticate with user [ " + userAndDomain + " ] for resource [ " +
                                        path + " ] ");
                }
            }
            catch (Exception ex)
            {
                Dev2Logger.Log.Error("FileIO", ex);
                throw;
            }
            return result;
        }


        /// <summary>
        ///     Checks the permissions.
        /// </summary>
        /// <param name="user">The user.</param>
        /// <param name="path">The path.</param>
        /// <param name="expectedRights">The expected rights.</param>
        /// <returns></returns>
        public static bool CheckPermissions(WindowsIdentity user, string path, FileSystemRights expectedRights)
        {
            var fi = new FileInfo(path);
            var di = new DirectoryInfo(path);
            AuthorizationRuleCollection acl;

            if (fi.Exists)
            {
                acl = fi.GetAccessControl().GetAccessRules(true, true, typeof (SecurityIdentifier));
            }
            else if (di.Exists)
            {
                acl = di.GetAccessControl().GetAccessRules(true, true, typeof (SecurityIdentifier));
            }
            else
            {
                return false;
            }

            // gets rules that concern the user and his groups
            IEnumerable<AuthorizationRule> userRules = from AuthorizationRule rule in acl
                where user.Groups != null && (user.User != null && (user.User.Equals(rule.IdentityReference)
                                                                    || user.Groups.Contains(rule.IdentityReference)))
                select rule;

            FileSystemRights denyRights = 0;
            FileSystemRights allowRights = 0;

            // iterates on rules to compute denyRights and allowRights
            foreach (FileSystemAccessRule rule in userRules)
            {
                if (rule.AccessControlType.Equals(AccessControlType.Deny))
                {
                    denyRights = denyRights | rule.FileSystemRights;
                }
                else if (rule.AccessControlType.Equals(AccessControlType.Allow))
                {
                    allowRights = allowRights | rule.FileSystemRights;
                }
            }

            allowRights = allowRights & ~denyRights;

            // are rights sufficient?
            return (allowRights & expectedRights) == expectedRights;
        }

        #endregion Permissions

// ReSharper restore InconsistentNaming
    }
}