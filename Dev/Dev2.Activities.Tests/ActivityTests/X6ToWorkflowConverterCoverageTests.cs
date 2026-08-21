/*
 * Coverage tests for Dev2.Activities.WF.X6ToWorkflowConverter.
 *
 * Why this file exists
 * --------------------
 * The merged Cobertura snapshot shows X6ToWorkflowConverter at 0% line coverage
 * (840 uncovered lines) — the second-largest uncovered surface in the merged
 * report after Warewolf.Resource (which is a pure resource file).  The class is
 * a pure XML/JSON translation layer with no external dependencies, so it is
 * cheaply unit-testable without a host.
 *
 * The single existing test (SwitchVariableCorruptionLoggingTests) only exercises
 * the FlowSwitch corruption branch.  This file fans out coverage to:
 *
 *   • The X6JsonToWorkflow round-trip for a representative cross-section of the
 *     activity-type switch in CreateActivityFromNode — start, assign, decision,
 *     sequence, foreach, calculate, random, comment, unknown — every entry
 *     drives one or more case arms that were previously dead in coverage.
 *
 *   • Edge-cases on the public entry points: null/empty resource JSON, missing
 *     "ResourceJSON" dictionary key, malformed JSON, and the static-helper
 *     X6JsonToXaml(Dictionary<...>) overload.
 *
 *   • The pure XML transformation static helpers (ReplaceBadCollection,
 *     ReplaceSystemNamespace, AddReplaceNameSpace, ReplaceDefaultNamespace).
 *
 * Tests assert on the most stable contract — that conversion produces a
 * non-empty StringBuilder and that helpers normalise the well-known
 * System.Private.CoreLib → mscorlib substitution.  Asserting on the exact XAML
 * output would tightly couple to formatting decisions inside WorkflowHelper.
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Dev2.Activities.WF;
using Dev2.Common.X6;
using Dev2.Data.SystemTemplates.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Dev2.Tests.Activities.ActivityTests
{
    [TestClass]
    public class X6ToWorkflowConverterCoverageTests
    {
        // ─────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────

        static Cell MakeNode(string type, string displayName = null, Dictionary<string, object> extraData = null)
        {
            var id = Guid.NewGuid().ToString();
            var data = new Dictionary<string, object>
            {
                ["type"]                  = type,
                [Constants.DISPLAYNAME]   = displayName ?? type,
                [Constants.PROPERTY_UNIQUEID] = id
            };
            if (extraData != null)
            {
                foreach (var kv in extraData) data[kv.Key] = kv.Value;
            }
            return new Cell { id = id, data = data };
        }


        /// <summary>
        /// A DsfDecision's EXPRESSION is a serialized <see cref="Dev2DecisionStack"/>, not a raw
        /// Warewolf expression. Passing "[[a]] = 1" made Newtonsoft read the leading '[' as the start
        /// of a JSON array and throw. That went unnoticed because the decision node was being dropped
        /// before the expression was ever parsed.
        /// </summary>
        static string DecisionStack(string displayText) =>
            JsonConvert.SerializeObject(new Dev2DecisionStack
            {
                TheStack      = new List<Dev2Decision>(),
                DisplayText   = displayText,
                TrueArmText   = "Yes",
                FalseArmText  = "No"
            });

        static Cell MakeStartNode()
        {
            // The converter's start-node detection lower-cases the type and compares to Constants.START.
            return new Cell
            {
                id   = Guid.NewGuid().ToString(),
                data = new Dictionary<string, object> { ["type"] = Constants.START }
            };
        }

        static string Serialise(string resourceName, params Cell[] cells)
        {
            var graph = new X6WorkflowSaveModel
            {
                ResourceName = resourceName,
                Cells        = new List<Cell>(cells)
            };
            return JsonConvert.SerializeObject(graph);
        }

        static StringBuilder Convert(string resourceName, params Cell[] cells)
        {
            var converter = new X6ToWorkflowConverter();
            return converter.X6JsonToWorkflow(Serialise(resourceName, cells));
        }

        // ─────────────────────────────────────────────────────────────────
        // X6JsonToWorkflow — activity-type fan-out
        // Each case below drives a distinct switch arm of CreateActivityFromNode.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_EmptyGraph_ThrowsEmptyWorkflowGraphException()
        {
            // An empty graph has no cell that resolves to a start node, so BuildWorkflow
            // falls back to a bare Sequence and EnsureImplementation cannot finalise it.
            // This is now a structured, named exception rather than a raw NullReferenceException
            // (round-trip fidelity gate hardening — see warewolf-lee-mcp-v3-addendum-a.md).
            var converter = new X6ToWorkflowConverter();
            Assert.ThrowsException<EmptyWorkflowGraphException>(() =>
                converter.X6JsonToWorkflow(Serialise("EmptyWorkflow")));
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_StartNodeOnly_ProducesNonEmptyXaml()
        {
            var result = Convert("StartOnly", MakeStartNode());

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_AssignActivity_ProducesNonEmptyXaml()
        {
            var assign = MakeNode("DsfDotNetMultiAssignActivity", "Assign A=1",
                new Dictionary<string, object>
                {
                    ["fields"] = JsonConvert.SerializeObject(new[]
                    {
                        new { FieldName = "[[a]]", FieldValue = "1" }
                    })
                });

            var result = Convert("AssignFlow", MakeStartNode(), assign);

            Assert.IsTrue(result.Length > 0,
                "Workflow with a single Assign activity should produce non-empty XAML");
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_DecisionActivity_ProducesNonEmptyXaml()
        {
            var decision = MakeNode("DsfDecision", "Is A == 1",
                new Dictionary<string, object>
                {
                    // CreateDecisionActivity reads DISPLAYTEXT, not DISPLAYNAME. Without it the
                    // decision was silently dropped and this test passed on the start node alone.
                    [Constants.DISPLAYTEXT]  = "Is A == 1",
                    [Constants.EXPRESSION]   = DecisionStack("Is A == 1"),
                    [Constants.TRUEARMTEXT]  = "Yes",
                    [Constants.FALSEARMTEXT] = "No"
                });

            var result = Convert("DecisionFlow", MakeStartNode(), decision);

            Assert.IsTrue(result.Length > 0,
                "Workflow with a Decision activity should produce non-empty XAML");
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_SequenceActivity_ProducesNonEmptyXaml()
        {
            var seq = MakeNode("DsfSequenceActivity", "MySequence");
            var result = Convert("SeqFlow", MakeStartNode(), seq);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_ForEachActivity_ProducesNonEmptyXaml()
        {
            var fe = MakeNode("DsfForEachActivity", "ForEachLoop",
                new Dictionary<string, object>
                {
                    ["foreachtype"] = "NumOfExecution",
                    ["numofexecutions"] = "3"
                });
            var result = Convert("ForEachFlow", MakeStartNode(), fe);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_CommentActivity_ProducesNonEmptyXaml()
        {
            var comment = MakeNode("DsfCommentActivity", "Just a comment",
                new Dictionary<string, object> { ["text"] = "Hello there" });
            var result = Convert("CommentFlow", MakeStartNode(), comment);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_RandomActivity_ProducesNonEmptyXaml()
        {
            var random = MakeNode("DsfRandomActivity", "RandomNum",
                new Dictionary<string, object>
                {
                    ["randomtype"] = "Numbers",
                    ["length"]     = "8",
                    ["result"]     = "[[r]]"
                });
            var result = Convert("RandomFlow", MakeStartNode(), random);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_CalculateActivity_PreservesExpressionAndResult()
        {
            // Regression test for the "Calculate" gap documented in
            // X6-Converter-Missing-Activity-Support-Spec.md §3.1: the legacy DsfCalculateActivity
            // had no ToX6Json/FromX6Json at all, so a Calculate node fell through
            // CreateActivityFromNode's default case and threw UnsupportedActivityTypeException.
            // Assert on the actual Expression/Result values surviving the round trip, not merely
            // that XAML was produced — see spec §4 for why the weaker assertion previously hid
            // vacuous coverage.
            var calculate = MakeNode("DsfCalculateActivity", "Calculate",
                new Dictionary<string, object>
                {
                    [Constants.CALCULATE_EXPRESSION] = "1+1",
                    [Constants.CALCULATE_RESULT] = "[[legacyCalcResult]]"
                });

            var result = Convert("CalculateFlow", MakeStartNode(), calculate);
            var xaml = result.ToString();

            Assert.IsTrue(xaml.Contains("1+1"), "The Calculate activity's Expression should survive the round trip.\n" + xaml);
            Assert.IsTrue(xaml.Contains("legacyCalcResult"), "The Calculate activity's Result should survive the round trip.\n" + xaml);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_DotNetCalculateActivity_PreservesExpressionAndResult()
        {
            // DsfDotNetCalculateActivity had a ToX6Json but no FromX6Json override at all, so this
            // direction of the round trip silently dropped Expression/Result even once dispatch
            // was wired up (see spec §3.1 step 3).
            var calculate = MakeNode("DsfDotNetCalculateActivity", "Calculate",
                new Dictionary<string, object>
                {
                    [Constants.CALCULATE_EXPRESSION] = "2+2",
                    [Constants.CALCULATE_RESULT] = "[[dotnetCalcResult]]"
                });

            var result = Convert("DotNetCalculateFlow", MakeStartNode(), calculate);
            var xaml = result.ToString();

            Assert.IsTrue(xaml.Contains("2+2"), "The Calculate activity's Expression should survive the round trip.\n" + xaml);
            Assert.IsTrue(xaml.Contains("dotnetCalcResult"), "The Calculate activity's Result should survive the round trip.\n" + xaml);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_UnknownActivityType_ThrowsUnsupportedActivityTypeException()
        {
            // Unknown 'type' values fall through the switch to the default case, which now
            // throws instead of silently substituting a no-op WriteLine activity — an LLM/MCP
            // caller must see this as a hard error, not a silently-corrupted workflow (round-trip
            // fidelity gate hardening — see warewolf-lee-mcp-v3-addendum-a.md).
            var unknown = MakeNode("ThisIsNotARealActivityType");

            var ex = Assert.ThrowsException<UnsupportedActivityTypeException>(() =>
                Convert("UnknownTypeFlow", MakeStartNode(), unknown));

            Assert.AreEqual("ThisIsNotARealActivityType", ex.ActivityType);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_NodeMissingType_ThrowsRatherThanBeingSkipped()
        {
            // A node whose 'data' has no 'type' key cannot be turned into an activity. This used to
            // be skipped in silence, so the converter returned a workflow quietly missing a step
            // while reporting success. It now fails loudly instead — see the call site in
            // X6ToWorkflowConverter.X6JsonToActivityBuilder.
            var noType = new Cell
            {
                id   = Guid.NewGuid().ToString(),
                data = new Dictionary<string, object> { ["displayname"] = "no-type" }
            };

            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                Convert("NoTypeFlow", MakeStartNode(), noType));

            StringAssert.Contains(ex.Message, noType.id);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_ChainedAssignAndDecision_ProducesNonEmptyXaml()
        {
            // Two activities + an edge → exercises the BuildWorkflow connection wiring.
            var startId   = Guid.NewGuid().ToString();
            var assignId  = Guid.NewGuid().ToString();
            var decisionId = Guid.NewGuid().ToString();

            var start = new Cell
            {
                id   = startId,
                data = new Dictionary<string, object> { ["type"] = Constants.START }
            };

            var assign = new Cell
            {
                id   = assignId,
                data = new Dictionary<string, object>
                {
                    ["type"] = "DsfDotNetMultiAssignActivity",
                    [Constants.DISPLAYNAME] = "Assign",
                    [Constants.PROPERTY_UNIQUEID] = assignId,
                    ["fields"] = JsonConvert.SerializeObject(new[]
                        { new { FieldName = "[[x]]", FieldValue = "42" } })
                }
            };

            var decision = new Cell
            {
                id   = decisionId,
                data = new Dictionary<string, object>
                {
                    ["type"] = "DsfDecision",
                    [Constants.DISPLAYNAME] = "Is x == 42",
                    // CreateDecisionActivity reads DISPLAYTEXT; without it the decision was dropped
                    // and this test's "non-empty XAML" assertion never actually covered the decision.
                    [Constants.DISPLAYTEXT]  = "Is x == 42",
                    [Constants.EXPRESSION]  = DecisionStack("Is x == 42"),
                    [Constants.PROPERTY_UNIQUEID] = decisionId
                }
            };

            // An edge connects start → assign → decision
            var edge1 = new Cell
            {
                id     = Guid.NewGuid().ToString(),
                shape  = "edge",
                Source = new Connector(startId),
                Target = new Connector(assignId)
            };
            var edge2 = new Cell
            {
                id     = Guid.NewGuid().ToString(),
                shape  = "edge",
                Source = new Connector(assignId),
                Target = new Connector(decisionId)
            };

            var result = Convert("ChainedFlow", start, assign, decision, edge1, edge2);

            Assert.IsTrue(result.Length > 0,
                "Multi-activity graph with edges should produce non-empty XAML");
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_DecisionWithTrueFalseEdges_WiresBothBranches()
        {
            // Regression test for the round-trip fidelity bug found via
            // RoundTripFidelityTests ("Count Records" sample, AdvancedRecordsetAcceptanceTest.bite):
            // HandleDecisionConnection only wires FlowDecision.True/False when the connecting edge
            // carries Constants.ISDECISIONARM/ISTRUEARM data. WorkflowToX6Converter's CreateEdge
            // previously never set these flags, so every round-tripped FlowDecision came back with
            // null True/False branches — silently dropping all conditional branching. This asserts
            // the read side reconstructs both branches when the edge data is present (as
            // WorkflowToX6Converter now emits it — see WorkflowToX6ConverterCoverageTests.
            // Convert_Flowchart_Decision_TrueAndFalse for the write-side assertion).
            var startId = Guid.NewGuid().ToString();
            var decisionId = Guid.NewGuid().ToString();
            var trueId = Guid.NewGuid().ToString();
            var falseId = Guid.NewGuid().ToString();

            var start = MakeStartNode();
            start.id = startId;

            var decision = new Cell
            {
                id = decisionId,
                data = new Dictionary<string, object>
                {
                    ["type"] = Constants.FLOWDECISION,
                    [Constants.DISPLAYNAME] = "Is x == 42",
                    [Constants.DISPLAYTEXT] = "Is x == 42",
                    [Constants.EXPRESSION] = "[[x]] = 42",
                    [Constants.PROPERTY_UNIQUEID] = decisionId
                }
            };

            var trueBranch = MakeNode("DsfDotNetMultiAssignActivity", "OnTrue",
                new Dictionary<string, object>
                {
                    ["fields"] = JsonConvert.SerializeObject(new[] { new { FieldName = "[[r]]", FieldValue = "true-branch" } })
                });
            trueBranch.id = trueId;

            var falseBranch = MakeNode("DsfDotNetMultiAssignActivity", "OnFalse",
                new Dictionary<string, object>
                {
                    ["fields"] = JsonConvert.SerializeObject(new[] { new { FieldName = "[[r]]", FieldValue = "false-branch" } })
                });
            falseBranch.id = falseId;

            var edgeToDecision = new Cell
            {
                id = Guid.NewGuid().ToString(),
                shape = "edge",
                Source = new Connector(startId),
                Target = new Connector(decisionId)
            };
            var edgeTrue = new Cell
            {
                id = Guid.NewGuid().ToString(),
                shape = "edge",
                label = "true",
                Source = new Connector(decisionId),
                Target = new Connector(trueId),
                data = new Dictionary<string, object> { [Constants.ISDECISIONARM] = true, [Constants.ISTRUEARM] = true }
            };
            var edgeFalse = new Cell
            {
                id = Guid.NewGuid().ToString(),
                shape = "edge",
                label = "false",
                Source = new Connector(decisionId),
                Target = new Connector(falseId),
                data = new Dictionary<string, object> { [Constants.ISDECISIONARM] = true, [Constants.ISTRUEARM] = false }
            };

            var result = Convert("DecisionBranchFlow", start, decision, trueBranch, falseBranch,
                edgeToDecision, edgeTrue, edgeFalse);
            var xaml = result.ToString();

            Assert.IsTrue(xaml.Contains("FlowDecision.True"), "Decision should have its True branch wired.\n" + xaml);
            Assert.IsTrue(xaml.Contains("FlowDecision.False"), "Decision should have its False branch wired.\n" + xaml);
            Assert.IsTrue(xaml.Contains("OnTrue"), "The True branch's target activity should be present in the XAML.\n" + xaml);
            Assert.IsTrue(xaml.Contains("OnFalse"), "The False branch's target activity should be present in the XAML.");
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_MalformedJson_Throws()
        {
            // The converter's outer try/catch logs and rethrows on parse failure.
            // We assert it surfaces a real exception (rather than silently returning empty)
            // because the calling pipeline relies on this to flag bad input early.
            var converter = new X6ToWorkflowConverter();
            Assert.ThrowsException<JsonReaderException>(
                () => converter.X6JsonToWorkflow("{this is not valid json"));
        }

        // ─────────────────────────────────────────────────────────────────
        // X6JsonToXaml — static dictionary entry point
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToXaml_NoResourceJsonKey_ReturnsEmpty()
        {
            var result = X6ToWorkflowConverter.X6JsonToXaml(
                new Dictionary<string, StringBuilder>());
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length,
                "Missing ResourceJSON key should yield an empty StringBuilder, not null");
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToXaml_EmptyResourceJsonValue_ReturnsEmpty()
        {
            var result = X6ToWorkflowConverter.X6JsonToXaml(
                new Dictionary<string, StringBuilder>
                {
                    ["ResourceJSON"] = new StringBuilder()
                });
            Assert.AreEqual(0, result.Length,
                "Empty ResourceJSON value should yield an empty StringBuilder");
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToXaml_WithMinimalGraph_ProducesXamlAndAppliesNamespaceFix()
        {
            var json = Serialise("MiniWorkflow", MakeStartNode());

            var result = X6ToWorkflowConverter.X6JsonToXaml(
                new Dictionary<string, StringBuilder>
                {
                    ["ResourceJSON"] = new StringBuilder(json)
                });

            Assert.IsTrue(result.Length > 0, "Should produce non-empty XAML");
            var xaml = result.ToString();
            Assert.IsFalse(xaml.Contains("System.Private.CoreLib"),
                "AddReplaceNameSpace post-processing should have rewritten System.Private.CoreLib references");
        }

        // ─────────────────────────────────────────────────────────────────
        // Static helpers — pure XML transforms
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void ReplaceSystemNamespace_RewritesCoreLibPrefix()
        {
            var xml =
                "<root xmlns:s=\"clr-namespace:System;assembly=System.Private.CoreLib\">" +
                "  <s:String>hello</s:String>" +
                "</root>";

            var result = X6ToWorkflowConverter.ReplaceSystemNamespace(xml);

            Assert.IsFalse(result.Contains("System.Private.CoreLib"),
                "Bad System.Private.CoreLib namespace should be rewritten to mscorlib");
            Assert.IsTrue(result.Contains("clr-namespace:System;assembly=mscorlib"),
                "Result should contain the corrected mscorlib namespace");
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void ReplaceSystemNamespace_NoBadNamespace_LeavesXmlIntact()
        {
            var xml = "<root xmlns:s=\"clr-namespace:System;assembly=mscorlib\"><s:String>hi</s:String></root>";
            var result = X6ToWorkflowConverter.ReplaceSystemNamespace(xml);
            Assert.IsTrue(result.Contains("clr-namespace:System;assembly=mscorlib"),
                "Already-correct namespace should be preserved");
            Assert.IsFalse(result.Contains("System.Private.CoreLib"));
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void ReplaceSystemNamespace_WhitespaceInput_ReturnsInput()
        {
            Assert.AreEqual("", X6ToWorkflowConverter.ReplaceSystemNamespace(""));
            Assert.AreEqual("   ", X6ToWorkflowConverter.ReplaceSystemNamespace("   "));
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void ReplaceBadCollection_RewritesEmptyCollectionElement()
        {
            var xml =
                "<root xmlns:bad=\"clr-namespace:System.Collections.ObjectModel;assembly=System.Private.CoreLib\" " +
                "      xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                "  <bad:Collection x:TypeArguments=\"Activity\" />" +
                "</root>";

            var result = X6ToWorkflowConverter.ReplaceBadCollection(xml);

            // Helper ensures sco (mscorlib) namespace is declared at the root and rewrites
            // the empty <Collection> element away from System.Private.CoreLib.  The original
            // bad xmlns declaration stays on the root (the helper is conservative — it only
            // mutates elements, not root xmlns declarations) so we check what we know it
            // does change: the Collection element no longer lives in the bad namespace.
            Assert.IsTrue(result.Contains("clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"),
                "Result should declare the sco mscorlib namespace");
            var doc = XDocument.Parse(result);
            var badNs = "clr-namespace:System.Collections.ObjectModel;assembly=System.Private.CoreLib";
            var stillBad = doc.Descendants()
                .Any(el => el.Name.LocalName == "Collection" && el.Name.NamespaceName == badNs);
            Assert.IsFalse(stillBad,
                "No <Collection> element should remain in the System.Private.CoreLib namespace");
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void ReplaceBadCollection_WhitespaceInput_ReturnsInput()
        {
            Assert.AreEqual("",   X6ToWorkflowConverter.ReplaceBadCollection(""));
            Assert.AreEqual("  ", X6ToWorkflowConverter.ReplaceBadCollection("  "));
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void ReplaceDefaultNamespace_RewritesElementNamespaces()
        {
            // Build a tree where the bad scg namespace is the *default* on a child element.
            // ReplaceDefaultNamespace walks elements and rewrites those whose Name.Namespace
            // matches the bad scg namespace.  It does not touch root xmlns declarations
            // unless they're a literal default-namespace declaration matching the bad URI,
            // so we assert specifically on the element-namespace rewrite.
            var xml =
                "<root>" +
                "  <List xmlns=\"clr-namespace:System.Collections.Generic;assembly=System.Private.CoreLib\" />" +
                "</root>";
            var root = XElement.Parse(xml);

            X6ToWorkflowConverter.ReplaceDefaultNamespace(root);

            var listEl = root.Descendants().First(e => e.Name.LocalName == "List");
            Assert.AreEqual(
                "clr-namespace:System.Collections.Generic;assembly=mscorlib",
                listEl.Name.NamespaceName,
                "The List element's namespace should have been rewritten to mscorlib");
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void AddReplaceNameSpace_AddsMissingPresentationNamespaces()
        {
            var xml = new StringBuilder(
                "<Activity xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\">" +
                "  <TextExpression.NamespacesForImplementation />" +
                "</Activity>");

            var result = X6ToWorkflowConverter.AddReplaceNameSpace(xml).ToString();

            // The helper guarantees the av and sap prefixes are declared at the root.
            Assert.IsTrue(result.Contains("xmlns:av"),
                "av (winfx presentation) namespace should be ensured at the root");
            Assert.IsTrue(result.Contains("xmlns:sap"),
                "sap (activities presentation) namespace should be ensured at the root");
            Assert.IsTrue(result.Contains("Unlimited.Applications.BusinessDesignStudio.Activities"),
                "NamespacesForImplementation list should have been populated with the default Warewolf namespaces");
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void AddReplaceNameSpace_MalformedXml_ReturnsInputUnchanged()
        {
            // The helper wraps its work in a try/catch and returns the original StringBuilder on failure.
            var input = new StringBuilder("<<not valid xml");
            var result = X6ToWorkflowConverter.AddReplaceNameSpace(input);
            Assert.AreSame(input, result,
                "On parse failure AddReplaceNameSpace must return the original StringBuilder instance");
        }

        // ─────────────────────────────────────────────────────────────────
        // Unconvertible nodes must not vanish
        //
        // CreateActivityFromNode returns null when a node cannot be turned into an activity.
        // The caller used to skip such nodes in silence, so conversion "succeeded" while quietly
        // producing a workflow with a step missing — invisible until the workflow ran and behaved
        // differently. That is the exact failure the round-trip fidelity gate exists to prevent.
        // ─────────────────────────────────────────────────────────────────

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_NodeWithBlankDisplayName_ThrowsRatherThanDroppingTheStep()
        {
            // A blank display name makes the Create* helper return null. Conversion must fail
            // loudly rather than return a workflow that is silently one step short.
            var unconvertible = MakeNode("dsfdotnetmultiassignactivity", displayName: "   ");

            var ex = Assert.ThrowsException<InvalidOperationException>(() =>
                Convert("DropsAStep", MakeStartNode(), unconvertible));

            StringAssert.Contains(ex.Message, unconvertible.id,
                "The error must name the offending node so the caller can find it.");
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_DisplayNameThatSurvivedJsonAsNonString_StillResolves()
        {
            // Cell.data is Dictionary<string, object>. After a serialize/deserialize cycle a value
            // arrives as a Newtonsoft JValue, not a string, so the old "is not string" guard
            // rejected a perfectly good display name and dropped the activity. TryGetString
            // tolerates it, matching what the FromX6Json implementations already did.
            var node = MakeNode("dsfdotnetmultiassignactivity");
            node.data[Constants.DISPLAYNAME] = new JValue("Assign");

            var xaml = Convert("NonStringDisplayName", MakeStartNode(), node);

            Assert.IsTrue(xaml.Length > 0, "A JValue display name must not cause the activity to be dropped.");
        }

    }
}
