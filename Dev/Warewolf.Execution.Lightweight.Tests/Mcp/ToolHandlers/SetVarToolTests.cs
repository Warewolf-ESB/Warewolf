/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for SetVarTool (set_var): required-name validation,
 *  secret-shaped-name refusal, Administrator permission gating (stricter than
 *  the Contribute bar used by workflow/source authoring tools), open-access
 *  bypass when secure.config isn't effective, and the actual
 *  Environment.SetEnvironmentVariable set/unset behaviour + response shape
 *  (never echoes the value back).
 */

using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class SetVarToolTests
    {
        private string _varName = string.Empty;

        [TestInitialize]
        public void Setup() => _varName = "SET_VAR_TEST_" + Guid.NewGuid().ToString("N");

        [TestCleanup]
        public void Cleanup() => Environment.SetEnvironmentVariable(_varName, null);

        // ── Test doubles (matches AddSourceToolTests' conventions) ──

        private sealed class StubAuthPolicyLoader : IWorkflowAuthPolicyLoader
        {
            public bool IsConfigEffective { get; set; }
            public int PolicyCount => 0;
            public PolicyLookupResult GetPolicy(string workflowName) => PolicyLookupResult.ConfigMissing();

            public Func<string, IEnumerable<string>, WorkflowPermission> EffectivePermissions { get; set; } =
                (_, _) => WorkflowPermission.None;

            public WorkflowPermission GetEffectivePermissions(string workflowName, IEnumerable<string> callerRoles) =>
                EffectivePermissions(workflowName, callerRoles);

            public void Reload() { }
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };

        // ── Tests: input validation ────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws()
        {
            SetVarTool.Handle(OpenPolicy, null, "   ", "value");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_EmptyName_Throws()
        {
            SetVarTool.Handle(OpenPolicy, null, "", "value");
        }

        // ── Tests: secret-shaped name refusal ───────────────────────────────────

        [DataTestMethod]
        [TestCategory("UnitTest")]
        [DataRow("DB_PASSWORD")]
        [DataRow("MY_SECRET")]
        [DataRow("API_TOKEN")]
        [DataRow("SqlConnectionString")]
        [DataRow("ADMIN_CREDENTIAL")]
        [DataRow("STRIPE_APIKEY")]
        [DataRow("AAD_CLIENTSECRET")]
        [DataRow("TLS_PRIVATEKEY")]
        [DataRow("ftp_pwd")]
        public void Handle_SecretShapedName_Throws(string sensitiveName)
        {
            var ex = Assert.ThrowsException<McpException>(() => SetVarTool.Handle(OpenPolicy, null, sensitiveName, "value"));
            StringAssert.Contains(ex.Message, "looks like it holds a secret or credential");

            // Never actually set — refused before Environment.SetEnvironmentVariable is called.
            Assert.IsNull(Environment.GetEnvironmentVariable(sensitiveName));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NonSensitiveName_DoesNotThrow()
        {
            var result = SetVarTool.Handle(OpenPolicy, null, _varName, "value");
            Assert.AreEqual(_varName, result.Name);
        }

        // ── Tests: permission gating ─────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_SecureConfigEffective_NoPermission_ThrowsPermissionDenied()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.None,
            };

            SetVarTool.Handle(loader, Principal("Developers"), _varName, "value");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_SecureConfigEffective_ContributeOnly_ThrowsPermissionDenied()
        {
            // set_var requires Administrator specifically — Contribute (sufficient for
            // add_source/create_workflow) must NOT be enough for a host-wide env-var change.
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.Contribute,
            };

            SetVarTool.Handle(loader, Principal("Developers"), _varName, "value");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_WithAdministratorPermission_Succeeds()
        {
            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.Administrator,
            };

            var result = SetVarTool.Handle(loader, Principal("Admins"), _varName, "value");

            Assert.IsTrue(result.Set);
            Assert.AreEqual("value", Environment.GetEnvironmentVariable(_varName));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigNotEffective_NoPrincipalRequired_Succeeds()
        {
            var result = SetVarTool.Handle(OpenPolicy, null, _varName, "value");

            Assert.IsTrue(result.Set);
            Assert.AreEqual("value", Environment.GetEnvironmentVariable(_varName));
        }

        // ── Tests: set/unset behaviour and response shape ───────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SetsEnvironmentVariable_Immediately()
        {
            SetVarTool.Handle(OpenPolicy, null, _varName, "hello-world");

            Assert.AreEqual("hello-world", Environment.GetEnvironmentVariable(_varName));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NullValue_UnsetsVariable()
        {
            Environment.SetEnvironmentVariable(_varName, "pre-existing");

            var result = SetVarTool.Handle(OpenPolicy, null, _varName, null);

            Assert.IsFalse(result.Set);
            Assert.IsTrue(result.HadPreviousValue);
            Assert.IsNull(Environment.GetEnvironmentVariable(_varName));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_HadPreviousValue_FalseWhenNotPreviouslySet()
        {
            var result = SetVarTool.Handle(OpenPolicy, null, _varName, "value");

            Assert.IsFalse(result.HadPreviousValue);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_HadPreviousValue_TrueWhenPreviouslySet()
        {
            Environment.SetEnvironmentVariable(_varName, "old-value");

            var result = SetVarTool.Handle(OpenPolicy, null, _varName, "new-value");

            Assert.IsTrue(result.HadPreviousValue);
            Assert.AreEqual("new-value", Environment.GetEnvironmentVariable(_varName));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_NameIsTrimmed()
        {
            var result = SetVarTool.Handle(OpenPolicy, null, $"  {_varName}  ", "value");

            Assert.AreEqual(_varName, result.Name);
            Assert.AreEqual("value", Environment.GetEnvironmentVariable(_varName));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Response_NeverContainsTheValue()
        {
            var result = SetVarTool.Handle(OpenPolicy, null, _varName, "hunter2literal");

            var serialized = JsonSerializer.Serialize(result);
            StringAssert.DoesNotMatch(serialized, new System.Text.RegularExpressions.Regex("hunter2literal"));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Response_NotesRestartCaveat()
        {
            var result = SetVarTool.Handle(OpenPolicy, null, _varName, "value");

            StringAssert.Contains(result.Note, "restart");
        }
    }
}
