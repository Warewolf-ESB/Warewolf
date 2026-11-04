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
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text.Json.Nodes;
using Dev2;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Bite.Json;

namespace Warewolf.Bite.Tests
{
    /// <summary>The version 1 JSON body: detection, reading, writing - WF removal T1.5.</summary>
    [TestClass]
    public class BiteJsonTests
    {
        const string A = "aaaaaaaa-0000-0000-0000-000000000001";
        const string B = "aaaaaaaa-0000-0000-0000-000000000002";
        const string Comment = "Unlimited.Applications.BusinessDesignStudio.Activities.DsfCommentActivity";

        static string Resource(string body, string element = "BiteDefinition", string version = "1") =>
            $"<Service ID=\"{Guid.Empty}\" Name=\"t\"><Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
            $"<{element}{(element == "BiteDefinition" ? $" FormatVersion=\"{version}\"" : "")}>{WebUtility.HtmlEncode(body)}</{element}></Action></Service>";

        static string Json(string nodes, string start = "\"n0\"") => $"{{\"start\": {start}, \"nodes\": {{ {nodes} }} }}";

        static string Step(string id, string activity, string next = "null") =>
            $"\"{id}\": {{\"kind\": \"step\", \"activity\": {activity}, \"next\": {next}}}";

        static string CommentActivity(string uniqueId, string text = "hi") =>
            $"{{\"$type\": \"{Comment}\", \"UniqueID\": \"{uniqueId}\", \"DisplayName\": \"c\", \"Text\": \"{text}\"}}";

        static IDev2Activity? ReadJson(string json) => new BiteReader().Read(Resource(json));

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_BiteDefinition_TakesPrecedenceOverXamlDefinition()
        {
            var resource = Resource(Json(Step("n0", CommentActivity(A, "from json"))))
                .Replace("</Action>", $"<XamlDefinition>{WebUtility.HtmlEncode(Xaml.Workflow($"<Flowchart.StartNode><FlowStep>{Xaml.Comment(B)}</FlowStep></Flowchart.StartNode>"))}</XamlDefinition></Action>");

            var start = (DsfCommentActivity)new BiteReader().Read(resource)!;

            Assert.AreEqual("from json", start.Text);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_UnknownFormatVersion_FailsLoudly()
        {
            var e = Assert.ThrowsException<BiteFormatException>(() =>
                new BiteReader().Read(Resource(Json(Step("n0", CommentActivity(A))), version: "2")));

            StringAssert.Contains(e.Message, "FormatVersion '2'");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_TypeOutsideTheAllowlist_IsRejected()
        {
            var e = Assert.ThrowsException<BiteFormatException>(() =>
                ReadJson(Json(Step("n0", "{\"$type\": \"System.Diagnostics.Process\"}"))));

            StringAssert.Contains(e.Message, "not allowed");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_SuccessorThatIsNotANode_FailsNamingThePath()
        {
            var e = Assert.ThrowsException<BiteFormatException>(() =>
                ReadJson(Json(Step("n0", CommentActivity(A), next: "\"missing\""))));

            StringAssert.Contains(e.Message, "$.nodes.n0.next");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_NoStart_ReturnsNull()
        {
            Assert.IsNull(ReadJson(Json(Step("n0", CommentActivity(A)), start: "null")));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_DecisionAndSwitch_RoundTripWithTheirArmsAndCases()
        {
            var xaml = Xaml.Workflow(
                $"<Flowchart.StartNode><FlowDecision><FlowDecision.Condition>{Xaml.Decision("dddddddd-0000-0000-0000-000000000001")}</FlowDecision.Condition>" +
                "<FlowDecision.True><FlowSwitch x:TypeArguments=\"x:String\">" +
                $"<FlowSwitch.Expression><uaba:DsfFlowSwitchActivity DisplayName=\"s\" ExpressionText=\"{Xaml.SwitchExpression}\" UniqueID=\"dddddddd-0000-0000-0000-000000000002\" /></FlowSwitch.Expression>" +
                $"<FlowStep x:Key=\"1\">{Xaml.Comment(A)}</FlowStep></FlowSwitch></FlowDecision.True>" +
                $"<FlowDecision.False><FlowStep>{Xaml.Comment(B)}</FlowStep></FlowDecision.False></FlowDecision></Flowchart.StartNode>");

            var json = BiteJsonWriter.FromXaml(xaml, "r");
            var decision = (DsfDecision)new BiteReader().ReadJson(json, "r")!;

            var kinds = ((JsonObject)JsonNode.Parse(json)!["nodes"]!).Select(n => n.Value!["kind"]!.GetValue<string>()).ToList();
            CollectionAssert.AreEquivalent(new[] { "decision", "switch", "step", "step" }, kinds);
            Assert.AreEqual(A, ((DsfSwitch)decision.TrueArm.Single()).Switches["1"].UniqueID);
            Assert.AreEqual(B, decision.FalseArm.Single().UniqueID);
            Assert.AreEqual(0, ActivityGraphDiff.Compare(Xaml.Read(xaml, "r"), decision).Count);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_InArgument_LiteralIsAPlainValue_ExpressionIsExpr()
        {
            var literal = BiteJsonWriter.FromXaml(Xaml.Workflow($"<Flowchart.StartNode><FlowStep><uaba:DsfActivity DisplayName=\"w\" UniqueID=\"{A}\" ActionName=\"%[[Action]]\" HelpLink=\"[SomeVariable]\" /></FlowStep></Flowchart.StartNode>"), "r");

            var activity = JsonNode.Parse(literal)!["nodes"]!.AsObject().First().Value!["activity"]!;
            Assert.AreEqual("[[Action]]", activity["ActionName"]!.GetValue<string>());
            Assert.AreEqual("SomeVariable", activity["HelpLink"]!["$expr"]!.GetValue<string>());

            var read = new BiteReader().ReadJson(literal, "r")!;
            var canonical = ActivityGraphCanonicalizer.Canonicalize(read)["nodes"]!.AsObject().First().Value!;
            Assert.AreEqual("[[Action]]", canonical["ActionName"]!.GetValue<string>());
            Assert.AreEqual("SomeVariable", canonical["HelpLink"]!["$expr"]!.GetValue<string>());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_ForEachBody_IsInlineWithItsArgument()
        {
            var xaml = Xaml.Workflow(
                $"<Flowchart.StartNode><FlowStep><uaba:DsfForEachActivity DisplayName=\"f\" UniqueID=\"{A}\" NumOfExections=\"2\" ForEachType=\"NumOfExecution\">" +
                "<uaba:DsfForEachActivity.DataFunc><ActivityFunc x:TypeArguments=\"x:String, x:Boolean\" DisplayName=\"Data Action\">" +
                "<ActivityFunc.Argument><DelegateInArgument x:TypeArguments=\"x:String\" Name=\"explicitData_1\" /></ActivityFunc.Argument>" +
                $"{Xaml.Comment(B)}</ActivityFunc></uaba:DsfForEachActivity.DataFunc></uaba:DsfForEachActivity></FlowStep></Flowchart.StartNode>");

            var json = BiteJsonWriter.FromXaml(xaml, "r");
            var dataFunc = JsonNode.Parse(json)!["nodes"]!.AsObject().First().Value!["activity"]!["DataFunc"]!;
            var forEach = (DsfForEachActivity)new BiteReader().ReadJson(json, "r")!;

            Assert.AreEqual("explicitData_1", dataFunc["argument"]!.GetValue<string>());
            Assert.AreEqual(B, dataFunc["body"]!["UniqueID"]!.GetValue<string>());
            Assert.AreEqual(B, ((IDev2Activity)forEach.DataFunc.Handler).UniqueID);
            Assert.AreEqual("explicitData_1", forEach.DataFunc.Argument.Name);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_SequenceChildren_RoundTripInOrder()
        {
            var xaml = Xaml.Workflow(
                $"<Flowchart.StartNode><FlowStep><da:DsfSequenceActivity DisplayName=\"s\" UniqueID=\"{A}\"><da:DsfSequenceActivity.Activities>" +
                Xaml.Comment("cccccccc-0000-0000-0000-000000000001") + Xaml.Comment("cccccccc-0000-0000-0000-000000000002") +
                "</da:DsfSequenceActivity.Activities></da:DsfSequenceActivity></FlowStep></Flowchart.StartNode>");

            var sequence = (DsfSequenceActivity)new BiteReader().ReadJson(BiteJsonWriter.FromXaml(xaml, "r"), "r")!;

            CollectionAssert.AreEqual(
                new[] { "cccccccc-0000-0000-0000-000000000001", "cccccccc-0000-0000-0000-000000000002" },
                sequence.Activities.Cast<IDev2Activity>().Select(a => a.UniqueID).ToArray());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_WriteAndRead_AreIndependentOfCurrentCulture()
        {
            var xaml = Xaml.Workflow($"<Flowchart.StartNode><FlowStep><uaba:DsfForEachActivity DisplayName=\"f\" UniqueID=\"{A}\" NumOfExections=\"6\" ForEachType=\"NumOfExecution\" FailOnFirstError=\"True\" /></FlowStep></Flowchart.StartNode>");
            var original = CultureInfo.CurrentCulture;
            string invariant, german;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;
                invariant = BiteJsonWriter.FromXaml(xaml, "r");
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");
                german = BiteJsonWriter.FromXaml(xaml, "r");
                Assert.AreEqual(0, ActivityGraphDiff.Compare(Xaml.Read(xaml, "r"), new BiteReader().ReadJson(german, "r")).Count);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }

            Assert.AreEqual(invariant, german);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_Writer_IsDeterministic()
        {
            var xaml = Xaml.Workflow($"<Flowchart.StartNode><FlowStep>{Xaml.Comment(A)}<FlowStep.Next><FlowStep>{Xaml.Comment(B)}</FlowStep></FlowStep.Next></FlowStep></Flowchart.StartNode>");

            Assert.AreEqual(BiteJsonWriter.FromXaml(xaml, "r"), BiteJsonWriter.FromXaml(xaml, "r"));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_UnreachableNodes_AreKept()
        {
            var xaml = Xaml.Workflow($"<Flowchart.StartNode><FlowStep>{Xaml.Comment(A)}</FlowStep></Flowchart.StartNode><FlowStep>{Xaml.Comment(B)}</FlowStep>");

            var nodes = JsonNode.Parse(BiteJsonWriter.FromXaml(xaml, "r"))!["nodes"]!.AsObject();

            Assert.AreEqual(2, nodes.Count, "Designer content not reachable from the start must survive a save.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_ActivityWithoutUniqueId_GetsAStableIdPerResource()
        {
            var json = Json(Step("n0", $"{{\"$type\": \"{Comment}\", \"DisplayName\": \"c\"}}"));

            var first = new BiteReader().ReadJson(json, "resource-1")!.UniqueID;
            var again = new BiteReader().ReadJson(json, "resource-1")!.UniqueID;
            var other = new BiteReader().ReadJson(json, "resource-2")!.UniqueID;

            Assert.AreEqual(first, again);
            Assert.AreNotEqual(first, other);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteJson")]
        public void BiteJson_ConvertResource_ReplacesOnlyTheBody()
        {
            var bite = $"<Service ID=\"{Guid.Empty}\" Name=\"t\"><DisplayName>t</DisplayName><Action Name=\"InvokeWorkflow\" Type=\"Workflow\">" +
                       $"<XamlDefinition>{WebUtility.HtmlEncode(Xaml.Workflow($"<Flowchart.StartNode><FlowStep>{Xaml.Comment(A)}</FlowStep></Flowchart.StartNode>"))}</XamlDefinition></Action></Service>";

            var converted = BiteJsonWriter.ConvertResource(bite);

            StringAssert.Contains(converted, "<DisplayName>t</DisplayName>");
            StringAssert.Contains(converted, "<BiteDefinition FormatVersion=\"1\">");
            Assert.IsFalse(converted.Contains("XamlDefinition"));
            Assert.AreEqual(A, new BiteReader().Read(converted)!.UniqueID);
        }
    }
}
