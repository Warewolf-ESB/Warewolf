/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 */

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Execution.Lightweight.Tests.TestSupport;

/// <summary>
/// Guards for tests whose code path depends on a Windows-only platform feature.
/// </summary>
internal static class PlatformGuards
{
    /// <summary>
    /// Marks the calling test Inconclusive off Windows. Tools such as <c>add_source</c>,
    /// <c>edit_source</c> and <c>create_test</c> encrypt through <c>DpapiWrapper.Encrypt</c>, which
    /// falls back to Windows DPAPI (<c>ProtectedData</c>) when no AES hook is registered and throws
    /// <see cref="PlatformNotSupportedException"/> elsewhere. In Azure the engine never takes that
    /// path: <c>KeyVaultStartupExtensions</c> registers the Key Vault-backed AES hooks at cold start.
    /// </summary>
    internal static void RequireWindowsDpapi()
    {
        if (!OperatingSystem.IsWindows())
        {
            Assert.Inconclusive(
                "Requires Windows DPAPI (DpapiWrapper.Encrypt with no AES hook registered). " +
                "Run on Windows; in Azure the engine encrypts through the Key Vault AES hook instead.");
        }
    }
}
