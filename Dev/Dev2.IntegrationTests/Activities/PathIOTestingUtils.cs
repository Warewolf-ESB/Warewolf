/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.IO;
using System.Runtime.ConstrainedExecution;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Principal;
using Dev2.PathOperations;
using Microsoft.Win32.SafeHandles;

namespace Dev2.Integration.Tests.Activities
{

    /// <summary>
    /// Used for internal security reasons
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

    public static class PathIOTestingUtils
    {

        [DllImport("advapi32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        public static extern bool LogonUser(String lpszUsername, String lpszDomain, String lpszPassword,
            int dwLogonType, int dwLogonProvider, out SafeTokenHandle phToken);

        // ReSharper disable InconsistentNaming
        public static void CreateAuthedUNCPath(string path, bool isDir = false, bool inDomain = true)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // handle UNC path

            byte[] data = new byte[3];

            data[0] = (byte)'a';
            data[1] = (byte)'b';
            data[2] = (byte)'c';

            CreateUNCFile(path, isDir, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, data, inDomain);
        }

        private static void CreateUNCFile(string path, bool isDir, int LOGON32_LOGON_INTERACTIVE, int LOGON32_PROVIDER_DEFAULT,
                                          byte[] data, bool inDomain = true)
        {
            SafeTokenHandle safeTokenHandle;
            bool loginOk = LogonUser(ParserStrings.PathOperations_Correct_Username, inDomain ? "DEV2" : string.Empty,
                                     ParserStrings.PathOperations_Correct_Password, LOGON32_LOGON_INTERACTIVE,
                                     LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

            if (loginOk)
            {
                using (safeTokenHandle)
                {
                    WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                    using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                    {
                        if (!isDir)
                        {
                            // Do the operation here
                            File.WriteAllBytes(path, data);
                        }
                        else
                        {
                            Directory.CreateDirectory(path);
                        }

                        // remove impersonation now
                        impersonatedUser.Undo();
                    }
                }
            }
            else
            {
                // login failed
                throw new Exception("Failed to authenticate for resource [ " + path + " ] ");
            }
        }

        public static void DeleteAuthedUNCPath(string path, bool inDomain = false)
        {
            const int LOGON32_PROVIDER_DEFAULT = 0;
            //This parameter causes LogonUser to create a primary token. 
            const int LOGON32_LOGON_INTERACTIVE = 2;

            // handle UNC path
            SafeTokenHandle safeTokenHandle;

            try
            {
                bool loginOk = LogonUser(ParserStrings.PathOperations_Correct_Username, inDomain ? "DEV2" : string.Empty, ParserStrings.PathOperations_Correct_Password, LOGON32_LOGON_INTERACTIVE, LOGON32_PROVIDER_DEFAULT, out safeTokenHandle);

                if (loginOk)
                {
                    using (safeTokenHandle)
                    {

                        WindowsIdentity newID = new WindowsIdentity(safeTokenHandle.DangerousGetHandle());
                        using (WindowsImpersonationContext impersonatedUser = newID.Impersonate())
                        {
                            // Do the operation here
                            if (Dev2ActivityIOPathUtils.IsDirectory(path))
                            {
                                Directory.Delete(path, true);
                            }
                            else
                            {
                                File.Delete(path);
                            }

                            // remove impersonation now
                            impersonatedUser.Undo();
                        }
                    }
                }
                else
                {
                    // login failed
                    throw new Exception("Failed to authenticate for resource [ " + path + " ] ");
                }
            }
            catch (Exception e)
            {
                throw e;
            }
        }
    }
}
