/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Driver.Persistence.Tests.TestSupport
{
    /// <summary>
    /// Guards for tests whose code path depends on a Windows-only platform feature.
    /// </summary>
    internal static class PlatformGuards
    {
        /// <summary>
        /// Marks the calling test Inconclusive off Windows. <c>HangfireScheduler.ResumeJob</c>'s
        /// success path encrypts the suspended environment via <c>DpapiWrapper.Encrypt</c>, which
        /// falls back to Windows DPAPI (<c>ProtectedData</c>) when no AES hook is registered and
        /// throws <see cref="PlatformNotSupportedException"/> elsewhere. The Server engine never
        /// registers an AES hook (only the Lightweight engine's Key Vault startup extension does).
        /// </summary>
        internal static void RequireWindowsDpapi()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Inconclusive(
                    "Requires Windows DPAPI (DpapiWrapper.Encrypt with no AES hook registered). " +
                    "Run on Windows; the Server engine has no AES hook equivalent to the Lightweight engine's Key Vault hook.");
            }
        }
    }
}
