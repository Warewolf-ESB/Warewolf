/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for the workflow-form tools (get_ui_schema, create_ui, get_ui, delete_ui): validation
 *  against the workflow's envelope, Contribute/View gating, replace-on-create, the .ui.json file
 *  next to the workflow, and delete_workflow taking the form with it.
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
using Warewolf.Execution.Lightweight.Mcp;
using Warewolf.Execution.Lightweight.Mcp.ToolHandlers;
using Warewolf.Execution.Lightweight.Tests.TestSupport;

namespace Warewolf.Execution.Lightweight.Tests.Mcp.ToolHandlers
{
    [TestClass]
    public class WorkflowUiToolsTests
    {
        private string _root = string.Empty;

        [TestInitialize]
        public void Setup() => _root = Directory.CreateDirectory(
            Path.Combine(Path.GetTempPath(), "workflow-ui-tests-" + Guid.NewGuid().ToString("N"))).FullName;

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_root))
            {
                Directory.Delete(_root, recursive: true);
            }
        }

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

        private HostEnvironmentConfig HostConfig() => McpToolTestHostConfig.ForWorkflowsDirectory(_root);

        static readonly StubAuthPolicyLoader OpenPolicy = new() { IsConfigEffective = false };

        static JsonElement Json(object obj) => System.Text.Json.JsonSerializer.SerializeToElement(obj);

        // A workflow with one input of each kind: CustomerId (scalar), Filter (object) and
        // Orders (recordset with input fields Sku and Qty), and two outputs.
        string CreateFixtureWorkflow(string name = "OrderEntry")
        {
            // create_workflow wants a reachable step that uses every declared variable.
            var assignments = new (string Field, string Value)[]
            {
                ("[[Result]]", "[[CustomerId]]"),
                ("[[Lines().Sku]]", "[[Orders().Sku]]"),
                ("[[Lines().Total]]", "[[Orders().Qty]]"),
                ("[[Result]]", "[[@Filter]]"),
            };
            var fields = new JArray(assignments.Select((a, i) => new JObject
            {
                ["FieldName"] = a.Field,
                ["FieldValue"] = a.Value,
                ["IndexNumber"] = i + 1,
            }));
            var assignId = Guid.NewGuid().ToString();
            var start = new Cell { id = "start", shape = "rect", data = new Dictionary<string, object> { ["type"] = Constants.START } };
            var assign = new Cell
            {
                id = assignId,
                shape = "rect",
                data = new Dictionary<string, object> { ["type"] = "dsfdotnetmultiassignactivity", ["displayName"] = "Assign", ["fields"] = fields },
            };
            var edge = new Cell { id = "e1", shape = "edge", data = new Dictionary<string, object>(), Source = new Connector("start"), Target = new Connector(assignId) };
            var graph = new X6WorkflowSaveModel { ResourceName = name, Cells = new List<Cell> { start, assign, edge } };
            var body = JsonDocument.Parse(JsonConvert.SerializeObject(graph)).RootElement;
            var envelope = Json(new
            {
                name,
                description = "",
                inputs = new object[]
                {
                    new { name = "CustomerId", kind = "scalar" },
                    new { name = "Filter", kind = "object" },
                    new { name = "Orders", kind = "recordset", fields = new[] { "Sku", "Qty" } },
                },
                outputs = new object[]
                {
                    new { name = "Result", kind = "scalar" },
                    new { name = "Lines", kind = "recordset", fields = new[] { "Sku", "Total" } },
                },
            });

            CreateWorkflowTool.Handle(HostConfig(), OpenPolicy, null, name, envelope, body);
            return name;
        }

        static object ValidForm() => new
        {
            title = "Enter an order",
            submitLabel = "Place order",
            fields = new object[]
            {
                new { input = "CustomerId", label = "Customer", control = "select", required = true,
                      options = new[] { new { value = "c1", label = "Acme" }, new { value = "c2", label = "Globex" } } },
                new { input = "Filter", control = "json" },
                new { input = "Orders", control = "table",
                      columns = new object[] { new { field = "Sku" }, new { field = "Qty", control = "number", required = true } } },
            },
            outputs = new object[] { new { output = "Lines", display = "table" } },
        };

        static string MessageOf(Action action)
        {
            try
            {
                action();
            }
            catch (McpException ex)
            {
                return ex.Message;
            }

            Assert.Fail("Expected an McpException.");
            return string.Empty;
        }

        CreateUiResult Create(string name, object form, ClaimsPrincipal? user = null, IWorkflowAuthPolicyLoader? loader = null) =>
            CreateUiTool.Handle(HostConfig(), loader ?? OpenPolicy, user, name, null, Json(form));

        // ── create_ui ─────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateUi_ValidForm_SavesNextToTheWorkflow()
        {
            var name = CreateFixtureWorkflow();

            var result = Create(name, ValidForm());

            Assert.IsTrue(result.Saved);
            Assert.IsFalse(result.Replaced);
            var saved = File.ReadAllText(Path.Combine(_root, name + ".ui.json"));
            Assert.IsTrue(saved.Contains("\"Place order\""), saved);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateUi_Again_ReplacesTheForm()
        {
            var name = CreateFixtureWorkflow();
            Create(name, ValidForm());

            var result = Create(name, new { title = "Second", fields = new[] { new { input = "CustomerId" } } });

            Assert.IsTrue(result.Replaced);
            var form = GetUiTool.Handle(HostConfig(), OpenPolicy, null, name, null).Form!.Value;
            Assert.AreEqual("Second", form.GetProperty("title").GetString());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateUi_ReportsEveryProblemAtOnce()
        {
            var name = CreateFixtureWorkflow();

            var message = MessageOf(() => Create(name, new
            {
                title = "",
                fields = new object[]
                {
                    new { input = "Nope" },
                    new { input = "CustomerId", control = "table" },
                    new { input = "Orders", columns = new[] { new { field = "Price" } } },
                    new { input = "Filter", control = "json", options = new[] { new { value = "x" } } },
                },
                outputs = new[] { new { output = "Missing" } },
            }));

            StringAssert.Contains(message, "`form.title` is required");
            StringAssert.Contains(message, "('Nope') is not one of the workflow's inputs: CustomerId, Filter, Orders");
            StringAssert.Contains(message, "('table') does not suit scalar input 'CustomerId'");
            StringAssert.Contains(message, "('Price') is not an input field of recordset 'Orders': Sku, Qty");
            StringAssert.Contains(message, "`form.fields[3]`.options only applies to a select or radio control");
            StringAssert.Contains(message, "('Missing') is not one of the workflow's outputs: Result, Lines");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateUi_SelectWithoutOptions_Throws()
        {
            var name = CreateFixtureWorkflow();

            var message = MessageOf(() => Create(name, new { title = "t", fields = new[] { new { input = "CustomerId", control = "select" } } }));

            StringAssert.Contains(message, "options must list the choices");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateUi_UnknownProperty_Throws()
        {
            var name = CreateFixtureWorkflow();

            var message = MessageOf(() => Create(name, new { title = "t", fields = new[] { new { input = "CustomerId", lable = "Customer" } } }));

            StringAssert.Contains(message, "could not be parsed");
            Assert.IsFalse(File.Exists(Path.Combine(_root, name + ".ui.json")));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateUi_DuplicateInput_Throws()
        {
            var name = CreateFixtureWorkflow();

            var message = MessageOf(() => Create(name, new { title = "t", fields = new[] { new { input = "CustomerId" }, new { input = "CustomerId" } } }));

            StringAssert.Contains(message, "appears in more than one field");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateUi_ViewOnly_Throws()
        {
            var name = CreateFixtureWorkflow();
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.View };

            var message = MessageOf(() => Create(name, ValidForm(), Principal("Viewers"), loader));

            StringAssert.Contains(message, "do not have permission");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void CreateUi_WorkflowNotFound_Throws()
        {
            var message = MessageOf(() => Create("NoSuchWorkflow", ValidForm()));

            StringAssert.Contains(message, "was not found");
        }

        // ── get_ui ────────────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetUi_NoForm_ReturnsTheEnvelopeAndNullForm()
        {
            var name = CreateFixtureWorkflow();

            var result = GetUiTool.Handle(HostConfig(), OpenPolicy, null, name, null);

            Assert.IsNull(result.Form);
            CollectionAssert.AreEqual(new[] { "CustomerId", "Filter", "Orders" }, result.Envelope.Inputs.Select(i => i.Name).ToArray());
            CollectionAssert.AreEqual(new[] { "Sku", "Qty" }, result.Envelope.Inputs[2].Fields!.ToArray());
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetUi_WithoutViewPermission_Throws()
        {
            var name = CreateFixtureWorkflow();
            var loader = new StubAuthPolicyLoader { IsConfigEffective = true, EffectivePermissions = (_, _) => WorkflowPermission.None };

            var message = MessageOf(() => GetUiTool.Handle(HostConfig(), loader, Principal("Nobody"), name, null));

            StringAssert.Contains(message, "do not have permission to view");
        }

        // ── delete_ui / delete_workflow ───────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DeleteUi_RemovesTheForm_ThenHasNothingToDelete()
        {
            var name = CreateFixtureWorkflow();
            Create(name, ValidForm());

            Assert.IsTrue(DeleteUiTool.Handle(HostConfig(), OpenPolicy, null, name, null).Deleted);
            Assert.IsNull(GetUiTool.Handle(HostConfig(), OpenPolicy, null, name, null).Form);
            StringAssert.Contains(MessageOf(() => DeleteUiTool.Handle(HostConfig(), OpenPolicy, null, name, null)), "has no form");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void DeleteWorkflow_TakesItsFormWithIt()
        {
            var name = CreateFixtureWorkflow();
            Create(name, ValidForm());

            DeleteWorkflowTool.Handle(HostConfig(), OpenPolicy, null, name);

            Assert.IsFalse(File.Exists(Path.Combine(_root, name + ".ui.json")));
        }

        // ── get_ui_schema ─────────────────────────────────────────────────────

        [TestMethod]
        [TestCategory("UnitTest")]
        public void GetUiSchema_NamesEveryControlValidationAccepts()
        {
            var schema = GetUiSchemaTool.Handle().FormSchema.GetRawText();

            foreach (var control in WorkflowUiCatalog.ControlsByKind.Values.SelectMany(c => c).Concat(WorkflowUiCatalog.ColumnControls))
            {
                StringAssert.Contains(schema, control);
            }
        }
    }
}
