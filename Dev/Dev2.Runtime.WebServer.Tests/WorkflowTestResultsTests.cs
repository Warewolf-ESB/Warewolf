/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Common.Interfaces;
using Dev2.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Dev2.Runtime.WebServer.Tests
{
    [TestClass]
    public class WorkflowTestResultsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowTestResults))]
        public void WorkflowTestResults_Add_TestInvalid_When_TestStep_IsNull()
        {
            var invalid_test = new ServiceTestModelTO 
            {
                TestName = "Invalid test",
            };
            var sut = new WorkflowTestResults();

            sut.Add(invalid_test);

            var result = sut.Results;

            Assert.IsTrue(result.First().TestInvalid);
            Assert.AreEqual(RunResult.TestInvalid, result.First().Result.RunTestResult);
            Assert.AreEqual("Test has no selected nodes", result.First().Result.Message);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowTestResults))]
        public void WorkflowTestResults_Add_TestInvalid_When_TestStep_CountIsZero()
        {
            var invalid_test = new ServiceTestModelTO
            {
                TestName = "Invalid test",
                TestSteps = new List<IServiceTestStep>()
            };

            var sut = new WorkflowTestResults();

            sut.Add(invalid_test);

            var result = sut.Results;

            Assert.IsTrue(result.First().TestInvalid);
            Assert.AreEqual(RunResult.TestInvalid, result.First().Result.RunTestResult);
            Assert.AreEqual("Test has no selected nodes", result.First().Result.Message);

        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowTestResults))]
        public void WorkflowTestResults_Add_Given_ValidTest_And_TestFailing_ExpectFailingTest()
        {
            var invalid_test = new ServiceTestModelTO
            {
                TestName = "Failing test",
                TestFailing = true,
                FailureMessage = "test failed test message",
                TestSteps = new List<IServiceTestStep> 
                {
                   new ServiceTestStepTO()
                }
            };

            var sut = new WorkflowTestResults();

            sut.Add(invalid_test);

            var result = sut.Results;

            var failingTest = result.First();
            Assert.IsTrue(failingTest.TestFailing);
            Assert.IsFalse(failingTest.TestInvalid);
            Assert.AreNotEqual(RunResult.TestInvalid, failingTest.Result.RunTestResult);
            Assert.AreEqual("test failed test message", failingTest.Result.Message);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WorkflowTestResults))]
        public void WorkflowTestResults_Add_Given_ValidTest_And_TestPassing_ExpectPassingTest()
        {
            var invalid_test = new ServiceTestModelTO
            {
                TestName = "Passing test",
                TestPassed = true,
                TestSteps = new List<IServiceTestStep>
                {
                   new ServiceTestStepTO()
                }
            };

            var sut = new WorkflowTestResults();

            sut.Add(invalid_test);

            var result = sut.Results;

            var failingTest = result.First();
            Assert.IsTrue(failingTest.TestPassed);
            Assert.IsFalse(failingTest.TestFailing);
            Assert.IsFalse(failingTest.TestInvalid);
            Assert.AreEqual(RunResult.TestPassed, failingTest.Result.RunTestResult);
            Assert.AreEqual(string.Empty, failingTest.Result.Message);

        }
    }
}
