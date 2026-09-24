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

namespace Warewolf.HangfireServer.Tests.TestSupport
{
    /// <summary>
    /// Guards for tests whose code path depends on a Windows-only platform feature.
    /// </summary>
    internal static class PlatformGuards
    {
        /// <summary>
        /// Marks the calling test Inconclusive off Windows. <c>Args.ShowConsole = true</c> makes
        /// <c>Program.Implementation.Run()</c> construct a <see cref="Warewolf.Common.ConsoleWindow"/>,
        /// which P/Invokes <c>kernel32.dll</c> (<c>AllocConsole</c>/<c>GetStdHandle</c>) and is
        /// genuinely Windows-only.
        /// </summary>
        internal static void RequireWindowsConsole()
        {
            if (!OperatingSystem.IsWindows())
            {
                Assert.Inconclusive(
                    "Requires a Windows console window (ConsoleWindow P/Invokes kernel32.dll AllocConsole). " +
                    "Run on Windows, or set Args.ShowConsole = false if the test does not need verbose console output.");
            }
        }
    }
}
