/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for the local-development secret-reference fallback: resolves
 *  successfully from this process's own environment variables, and throws a
 *  clear McpException (rather than silently returning null) when unset.
 *  KeyVaultMcpSecretResolver is intentionally not unit tested here — it is a
 *  thin wrapper over Azure.Security.KeyVault.Secrets.SecretClient requiring a
 *  live (or mocked) Key Vault, consistent with KeyVaultSecretManager having no
 *  dedicated unit test in this project either.
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System;
using System.Threading;
using System.Threading.Tasks;
using Warewolf.Execution.Lightweight.Mcp.Secrets;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.Secrets
{
    [TestClass]
    public class EnvironmentMcpSecretResolverTests
    {
        private const string VarName = "ADDSOURCE_TEST_ENV_RESOLVER_VAR";

        [TestCleanup]
        public void Cleanup() => Environment.SetEnvironmentVariable(VarName, null);

        [TestMethod]
        [TestCategory("UnitTest")]
        public async Task ResolveAsync_VariableSet_ReturnsValue()
        {
            Environment.SetEnvironmentVariable(VarName, "the-value");
            var resolver = new EnvironmentMcpSecretResolver();

            var value = await resolver.ResolveAsync(VarName, CancellationToken.None);

            Assert.AreEqual("the-value", value);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public async Task ResolveAsync_VariableNotSet_Throws()
        {
            var resolver = new EnvironmentMcpSecretResolver();

            await resolver.ResolveAsync(VarName, CancellationToken.None);
        }
    }
}
