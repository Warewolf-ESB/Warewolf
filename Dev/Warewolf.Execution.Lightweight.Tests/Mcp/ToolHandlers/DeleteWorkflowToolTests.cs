/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for DeleteWorkflowTool (delete_workflow): required-parameter checks,
 *  not-found reporting, Contribute permission gating (reusing ListWorkflowsTool's
 *  generalised rule, matching create_workflow/edit_workflow/deploy_workflow), the
 *  deliberate permission-before-existence ordering that stops the tool being used as
 *  an existence oracle, and the success path — the .bite file leaves disk and every
 *  cache that would keep serving it (WorkflowIndex, the WorkflowExecutor pool) is
 *  cleared, so the name is immediately free for reuse.
 *
 *  Also covers WorkflowIndex.Remove directly: it is new infrastructure added for this
 *  tool, and its no-op paths (blank args, never-loaded directory, absent key) are the
 *  ones a caller relies on when deleting unconditionally after a file delete.
 */

using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ModelContextProtocol;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using Warewolf.Execution.Lightweight.Auth;
using Warewolf.Execution.Lightweight.Auth.Models;
using Warewolf.Execution.Lightweight.Infrastructure;
using Warewolf.Execution.Lightweight.Logging;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class DeleteWorkflowToolTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "delete-wf-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

        // ── Test doubles (matches the sibling *ToolTests' conventions) ────────

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

        private sealed class NullExecutionLogger : IExecutionLogger
        {
            public void LogTrace(string message, Guid executionId) { }
            public void LogTrace(string message, Exception exception, Guid executionId) { }
            public void LogDebug(string message, Guid executionId) { }
            public void LogDebug(string message, Exception exception, Guid executionId) { }
            public void LogError(Exception ex, string log) { }
            public void LogInfo(string message, Guid executionId) { }
            public void LogInfo(string message, Exception exception, Guid executionId) { }
            public void LogInfo(string message) { }
            public void LogWarning(string message, Guid executionId) { }
            public void LogWarning(string message, Exception exception, Guid executionId) { }
            public void LogError(string message, Guid executionId) { }
            public void LogError(string activityName, Exception ex, Guid executionId) { }
            public void LogFatal(string message, Guid executionId) { }
            public void LogFatal(string message, Exception exception, Guid executionId) { }
        }

        private static WorkflowClaimsPrincipal Principal(params string[] roles) =>
            new(new ClaimsIdentity(
                new[] { new Claim(ClaimTypes.Name, "alice") }
                    .Concat(roles.Select(r => new Claim(ClaimTypes.Role, r))),
                "Bearer", ClaimTypes.Name, ClaimTypes.Role));

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        private static StubAuthPolicyLoader OpenLoader() => new() { IsConfigEffective = false };

        // ── Workflow fixture helpers ──────────────────────────────────────────

        static Cell MakeStartNode(string id = "start") =>
            new() { id = id, shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };

        static Cell MakeAssign(string id, string displayName, JArray fields) =>
            new()
            {
                id = id,
                shape = "rect",
                data = new Dictionary<string, object>
                {
                    ["type"] = "dsfdotnetmultiassignactivity",
                    ["displayName"] = displayName,
                    ["fields"] = fields,
                }
            };

        static Cell MakeEdge(string id, string sourceId, string targetId) =>
            new() { id = id, shape = "edge", data = new Dictionary<string, object>(), Source = new Connector(sourceId), Target = new Connector(targetId) };

        static JsonElement BodyOf(string resourceName, params Cell[] cells)
        {
            var graph = new X6WorkflowSaveModel { ResourceName = resourceName, Cells = new List<Cell>(cells) };
            return JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
        }

        static JsonElement EnvelopeOf(object envelopeObj) => System.Text.Json.JsonSerializer.SerializeToElement(envelopeObj);

        /// <summary>
        /// Creates a real, compilable workflow at <paramref name="name"/> under <see cref="_root"/>
        /// whose single Assign writes <paramref name="resultValue"/> into <c>[[Result]]</c>, so a
        /// later execution can prove <em>which</em> definition ran.
        /// </summary>
        private void CreateWorkflow(string name, string resultValue)
        {
            var fields = new JArray(new JObject { ["FieldName"] = "[[Result]]", ["FieldValue"] = resultValue, ["IndexNumber"] = 1 });
            var body = BodyOf(name, MakeStartNode(), MakeAssign("assign1", "Assign", fields), MakeEdge("e1", "start", "assign1"));
            var envelope = EnvelopeOf(new
            {
                name,
                description = "deletable workflow",
                inputs = Array.Empty<object>(),
                outputs = new[] { new { name = "Result", kind = "scalar", fields = Array.Empty<string>() } },
            });

            var created = CreateWorkflowTool.Handle(HostConfig(), OpenLoader(), null, name, envelope, body);
            Assert.IsTrue(created.Created, $"fixture workflow '{name}' was not created");
        }

        private string PathOf(string name) => Path.Combine(_root, name + ".bite");

        private DeleteWorkflowResult Handle(
            IWorkflowAuthPolicyLoader authPolicyLoader,
            ClaimsPrincipal? user,
            string name) =>
            DeleteWorkflowTool.Handle(HostConfig(), authPolicyLoader, user, name);

        // ── Tests: input validation ───────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        [ExpectedException(typeof(McpException))]
        public void Handle_BlankName_Throws()
        {
            Handle(OpenLoader(), null, "   ");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_UnknownName_ThrowsNotFound()
        {
            var ex = Assert.ThrowsException<McpException>(() => Handle(OpenLoader(), null, "NoSuchWorkflow"));

            StringAssert.Contains(ex.Message, "was not found");
        }

        // ── Tests: permission gating ──────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_NoContributePermission_ThrowsAndLeavesFileOnDisk()
        {
            CreateWorkflow("Protected", "keep me");

            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.View | WorkflowPermission.Execute,
            };

            Assert.ThrowsException<McpException>(() => Handle(loader, Principal("Developers"), "Protected"));

            Assert.IsTrue(File.Exists(PathOf("Protected")), "a permission failure must not delete the workflow");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_SecureConfigEffective_WithContributePermission_Succeeds()
        {
            CreateWorkflow("Allowed", "bye");

            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (path, _) =>
                    path.Equals("Allowed", StringComparison.OrdinalIgnoreCase)
                        ? WorkflowPermission.Contribute
                        : WorkflowPermission.None,
            };

            var result = Handle(loader, Principal("Developers"), "Allowed");

            Assert.IsTrue(result.Deleted);
            Assert.IsFalse(File.Exists(PathOf("Allowed")));
        }

        /// <summary>
        /// Permission is checked before existence, so an unauthorized caller cannot tell an
        /// existing workflow from an absent one by the error text — the tool is not an existence
        /// oracle for workflows the caller may not touch. This is the one deliberate ordering
        /// difference from <see cref="DeployWorkflowTool"/>, which resolves the file first.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_Unauthorized_ReportsIdenticallyForExistingAndMissingWorkflow()
        {
            CreateWorkflow("RealOne", "secret");

            var loader = new StubAuthPolicyLoader
            {
                IsConfigEffective = true,
                EffectivePermissions = (_, _) => WorkflowPermission.None,
            };

            var existing = Assert.ThrowsException<McpException>(() => Handle(loader, Principal("Outsiders"), "RealOne"));
            var missing = Assert.ThrowsException<McpException>(() => Handle(loader, Principal("Outsiders"), "NotEvenThere"));

            StringAssert.Contains(existing.Message, "do not have permission");
            StringAssert.Contains(missing.Message, "do not have permission");
            Assert.AreEqual(
                existing.Message.Replace("RealOne", "{name}"),
                missing.Message.Replace("NotEvenThere", "{name}"),
                "the two errors must differ only by the echoed name, or existence leaks");
        }

        // ── Tests: success path ───────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_ExistingWorkflow_DeletesFileAndReportsDeleted()
        {
            CreateWorkflow("Doomed", "gone");
            Assert.IsTrue(File.Exists(PathOf("Doomed")), "fixture precondition");

            var result = Handle(OpenLoader(), null, "Doomed");

            Assert.AreEqual("Doomed", result.Name);
            Assert.IsTrue(result.Deleted);
            Assert.IsFalse(File.Exists(PathOf("Doomed")), "the .bite file must be gone from disk");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_AfterDelete_GetWorkflowDefinitionNoLongerResolvesTheName()
        {
            CreateWorkflow("Vanishing", "poof");

            // Prime WorkflowIndex with the name, so the assertion below proves the entry was
            // actively removed rather than never having been cached.
            var definition = GetWorkflowDefinitionTool.Handle(HostConfig(), OpenLoader(), null, "Vanishing");
            Assert.AreEqual("Vanishing", definition.Name, "fixture precondition");

            Handle(OpenLoader(), null, "Vanishing");

            Assert.ThrowsException<McpException>(
                () => GetWorkflowDefinitionTool.Handle(HostConfig(), OpenLoader(), null, "Vanishing"),
                "a deleted workflow must stop resolving without waiting for a process restart");

            // The assertion above passes even with a stale index entry, because
            // WorkflowNameResolver.Resolve guards its index hit with File.Exists. The HTTP
            // execution path does NOT: WorkflowFunctionHelper.ResolveWorkflowFilePath trusts
            // WorkflowIndex.Resolve outright and would hand a deleted file's path straight to
            // the executor. So assert the index itself was cleared — that is the entry
            // WorkflowIndex.Remove exists to drop.
            Assert.IsNull(
                WorkflowIndex.Instance.Resolve(_root, "Vanishing"),
                "the index entry must be dropped, or the HTTP path still routes to the deleted file");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Handle_AfterDelete_TheNameIsFreeForReuse()
        {
            CreateWorkflow("Recycled", "first");
            Handle(OpenLoader(), null, "Recycled");

            // create_workflow rejects an existing name, so this only passes if no stale index
            // entry survived the delete.
            CreateWorkflow("Recycled", "second");

            Assert.IsTrue(File.Exists(PathOf("Recycled")));
        }

        /// <summary>
        /// Delete → re-create at the same name → execute must run the NEW definition. The
        /// WorkflowExecutor pool is keyed on path + timestamp + length, so this mostly guards the
        /// gap that key cannot see (a same-length rewrite inside the filesystem's timestamp
        /// granularity) — the case <see cref="WorkflowExecutor.EvictWorkflow"/> exists for.
        /// </summary>
        [TestMethod]
        [TestCategory("UnitTest")]
        public async System.Threading.Tasks.Task Handle_DeleteThenRecreate_ExecutesTheNewDefinition()
        {
            Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", "false");
            try
            {
                CreateWorkflow("Swapped", "old value");
                var executor = new WorkflowExecutor(new NullExecutionLogger());

                var before = executor.Execute(PathOf("Swapped"), new Dictionary<string, string>());
                Assert.IsTrue(before.IsSuccess, "errors: " + string.Join("; ", before.Errors ?? new List<string>()));
                StringAssert.Contains(
                    await before.ReadPayloadAsync(),
                    "old value",
                    "fixture precondition: the original definition ran");

                Handle(OpenLoader(), null, "Swapped");
                CreateWorkflow("Swapped", "new value");

                var after = new WorkflowExecutor(new NullExecutionLogger()).Execute(PathOf("Swapped"), new Dictionary<string, string>());

                Assert.IsTrue(after.IsSuccess, "errors: " + string.Join("; ", after.Errors ?? new List<string>()));
                StringAssert.Contains(
                    await after.ReadPayloadAsync(),
                    "new value",
                    "the re-created definition must run, not a pooled compilation of the deleted one");
            }
            finally
            {
                Environment.SetEnvironmentVariable("WAREWOLF_LICENSE_CHECK_ENABLED", null);
            }
        }

        // ── Tests: WorkflowIndex.Remove (new infrastructure) ──────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void WorkflowIndexRemove_BlankArguments_ReturnsSilently()
        {
            WorkflowIndex.Instance.Remove(string.Empty, "anything");
            WorkflowIndex.Instance.Remove(_root, "   ");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void WorkflowIndexRemove_DirectoryNeverLoadedAndKeyAbsent_IsANoOp()
        {
            var neverLoaded = Directory.CreateDirectory(
                Path.Combine(Path.GetTempPath(), "delete-wf-index-" + Guid.NewGuid().ToString("N"))).FullName;
            try
            {
                WorkflowIndex.Instance.Remove(neverLoaded, "not/in/the/index");

                Assert.IsNull(WorkflowIndex.Instance.Resolve(neverLoaded, "not/in/the/index"));
            }
            finally
            {
                Directory.Delete(neverLoaded, recursive: true);
            }
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void WorkflowIndexRemove_IsCaseAndSeparatorInsensitive()
        {
            CreateWorkflow("Folder/Nested", "x");
            Assert.IsNotNull(WorkflowIndex.Instance.Resolve(_root, "Folder/Nested"), "fixture precondition");

            WorkflowIndex.Instance.Remove(_root, @"FOLDER\NESTED");

            Assert.IsNull(WorkflowIndex.Instance.Resolve(_root, "Folder/Nested"));
        }
    }
}
