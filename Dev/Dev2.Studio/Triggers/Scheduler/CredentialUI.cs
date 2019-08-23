/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;


namespace Dev2.Triggers.Scheduler
{
    [ExcludeFromCodeCoverage] //Uses COM interop to show the Windows Credential Screen
    public static class CredentialUI
    {
        [DllImport("ole32.dll")]
        static extern void CoTaskMemFree(IntPtr ptr);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        struct CREDUI_INFO
        {
            public int cbSize;
            public string pszMessageText;
            public string pszCaptionText;
        }

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        static extern bool CredUnPackAuthenticationBuffer(int dwFlags,
            IntPtr pAuthBuffer,
            uint cbAuthBuffer,
            StringBuilder pszUserName,
            ref int pcchMaxUserName,
            StringBuilder pszDomainName,
            ref int pcchMaxDomainame,
            StringBuilder pszPassword,
            ref int pcchMaxPassword);

        [DllImport("credui.dll", CharSet = CharSet.Auto)]
        static extern int CredUIPromptForWindowsCredentials(ref CREDUI_INFO notUsedHere,
            int authError,
            ref uint authPackage,
            IntPtr inAuthBuffer,
            uint inAuthBufferSize,
            out IntPtr refOutAuthBuffer,
            out uint refOutAuthBufferSize,
            ref bool fSave,
            int flags);

        [ExcludeFromCodeCoverage]
        public static void GetCredentialsVistaAndUp(string taskName, out NetworkCredential networkCredential)
        {
            var credui = new CREDUI_INFO
            {
                pszCaptionText = @"Please enter the credentials to use to schedule",
                pszMessageText = taskName
            };
            credui.cbSize = Marshal.SizeOf(credui);
            uint authPackage = 0;
            var save = false;
            var result = CredUIPromptForWindowsCredentials(ref credui,
                0,
                ref authPackage,
                IntPtr.Zero,
                0,
                out IntPtr outCredBuffer,
                out uint outCredSize,
                ref save,
                1 /* Generic */);

            var usernameBuf = new StringBuilder(100);
            var passwordBuf = new StringBuilder(100);
            var domainBuf = new StringBuilder(100);

            var maxUserName = 100;
            var maxDomain = 100;
            var maxPassword = 100;
            if (result == 0 && CredUnPackAuthenticationBuffer(0, outCredBuffer, outCredSize, usernameBuf, ref maxUserName,
    domainBuf, ref maxDomain, passwordBuf, ref maxPassword))
            {
                CoTaskMemFree(outCredBuffer);
                networkCredential = new NetworkCredential
                {
                    UserName = usernameBuf.ToString(),
                    Password = passwordBuf.ToString(),
                    Domain = domainBuf.ToString()
                };
                return;
            }


            networkCredential = null;
        }
    }
}