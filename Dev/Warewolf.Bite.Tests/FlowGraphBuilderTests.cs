/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Linq;
using Dev2.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Warewolf.Bite.Tests
{
    /// <summary>Control flow, matching ActivityParser (quirks included) - WF removal T1.2.</summary>
    [TestClass]
    public class FlowGraphBuilderTests
    {
        const string A = "aaaaaaaa-0000-0000-0000-000000000001";
        const string B = "aaaaaaaa-0000-0000-0000-000000000002";
        const string C = "aaaaaaaa-0000-0000-0000-000000000003";
        const string D = "aaaaaaaa-0000-0000-0000-000000000004";

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_StartIsStep_ReturnsActivityLinkedToNext()
        {
            var start = Xaml.Read(Xaml.Workflow(
                $"<Flowchart.StartNode><FlowStep>{Xaml.Comment(A)}<FlowStep.Next><FlowStep>{Xaml.Comment(B)}</FlowStep></FlowStep.Next></FlowStep></Flowchart.StartNode>"));

            Assert.IsInstanceOfType(start, typeof(DsfCommentActivity));
            Assert.AreEqual(A, start!.UniqueID);
            Assert.AreEqual(B, start.NextNodes.Single().UniqueID);
            Assert.IsNull(start.NextNodes.Single().NextNodes, "A step with no Next has null NextNodes, as ActivityParser leaves it.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_StartIsDecision_ReturnsDsfDecisionWithArmsAndConditions()
        {
            var start = Xaml.Read(Xaml.Workflow(
                $"<Flowchart.StartNode><FlowDecision><FlowDecision.Condition>{Xaml.Decision(D)}</FlowDecision.Condition>" +
                $"<FlowDecision.True><FlowStep>{Xaml.Comment(A)}</FlowStep></FlowDecision.True>" +
                $"<FlowDecision.False><FlowStep>{Xaml.Comment(B)}</FlowStep></FlowDecision.False></FlowDecision></Flowchart.StartNode>"));

            var decision = (DsfDecision)start!;
            Assert.AreEqual(D, decision.UniqueID);
            Assert.AreEqual(A, decision.TrueArm.Single().UniqueID);
            Assert.AreEqual(B, decision.FalseArm.Single().UniqueID);
            Assert.AreEqual(1, decision.Conditions.TheStack.Count);
            Assert.IsTrue(decision.And);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_StartIsSwitch_ReturnsDsfSwitchWithCasesDefaultAndWfDefaultDisplayName()
        {
            var start = Xaml.Read(Xaml.Workflow(
                "<Flowchart.StartNode><FlowSwitch x:TypeArguments=\"x:String\" Default=\"{x:Reference dflt}\">" +
                $"<FlowSwitch.Expression><uaba:DsfFlowSwitchActivity DisplayName=\"s\" ExpressionText=\"{Xaml.SwitchExpression}\" UniqueID=\"{D}\" /></FlowSwitch.Expression>" +
                $"<FlowStep x:Key=\"1\">{Xaml.Comment(A)}</FlowStep>" +
                $"<FlowStep x:Key=\"2\" x:Name=\"dflt\">{Xaml.Comment(B)}</FlowStep>" +
                "</FlowSwitch></Flowchart.StartNode>"));

            var dsfSwitch = (DsfSwitch)start!;
            Assert.AreEqual("[[a]]", dsfSwitch.Switch);
            Assert.AreEqual("Switch", dsfSwitch.DisplayName, "WF's FlowSwitch.DisplayName default, which ActivityParser copies.");
            Assert.AreEqual(A, dsfSwitch.Switches["1"].UniqueID);
            Assert.AreEqual(B, dsfSwitch.Switches["2"].UniqueID);
            Assert.AreSame(dsfSwitch.Switches["2"], dsfSwitch.Default.Single(), "Default and case 2 are the same x:Reference'd step.");
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_NoStartNode_ReturnsNull()
        {
            Assert.IsNull(Xaml.Read(Xaml.Workflow($"<FlowStep>{Xaml.Comment(A)}</FlowStep>")));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_ImplementationIsNotAFlowchart_ReturnsNull()
        {
            var xaml = Xaml.Workflow("").Replace("<Flowchart></Flowchart>", Xaml.Comment(A));

            Assert.IsNull(Xaml.Read(xaml));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_SharedStepSuccessor_IsTheSameInstance()
        {
            var start = (DsfDecision)Xaml.Read(Xaml.Workflow(
                $"<Flowchart.StartNode><FlowDecision True=\"{{x:Reference shared}}\"><FlowDecision.Condition>{Xaml.Decision(D)}</FlowDecision.Condition>" +
                $"<FlowDecision.False><FlowStep x:Name=\"shared\">{Xaml.Comment(A)}</FlowStep></FlowDecision.False></FlowDecision></Flowchart.StartNode>"))!;

            Assert.AreSame(start.TrueArm.Single(), start.FalseArm.Single());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_BackEdgeToStep_Terminates()
        {
            var start = Xaml.Read(Xaml.Workflow(
                $"<Flowchart.StartNode><FlowStep x:Name=\"first\">{Xaml.Comment(A)}<FlowStep.Next><FlowStep>{Xaml.Comment(B)}" +
                "<FlowStep.Next><x:Reference>first</x:Reference></FlowStep.Next></FlowStep></FlowStep.Next></FlowStep></Flowchart.StartNode>"))!;

            Assert.AreSame(start, start.NextNodes.Single().NextNodes.Single());
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_DecisionReachedTwice_IsWrappedTwice_AsActivityParserDoes()
        {
            // ActivityParser's seen-check looks for the inner DsfFlowDecisionActivity but records the DsfDecision wrapper,
            // so a decision reached along two paths becomes two wrappers. Matched for parity; see the class remarks.
            var start = (DsfDecision)Xaml.Read(Xaml.Workflow(
                $"<Flowchart.StartNode><FlowDecision><FlowDecision.Condition>{Xaml.Decision(D)}</FlowDecision.Condition>" +
                $"<FlowDecision.True><FlowStep>{Xaml.Comment(A)}<FlowStep.Next><FlowDecision x:Name=\"inner\"><FlowDecision.Condition>{Xaml.Decision(C)}</FlowDecision.Condition></FlowDecision></FlowStep.Next></FlowStep></FlowDecision.True>" +
                $"<FlowDecision.False><FlowStep>{Xaml.Comment(B)}<FlowStep.Next><x:Reference>inner</x:Reference></FlowStep.Next></FlowStep></FlowDecision.False>" +
                "</FlowDecision></Flowchart.StartNode>"))!;

            var viaTrue = start.TrueArm.Single().NextNodes.Single();
            var viaFalse = start.FalseArm.Single().NextNodes.Single();
            Assert.AreEqual(C, viaTrue.UniqueID);
            Assert.AreEqual(C, viaFalse.UniqueID);
            Assert.AreNotSame(viaTrue, viaFalse);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_DecisionWithMissingArm_LeavesThatArmNull()
        {
            var start = (DsfDecision)Xaml.Read(Xaml.Workflow(
                $"<Flowchart.StartNode><FlowDecision><FlowDecision.Condition>{Xaml.Decision(D)}</FlowDecision.Condition>" +
                $"<FlowDecision.True><FlowStep>{Xaml.Comment(A)}</FlowStep></FlowDecision.True></FlowDecision></Flowchart.StartNode>"))!;

            Assert.IsNotNull(start.TrueArm);
            Assert.IsNull(start.FalseArm);
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_StepWhoseActionIsNotAnActivity_EndsTheChain()
        {
            Assert.IsNull(Xaml.Read(Xaml.Workflow("<Flowchart.StartNode><FlowStep><x:String>not an activity</x:String></FlowStep></Flowchart.StartNode>")));
        }

        [TestMethod]
        [Owner("Ashley Lewis")]
        [TestCategory("BiteReader")]
        public void BiteReader_CycleOfDecisionsOnly_FailsLoudlyInsteadOfOverflowingTheStack()
        {
            var xaml = Xaml.Workflow(
                $"<Flowchart.StartNode><FlowDecision x:Name=\"loop\" True=\"{{x:Reference loop}}\"><FlowDecision.Condition>{Xaml.Decision(D)}</FlowDecision.Condition></FlowDecision></Flowchart.StartNode>");

            var e = Assert.ThrowsException<BiteFormatException>(() => Xaml.Read(xaml));
            StringAssert.Contains(e.Message, "cycle");
        }
    }
}
