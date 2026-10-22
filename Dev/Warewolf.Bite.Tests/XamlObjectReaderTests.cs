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
using System.Text.Json.Nodes;
using Dev2;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Bite.Tests
{
    /// <summary>The XAML subset: values, markup, collections, ignored plumbing, and loud failure - WF removal T1.3, T1.4, T1.6.</summary>
    [TestClass]
    public class XamlObjectReaderTests
    {
        const string A = "aaaaaaaa-0000-0000-0000-000000000001";

        static IDev2Activity ReadStep(string activityXaml, string extraNamespaces = "", string resourceId = "11111111-1111-1111-1111-111111111111") =>
            Xaml.Read(Xaml.Workflow($"<Flowchart.StartNode><FlowStep>{activityXaml}</FlowStep></Flowchart.StartNode>", extraNamespaces), resourceId)!;

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_ConvertsValues_IndependentOfCurrentCulture()
        {
            var original = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = new CultureInfo("de-DE");

                var forEach = (DsfForEachActivity)ReadStep(
                    $"<uaba:DsfForEachActivity DisplayName=\"f\" UniqueID=\"{A}\" NumOfExections=\"6\" ForEachType=\"NumOfExecution\" FailOnFirstError=\"True\" />");

                Assert.AreEqual("6", forEach.NumOfExections);
                Assert.AreEqual(Dev2.Data.Interfaces.Enums.enForEachType.NumOfExecution, forEach.ForEachType);
                Assert.IsTrue(forEach.FailOnFirstError);
                Assert.AreEqual(Guid.Parse(A).ToString(), forEach.UniqueID);
            }
            finally
            {
                CultureInfo.CurrentCulture = original;
            }
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_XNull_AsAttributeAndAsElement_SetsNull()
        {
            var fromAttribute = (DsfCommentActivity)ReadStep(Xaml.Comment(A, extra: "Text=\"{x:Null}\""));
            var fromElement = (DsfCommentActivity)ReadStep(
                $"<uaba:DsfCommentActivity DisplayName=\"c\" UniqueID=\"{A}\"><uaba:DsfCommentActivity.Text><x:Null /></uaba:DsfCommentActivity.Text></uaba:DsfCommentActivity>");

            Assert.IsNull(fromAttribute.Text);
            Assert.IsNull(fromElement.Text);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_XReference_WithAnXKeyChild_UsesOnlyItsOwnTextAsTheName()
        {
            var start = (DsfSwitch)Xaml.Read(Xaml.Workflow(
                "<Flowchart.StartNode><FlowSwitch x:TypeArguments=\"x:String\">" +
                $"<FlowSwitch.Expression><uaba:DsfFlowSwitchActivity DisplayName=\"s\" ExpressionText=\"{Xaml.SwitchExpression}\" UniqueID=\"bbbbbbbb-0000-0000-0000-000000000001\" /></FlowSwitch.Expression>" +
                "<x:Reference>target<x:Key>K</x:Key></x:Reference>" +
                "</FlowSwitch></Flowchart.StartNode>" +
                $"<FlowStep x:Name=\"target\">{Xaml.Comment(A)}</FlowStep>"))!;

            Assert.AreEqual(A, start.Switches["K"].UniqueID, "x:Reference may also point forwards.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_InArgument_PercentEscapedLiteral_DropsThePercent()
        {
            var activity = ReadStep($"<uaba:DsfActivity DisplayName=\"w\" UniqueID=\"{A}\" ActionName=\"%[[Action]]\" />");

            Assert.AreEqual("[[Action]]", Canonical(activity)["ActionName"]!.GetValue<string>());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_InArgument_VbExpression_KeepsItsTextAsAnExpression()
        {
            var activity = ReadStep($"<uaba:DsfActivity DisplayName=\"w\" UniqueID=\"{A}\" ActionName=\"[SomeVariable]\" />");

            Assert.AreEqual("SomeVariable", Canonical(activity)["ActionName"]!["$expr"]!.GetValue<string>());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_BraceEscapedValue_IsTakenLiterally()
        {
            var comment = (DsfCommentActivity)ReadStep(Xaml.Comment(A, extra: "Text=\"{}{not markup}\""));

            Assert.AreEqual("{not markup}", comment.Text);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_EncodedCrLf_IsNormalisedToLf_AsSystemXamlDoes()
        {
            var comment = (DsfCommentActivity)ReadStep(Xaml.Comment(A, extra: "Text=\"one&#xD;&#xA;two&#xD;three\""));

            Assert.AreEqual("one\ntwo\nthree", comment.Text);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_ItemsForAGetOnlyCollection_AreAddedToIt()
        {
            var sequence = (DsfSequenceActivity)ReadStep(
                $"<da:DsfSequenceActivity DisplayName=\"s\" UniqueID=\"{A}\"><da:DsfSequenceActivity.Activities>" +
                Xaml.Comment("cccccccc-0000-0000-0000-000000000001") + Xaml.Comment("cccccccc-0000-0000-0000-000000000002") +
                "</da:DsfSequenceActivity.Activities></da:DsfSequenceActivity>");

            CollectionAssert.AreEqual(
                new[] { "cccccccc-0000-0000-0000-000000000001", "cccccccc-0000-0000-0000-000000000002" },
                sequence.Activities.Cast<IDev2Activity>().Select(a => a.UniqueID).ToArray());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_GenericListWithTypeArguments_IsCreatedWithItsItems()
        {
            var assign = (DsfMultiAssignActivity)ReadStep(
                $"<uaba:DsfMultiAssignActivity DisplayName=\"a\" UniqueID=\"{A}\"><uaba:DsfMultiAssignActivity.FieldsCollection>" +
                "<scg:List x:TypeArguments=\"uaba:ActivityDTO\" Capacity=\"4\">" +
                "<uaba:ActivityDTO FieldName=\"[[x]]\" FieldValue=\"1\" IndexNumber=\"1\" />" +
                "<uaba:ActivityDTO FieldName=\"[[y]]\" FieldValue=\"2\" IndexNumber=\"2\" />" +
                "</scg:List></uaba:DsfMultiAssignActivity.FieldsCollection></uaba:DsfMultiAssignActivity>");

            CollectionAssert.AreEqual(new[] { "[[x]]", "[[y]]" }, assign.FieldsCollection.Select(f => f.FieldName).ToArray());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_DesignerAndWfPlumbing_IsIgnored()
        {
            var xaml =
                "<Activity x:Class=\"Test\" xmlns=\"http://schemas.microsoft.com/netfx/2009/xaml/activities\" xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\" " +
                "xmlns:sap=\"http://schemas.microsoft.com/netfx/2009/xaml/activities/presentation\" xmlns:mva=\"clr-namespace:Microsoft.VisualBasic.Activities;assembly=System.Activities\" " +
                "xmlns:scg=\"clr-namespace:System.Collections.Generic;assembly=mscorlib\" xmlns:uaba=\"clr-namespace:Unlimited.Applications.BusinessDesignStudio.Activities;assembly=Dev2.Activities\">" +
                "<x:Members><x:Property Name=\"AmbientDataList\" Type=\"InOutArgument(scg:List(x:String))\" /></x:Members>" +
                "<sap:VirtualizedContainerService.HintSize>1,1</sap:VirtualizedContainerService.HintSize>" +
                "<mva:VisualBasic.Settings>ignored</mva:VisualBasic.Settings>" +
                "<TextExpression.NamespacesForImplementation><scg:List x:TypeArguments=\"x:String\"><x:String>Dev2.Common</x:String></scg:List></TextExpression.NamespacesForImplementation>" +
                "<Flowchart><Flowchart.Variables><Variable x:TypeArguments=\"x:Boolean\" Name=\"HasError\" /></Flowchart.Variables>" +
                "<Flowchart.StartNode><FlowStep><sap:WorkflowViewStateService.ViewState>ignored</sap:WorkflowViewStateService.ViewState>" +
                $"<uaba:DsfCommentActivity DisplayName=\"c\" UniqueID=\"{A}\" HasError=\"[HasError]\" InstructionList=\"[InstructionList]\" IsValid=\"[IsValid]\" sap:VirtualizedContainerService.HintSize=\"1,1\" />" +
                "</FlowStep></Flowchart.StartNode></Flowchart></Activity>";

            var start = Xaml.Read(xaml);

            Assert.AreEqual(A, start!.UniqueID);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_UnknownMember_FailsWithElementAndLine()
        {
            var e = Assert.ThrowsException<BiteFormatException>(() => ReadStep(Xaml.Comment(A, extra: "NoSuchMember=\"x\"")));

            StringAssert.Contains(e.Message, "NoSuchMember");
            Assert.IsNotNull(e.Line, "The error must carry the line it happened on.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_TypeOutsideTheAllowlist_IsRejectedNotInstantiated()
        {
            var e = Assert.ThrowsException<BiteFormatException>(() =>
                ReadStep("<sd:Process />", "xmlns:sd=\"clr-namespace:System.Diagnostics;assembly=System\""));

            StringAssert.Contains(e.Message, "not allowed");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_DuplicateXName_Fails()
        {
            var xaml = Xaml.Workflow(
                $"<Flowchart.StartNode><FlowStep x:Name=\"same\">{Xaml.Comment(A)}</FlowStep></Flowchart.StartNode>" +
                $"<FlowStep x:Name=\"same\">{Xaml.Comment("aaaaaaaa-0000-0000-0000-000000000009")}</FlowStep>");

            var e = Assert.ThrowsException<BiteFormatException>(() => Xaml.Read(xaml));
            StringAssert.Contains(e.Message, "Duplicate x:Name");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_ActivityWithoutStoredUniqueId_GetsAStableIdPerResource()
        {
            const string unsaved = "<uaba:DsfCommentActivity DisplayName=\"c\" />";

            var first = ReadStep(unsaved, resourceId: "resource-1").UniqueID;
            var again = ReadStep(unsaved, resourceId: "resource-1").UniqueID;
            var otherResource = ReadStep(unsaved, resourceId: "resource-2").UniqueID;

            Assert.AreEqual(first, again, "Same resource and position must give the same id on every load.");
            Assert.AreNotEqual(first, otherResource);
            Assert.IsTrue(Guid.TryParse(first, out _));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_StoredUniqueId_IsKept()
        {
            Assert.AreEqual(A, ReadStep(Xaml.Comment(A), resourceId: "resource-1").UniqueID);
        }

        static JsonNode Canonical(IDev2Activity activity) =>
            ((JsonObject)ActivityGraphCanonicalizer.Canonicalize(activity)["nodes"]!).First().Value!;
    }
}
