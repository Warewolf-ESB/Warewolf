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
using System.Activities;
using System.Activities.XamlIntegration;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Activities.Flow;
using Microsoft.VisualBasic.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Bite.Xaml;

namespace Warewolf.Bite.Tests
{
    /// <summary>WF-compatible XAML written from the flow model without WF's writer - WF removal T3.2.</summary>
    [TestClass]
    public class BiteXamlWriterTests
    {
        const string A = "bbbbbbbb-0000-0000-0000-000000000001";
        const string B = "bbbbbbbb-0000-0000-0000-000000000002";
        const string C = "bbbbbbbb-0000-0000-0000-000000000003";
        const string ResourceId = "11111111-1111-1111-1111-111111111111";

        static readonly XNamespace Wf = "http://schemas.microsoft.com/netfx/2009/xaml/activities";
        static readonly XNamespace X = "http://schemas.microsoft.com/winfx/2006/xaml";

        static DsfCommentActivity Comment(string uniqueId, string text = "hi") =>
            new DsfCommentActivity { UniqueID = uniqueId, DisplayName = "c", Text = text };

        static FlowchartModel Chart(params FlowNodeModel[] nodes)
        {
            var chart = new FlowchartModel { Name = "Test", StartNode = nodes.FirstOrDefault() };
            chart.Nodes.AddRange(nodes);
            return chart;
        }

        static FlowchartModel ReadModel(string xaml) => new BiteReader().ReadModel(xaml, ResourceId)!;

        // What engines still on WF do with a XamlDefinition.
        static IDev2Activity LoadOnWfPath(string xaml)
        {
            using (var reader = new StringReader(xaml))
            {
                return new ActivityParser().Parse((DynamicActivity)ActivityXamlServices.Load(reader));
            }
        }

        static void AssertSameGraph(IDev2Activity? expected, IDev2Activity? actual)
        {
            var differences = ActivityGraphDiff.Compare(expected, actual);
            Assert.AreEqual(0, differences.Count, string.Join(Environment.NewLine, differences.Take(10)));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_Output_LoadsOnTheWfPathToTheSameGraph()
        {
            var original = Xaml.Workflow(
                "<Flowchart.StartNode><x:Reference>__ReferenceID0</x:Reference></Flowchart.StartNode>" +
                $"<FlowStep x:Name=\"__ReferenceID0\">{Xaml.Comment(A)}<FlowStep.Next><x:Reference>__ReferenceID1</x:Reference></FlowStep.Next></FlowStep>" +
                $"<FlowDecision x:Name=\"__ReferenceID1\"><FlowDecision.Condition>{Xaml.Decision(B)}</FlowDecision.Condition>" +
                "<FlowDecision.True><x:Reference>__ReferenceID2</x:Reference></FlowDecision.True></FlowDecision>" +
                $"<FlowStep x:Name=\"__ReferenceID2\">{Xaml.Comment(C)}</FlowStep>");

            var rewritten = BiteXamlWriter.Write(ReadModel(original));

            AssertSameGraph(Xaml.Read(original), LoadOnWfPath(rewritten));
            AssertSameGraph(Xaml.Read(original), Xaml.Read(rewritten));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_Boilerplate_ImportsAndVariablesWfNeedsArePresent()
        {
            var root = XElement.Parse(BiteXamlWriter.Write(Chart(new FlowStepModel { Action = Comment(A) })));

            Assert.AreEqual("Test", (string?)root.Attribute(X + "Class"));
            var imports = root.Element(Wf + "TextExpression.NamespacesForImplementation")!.Descendants(X + "String").Select(e => e.Value).ToList();
            CollectionAssert.Contains(imports, "Dev2.Common");
            CollectionAssert.Contains(imports, "Unlimited.Applications.BusinessDesignStudio.Activities");
            var references = root.Element(Wf + "TextExpression.ReferencesForImplementation")!.Descendants(Wf + "AssemblyReference").Select(e => e.Value).ToList();
            CollectionAssert.AreEquivalent(new[] { "Dev2.Common", "Dev2.Data", "Dev2.Activities" }, references);
            var variables = root.Element(Wf + "Flowchart")!.Element(Wf + "Flowchart.Variables")!.Elements(Wf + "Variable").Select(v => (string?)v.Attribute("Name")).ToList();
            CollectionAssert.AreEquivalent(new[] { "InstructionList", "LastResult", "HasError", "ExplicitDataList", "IsValid", "t", "Dev2DecisionHandler" }, variables);
            Assert.IsNull(root.Element(X + "Members"), "x:Members is only declared when an expression needs it");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_AmbientDataListExpression_DeclaresTheStandardMembers()
        {
            // As WPF Studio wrote it: AmbientDataList="[AmbientDataList]"
            var activity = Comment(A);
            activity.AmbientDataList = new InOutArgument<List<string>>(new VisualBasicReference<List<string>>("AmbientDataList"));

            var xaml = BiteXamlWriter.Write(Chart(new FlowStepModel { Action = activity }));

            var members = XElement.Parse(xaml).Element(X + "Members")!.Elements(X + "Property").Select(p => (string?)p.Attribute("Name")).ToList();
            CollectionAssert.AreEqual(new[] { "AmbientDataList", "ParentWorkflowInstanceId", "ParentServiceName" }, members);
            LoadOnWfPath(xaml); // [AmbientDataList] compiles only against the declaration
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_ArgumentValues_LiteralsThatLookLikeMarkupOrExpressionsAreEscaped()
        {
            foreach (var literal in new[] { "[[a]]", "%[a]", "%x", "%[a", "{\"json\":1}", "plain" })
            {
                var activity = new DsfActivity { UniqueID = A, DisplayName = "{braces}", HelpLink = new InArgument<string>(literal) };

                var xaml = BiteXamlWriter.Write(Chart(new FlowStepModel { Action = activity }));

                var read = (DsfActivity)Xaml.Read(xaml)!;
                var wf = (DsfActivity)LoadOnWfPath(xaml);
                Assert.AreEqual(literal, ((System.Activities.Expressions.Literal<string>)read.HelpLink.Expression).Value, literal);
                Assert.AreEqual(literal, ((System.Activities.Expressions.Literal<string>)wf.HelpLink.Expression).Value, literal);
                Assert.AreEqual("{braces}", read.DisplayName);
            }
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_EmptyArgument_IsWrittenAsAnArgumentElementWithNoValue()
        {
            var activity = new DsfActivity { UniqueID = A, DisplayName = "sub", HelpLink = new InArgument<string>() };

            var xaml = BiteXamlWriter.Write(Chart(new FlowStepModel { Action = activity }));

            var helpLink = XElement.Parse(xaml).Descendants().Single(e => e.Name.LocalName == "DsfActivity.HelpLink");
            Assert.AreEqual("InArgument", helpLink.Elements().Single().Name.LocalName);
            Assert.IsFalse(helpLink.Elements().Single().HasElements || helpLink.Elements().Single().Value.Length > 0);
            var read = (DsfActivity)Xaml.Read(xaml)!;
            Assert.IsNotNull(read.HelpLink);
            Assert.IsNull(read.HelpLink.Expression);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_SharedSuccessorAndBackEdge_AreWrittenAsReferencesToOneNode()
        {
            var first = new FlowStepModel { Name = "First", Action = Comment(A) };
            var shared = new FlowStepModel { Name = "Shared", Action = Comment(B) };
            var decision = new FlowDecisionModel { Name = "Choose", Condition = ((FlowDecisionModel)ReadModel(Xaml.Workflow(
                $"<Flowchart.StartNode><FlowDecision><FlowDecision.Condition>{Xaml.Decision(C)}</FlowDecision.Condition></FlowDecision></Flowchart.StartNode>")).StartNode!).Condition };
            first.Next = decision;
            decision.True = shared;
            decision.False = shared;
            shared.Next = first; // back-edge

            var xaml = BiteXamlWriter.Write(Chart(first, decision, shared));

            var flowchart = XElement.Parse(xaml).Element(Wf + "Flowchart")!;
            Assert.AreEqual(1, flowchart.Elements(Wf + "FlowStep").Count(e => (string?)e.Attribute(X + "Name") == "Shared"), "the shared node is written once");
            var decisionElement = flowchart.Element(Wf + "FlowDecision")!;
            Assert.AreEqual("Shared", decisionElement.Element(Wf + "FlowDecision.True")!.Element(X + "Reference")!.Value);
            Assert.AreEqual("Shared", decisionElement.Element(Wf + "FlowDecision.False")!.Element(X + "Reference")!.Value);
            var reread = ReadModel(xaml);
            var rereadDecision = (FlowDecisionModel)((FlowStepModel)reread.StartNode!).Next!;
            Assert.AreSame(rereadDecision.True, rereadDecision.False);
            Assert.AreSame(reread.StartNode, ((FlowStepModel)rereadDecision.True!).Next);
            AssertSameGraph(Xaml.Read(xaml), LoadOnWfPath(xaml));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_SwitchCases_AreKeyedReferences()
        {
            var one = new FlowStepModel { Name = "One", Action = Comment(A) };
            var other = new FlowStepModel { Name = "Other", Action = Comment(B) };
            var flowSwitch = new FlowSwitchModel { Name = "Pick", DisplayName = "Pick", Default = other };
            flowSwitch.Cases.Add(new KeyValuePair<string, FlowNodeModel?>("1", one));

            var xaml = BiteXamlWriter.Write(Chart(flowSwitch, one, other));

            var keyed = XElement.Parse(xaml).Descendants(Wf + "FlowSwitch").Single().Elements(X + "Reference").Single();
            Assert.AreEqual("1", keyed.Element(X + "Key")!.Value);
            var reread = (FlowSwitchModel)ReadModel(xaml).StartNode!;
            Assert.AreEqual(A, ((DsfCommentActivity)((FlowStepModel)reread.Cases.Single(c => c.Key == "1").Value!).Action!).UniqueID);
            Assert.AreEqual(B, ((DsfCommentActivity)((FlowStepModel)reread.Default!).Action!).UniqueID);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_ActivityFuncBody_KeepsItsArgumentAndHandler()
        {
            var forEach = new DsfForEachActivity { UniqueID = A, DisplayName = "loop" };
            forEach.DataFunc.Argument = new DelegateInArgument<string>("explicitData_1");
            forEach.DataFunc.Handler = Comment(B, "inside");

            var xaml = BiteXamlWriter.Write(Chart(new FlowStepModel { Action = forEach }));

            var read = (DsfForEachActivity)Xaml.Read(xaml)!;
            Assert.AreEqual("explicitData_1", read.DataFunc.Argument.Name);
            Assert.AreEqual("inside", ((DsfCommentActivity)read.DataFunc.Handler).Text);
            AssertSameGraph(read, LoadOnWfPath(xaml));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_NamespacePrefixes_AreDeclaredOnceOnTheRootFromNamespaceInitials()
        {
            var forEach = new DsfForEachActivity { UniqueID = A, DisplayName = "loop" };
            forEach.DataFunc.Handler = Comment(B);

            var root = XElement.Parse(BiteXamlWriter.Write(Chart(new FlowStepModel { Action = forEach })));

            var declarations = root.Attributes().Where(a => a.IsNamespaceDeclaration).ToList();
            Assert.AreEqual(declarations.Count, declarations.Select(a => a.Value).Distinct().Count(), "each namespace is declared once");
            Assert.AreEqual("clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities",
                (string?)root.Attribute(XNamespace.Xmlns + "uaba"));
            Assert.IsFalse(root.Descendants().SelectMany(e => e.Attributes()).Any(a => a.IsNamespaceDeclaration), "no declarations below the root");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteXamlWriter")]
        public void BiteXamlWriter_Output_IsDeterministicAndCultureIndependent()
        {
            var xaml = Xaml.Workflow(
                $"<Flowchart.StartNode><FlowStep>{Xaml.Comment(A)}<FlowStep.Next><FlowDecision><FlowDecision.Condition>{Xaml.Decision(B)}</FlowDecision.Condition></FlowDecision></FlowStep.Next></FlowStep></Flowchart.StartNode>");
            var invariant = BiteXamlWriter.Write(ReadModel(xaml));

            var culture = Thread.CurrentThread.CurrentCulture;
            try
            {
                Thread.CurrentThread.CurrentCulture = new CultureInfo("de-DE");
                Assert.AreEqual(invariant, BiteXamlWriter.Write(ReadModel(xaml)));
            }
            finally
            {
                Thread.CurrentThread.CurrentCulture = culture;
            }
            Assert.AreEqual(invariant, BiteXamlWriter.Write(ReadModel(xaml)));
        }
    }
}
