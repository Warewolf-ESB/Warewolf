/*
 *  Warewolf - Once bitten, there's no going back
 *  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
 *  Licensed under GNU Affero General Public License 3.0 or later.
 *
 *  Unit tests for TestOutputEvaluator: the top-level test.Outputs / NoErrorExpected /
 *  ErrorExpected / ErrorContainsText evaluation that has no activity to hook (per-step
 *  testSteps[] assertions are already handled by shared Dev2.Activities code and are not
 *  this type's concern). Covers a representative spread of AssertOp operators ("=", "Contains",
 *  "Is Between") and the error-expectation cases, including the canonical
 *  [actual, From, To] operand order decision (docs/WorkflowTestFramework-Plan.md decision 5).
 */

using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Warewolf.Execution.Lightweight.Tests.Execution
{
    [TestClass]
    public class TestOutputEvaluatorTests
    {
        static IDSFDataObject NewDataObject() => new DsfDataObject(string.Empty, Guid.NewGuid());

        static ServiceTestModelTO NewTest(List<IServiceTestOutput> outputs = null, bool noErrorExpected = false, bool errorExpected = false, string errorContainsText = null) =>
            new()
            {
                TestName = "T",
                Outputs = outputs ?? new List<IServiceTestOutput>(),
                NoErrorExpected = noErrorExpected,
                ErrorExpected = errorExpected,
                ErrorContainsText = errorContainsText,
            };

        static ServiceTestOutputTO Output(string variable, string value, string assertOp = "=", string from = null, string to = null) =>
            new() { Variable = variable, Value = value, AssertOp = assertOp, From = from, To = to };

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_NoOutputsNoErrorFlags_Passes()
        {
            var dataObject = NewDataObject();
            var test = NewTest();

            var (passed, message) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsTrue(passed);
            Assert.AreEqual(string.Empty, message);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_EqualsOperator_MatchingValue_Passes()
        {
            var dataObject = NewDataObject();
            dataObject.Environment.Assign("[[Result]]", "10", 0);
            var test = NewTest(new List<IServiceTestOutput> { Output("[[Result]]", "10") });

            var (passed, _) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsTrue(passed);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_EqualsOperator_MismatchedValue_Fails()
        {
            var dataObject = NewDataObject();
            dataObject.Environment.Assign("[[Result]]", "10", 0);
            var test = NewTest(new List<IServiceTestOutput> { Output("[[Result]]", "11") });

            var (passed, message) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsFalse(passed);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_ContainsOperator_Passes()
        {
            var dataObject = NewDataObject();
            dataObject.Environment.Assign("[[Greeting]]", "hello world", 0);
            var test = NewTest(new List<IServiceTestOutput> { Output("[[Greeting]]", "world", "Contains") });

            var (passed, _) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsTrue(passed);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_IsBetweenOperator_ActualWithinFromTo_Passes()
        {
            // Canonical operand order is [actual, From, To] (Evaluator.cs's order) — regression
            // guard against accidentally adopting ServiceTestHelper's reversed [actual, To, From].
            var dataObject = NewDataObject();
            dataObject.Environment.Assign("[[Age]]", "25", 0);
            var test = NewTest(new List<IServiceTestOutput> { Output("[[Age]]", null, "Is Between", from: "18", to: "65") });

            var (passed, message) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsTrue(passed, message);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_IsBetweenOperator_ActualOutsideFromTo_Fails()
        {
            var dataObject = NewDataObject();
            dataObject.Environment.Assign("[[Age]]", "99", 0);
            var test = NewTest(new List<IServiceTestOutput> { Output("[[Age]]", null, "Is Between", from: "18", to: "65") });

            var (passed, _) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsFalse(passed);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_ErrorExpected_MatchingText_CaseInsensitive_Passes()
        {
            var dataObject = NewDataObject();
            dataObject.Environment.AddError("Something went WRONG here");
            var test = NewTest(errorExpected: true, errorContainsText: "wrong");

            var (passed, _) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsTrue(passed, "ErrorContainsText must match case-insensitively (canonical rule, decision 5).");
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_ErrorExpected_ButNoErrorOccurred_Fails()
        {
            var dataObject = NewDataObject();
            var test = NewTest(errorExpected: true, errorContainsText: "boom");

            var (passed, _) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsFalse(passed);
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_NoErrorExpected_ButErrorOccurred_Fails()
        {
            var dataObject = NewDataObject();
            dataObject.Environment.AddError("unexpected failure");
            var test = NewTest(noErrorExpected: true);

            var (passed, message) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsFalse(passed);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [TestMethod]
        [TestCategory("UnitTest")]
        public void Evaluate_NoErrorExpected_NoErrorOccurred_Passes()
        {
            var dataObject = NewDataObject();
            var test = NewTest(noErrorExpected: true);

            var (passed, _) = TestOutputEvaluator.Evaluate(dataObject, test);

            Assert.IsTrue(passed);
        }
    }
}
