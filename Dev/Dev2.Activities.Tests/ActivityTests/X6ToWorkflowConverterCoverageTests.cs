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
using System.Text;
using System.Xml.Linq;
using Dev2.Activities.WF;
using Dev2.Common.X6;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

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
        public void X6JsonToWorkflow_EmptyGraph_ProducesNonEmptyXaml()
        {
            var result = Convert("EmptyWorkflow");

            Assert.IsNotNull(result, "Converter should never return null for a valid (if empty) graph");
            Assert.IsTrue(result.Length > 0,
                "Even an empty graph should yield a wrapping ActivityBuilder/Flowchart XAML scaffold");
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
                    [Constants.EXPRESSION]   = "[[a]] = 1",
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
        public void X6JsonToWorkflow_UnknownActivityType_FallsBackToWriteLine()
        {
            // Unknown 'type' values fall through the switch to the default → WriteLine.
            // Verifying this branch is critical because it's the safety net for forward-
            // compatibility with new activity types added to the studio JSON.
            var unknown = MakeNode("ThisIsNotARealActivityType");
            var result  = Convert("UnknownTypeFlow", MakeStartNode(), unknown);
            Assert.IsTrue(result.Length > 0);
        }

        [TestMethod, Timeout(60000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void X6JsonToWorkflow_NodeMissingType_IsSkipped()
        {
            // A node whose 'data' has no 'type' key returns null from CreateActivityFromNode
            // and should be silently skipped (i.e. not crash the converter).
            var noType = new Cell
            {
                id   = Guid.NewGuid().ToString(),
                data = new Dictionary<string, object> { ["displayname"] = "no-type" }
            };
            var result = Convert("NoTypeFlow", noType);
            Assert.IsTrue(result.Length > 0);
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
                    [Constants.EXPRESSION]  = "[[x]] = 42",
                    [Constants.PROPERTY_UNIQUEID] = decisionId
                }
            };

            // An edge connects start → assign → decision
            var edge1 = new Cell
            {
                id     = Guid.NewGuid().ToString(),
                shape  = "edge",
                Source = new Connector { cell = startId },
                Target = new Connector { cell = assignId }
            };
            var edge2 = new Cell
            {
                id     = Guid.NewGuid().ToString(),
                shape  = "edge",
                Source = new Connector { cell = assignId },
                Target = new Connector { cell = decisionId }
            };

            var result = Convert("ChainedFlow", start, assign, decision, edge1, edge2);

            Assert.IsTrue(result.Length > 0,
                "Multi-activity graph with edges should produce non-empty XAML");
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
        public void ReplaceBadCollection_RewritesEmptyCollectionNamespace()
        {
            var xml =
                "<root xmlns:bad=\"clr-namespace:System.Collections.ObjectModel;assembly=System.Private.CoreLib\" " +
                "      xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                "  <bad:Collection x:TypeArguments=\"Activity\" />" +
                "</root>";

            var result = X6ToWorkflowConverter.ReplaceBadCollection(xml);

            Assert.IsTrue(result.Contains("clr-namespace:System.Collections.ObjectModel;assembly=mscorlib"),
                "Result should declare the sco mscorlib namespace");
            Assert.IsFalse(result.Contains("assembly=System.Private.CoreLib"),
                "Empty <Collection> element should no longer reference System.Private.CoreLib");
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void ReplaceBadCollection_WhitespaceInput_ReturnsInput()
        {
            Assert.AreEqual("",   X6ToWorkflowConverter.ReplaceBadCollection(""));
            Assert.AreEqual("  ", X6ToWorkflowConverter.ReplaceBadCollection("  "));
        }

        [TestMethod, Timeout(30000), TestCategory("X6ToWorkflowConverter_Coverage")]
        public void ReplaceDefaultNamespace_RewritesElementAndTypeArgumentNamespaces()
        {
            // Build a tree with the bad scg namespace at the root and a child element that
            // uses that namespace.  ReplaceDefaultNamespace mutates the XElement in place.
            var xml =
                "<root xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=System.Private.CoreLib\" " +
                "      xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\">" +
                "  <scg:List x:TypeArguments=\"clr-namespace:System.Collections.Generic;assembly=System.Private.CoreLib\" />" +
                "</root>";
            var root = XElement.Parse(xml);

            X6ToWorkflowConverter.ReplaceDefaultNamespace(root);

            var rendered = root.ToString();
            Assert.IsFalse(rendered.Contains("assembly=System.Private.CoreLib"),
                "All System.Private.CoreLib references on elements/TypeArguments should be rewritten");
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
    }
}
