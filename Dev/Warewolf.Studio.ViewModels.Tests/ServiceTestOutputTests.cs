using System;
using System.Collections.Generic;
using Dev2.BussinessLogic;
using Dev2.Common.Interfaces;
using Dev2.DataList;
using Dev2.DataList.Contract;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming
// ReSharper disable ObjectCreationAsStatement

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServiceTestOutputTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestOutput_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestOutput_Constructor_WhenNullVariable_ShouldThrowException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new ServiceTestOutput(null, "someValue", "", "");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestOutput_Constructor")]
        public void TestOutput_Constructor_WhenValidParameters_ShouldSetProperties()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            //------------Assert Results-------------------------
            Assert.AreEqual("someVar", input.Variable);
            Assert.AreEqual("someValue", input.Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestOutput_Constructor")]
        public void TestOutput_Variable_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Variable")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Variable = "var";
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestOutput_Constructor")]
        public void TestOutput_Value_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Value")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Value = "val";
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_TestInvalid_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestInvalid")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.TestInvalid = true;
            Assert.IsFalse(input.TestPending);
            Assert.IsFalse(input.TestFailing);
            Assert.IsFalse(input.TestPassed);
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_TestFailing_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestFailing")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.TestFailing = true;
            Assert.IsFalse(input.TestPending);
            Assert.IsFalse(input.TestInvalid);
            Assert.IsFalse(input.TestPassed);
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_TestPassed_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestPassed")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.TestPassed = true;
            Assert.IsFalse(input.TestPending);
            Assert.IsFalse(input.TestInvalid);
            Assert.IsFalse(input.TestFailing);
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_TestPending_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestPending")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.TestPending = true;
            Assert.IsFalse(input.TestInvalid);
            Assert.IsFalse(input.TestPassed);
            Assert.IsFalse(input.TestFailing);
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_Result_WhenSet_ShouldFirePropertyChange_TestPassed()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Result")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Result = new TestRunResult() { RunTestResult = RunResult.TestPassed };

            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
            Assert.IsFalse(input.TestInvalid);
            Assert.IsFalse(input.TestPending);
            Assert.IsFalse(input.TestFailing);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_Result_WhenSet_ShouldFirePropertyChange_TestFailing()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Result")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Result = new TestRunResult() { RunTestResult = RunResult.TestFailed };

            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
            Assert.IsFalse(input.TestInvalid);
            Assert.IsFalse(input.TestPending);
            Assert.IsFalse(input.TestPassed);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_Result_WhenSet_ShouldFirePropertyChange_TestInvalid()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Result")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Result = new TestRunResult() { RunTestResult = RunResult.TestInvalid };

            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
            Assert.IsFalse(input.TestFailing);
            Assert.IsFalse(input.TestPending);
            Assert.IsFalse(input.TestPassed);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_Result_WhenSet_ShouldFirePropertyChange_TestInvalid_TestResourceDeleted()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Result")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Result = new TestRunResult() { RunTestResult = RunResult.TestResourceDeleted };

            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
            Assert.IsFalse(input.TestFailing);
            Assert.IsFalse(input.TestPending);
            Assert.IsFalse(input.TestPassed);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_Result_WhenSet_ShouldFirePropertyChange_TestInvalid_TestResourcePathUpdated()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Result")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Result = new TestRunResult() { RunTestResult = RunResult.TestResourcePathUpdated };

            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
            Assert.IsFalse(input.TestFailing);
            Assert.IsFalse(input.TestPending);
            Assert.IsFalse(input.TestPassed);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestOutput_Result_WhenSet_ShouldFirePropertyChange_TestPending()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Result")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.Result = new TestRunResult() { RunTestResult = RunResult.TestPending };

            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
            Assert.IsFalse(input.TestFailing);
            Assert.IsFalse(input.TestInvalid);
            Assert.IsFalse(input.TestPassed);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateMatchVisibility_GivenOption1_ShouldSetUpCorrectly()
        {
            //---------------Set up test pack-------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(input);
            //---------------Execute Test ----------------------
            input.UpdateMatchVisibility("Is Email", new List<IFindRecsetOptions>() { new RsOpIsEmail() });
            //---------------Test Result -----------------------

            Assert.AreEqual(input.IsSearchCriteriaVisible, false);
            Assert.AreEqual(input.IsBetweenCriteriaVisible, false);
            Assert.AreEqual(input.IsSinglematchCriteriaVisible, false);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void UpdateMatchVisibility_GivenOption3_ShouldSetUpCorrectly()
        {
            //---------------Set up test pack-------------------
            var input = new ServiceTestOutput("someVar", "someValue", "", "");
            //---------------Assert Precondition----------------
            Assert.IsNotNull(input);
            //---------------Execute Test ----------------------
            input.UpdateMatchVisibility("Is Between", new List<IFindRecsetOptions>() { new RsOpIsBetween() });
            //---------------Test Result -----------------------

            Assert.AreEqual(input.IsSearchCriteriaVisible, true);
            Assert.AreEqual(input.IsBetweenCriteriaVisible, true);
            Assert.AreEqual(input.IsSinglematchCriteriaVisible, false);
        }



    }
}