using System;
using System.Collections.ObjectModel;
using System.Linq;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServiceTestStepTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_ActivityType_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ActivityType")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.ActivityType = typeof(DsfSwitch).Name;
            //------------Assert Results-------------------------
            Assert.AreEqual(typeof(DsfSwitch).Name, testModel.ActivityType);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_AssertSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "AssertSelected")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.AssertSelected = true;
            //------------Assert Results-------------------------
            Assert.AreEqual(true, testModel.AssertSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_Children_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Children")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.Children = new ObservableCollection<IServiceTestStep>();
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_StepDescription_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "StepDescription")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.StepDescription = "Desc";
            //------------Assert Results-------------------------
            Assert.AreEqual("Desc", testModel.StepDescription);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_MockSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "MockSelected")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.MockSelected = true;
            //------------Assert Results-------------------------
            Assert.AreEqual(true, testModel.MockSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_TestPending_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestPending")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.TestPending = true;
            //------------Assert Results-------------------------
            Assert.AreEqual(true, testModel.TestPending);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]

        public void ServiceTestStep_TestFailing_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestFailing")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.TestFailing = true;
            //------------Assert Results-------------------------
            Assert.AreEqual(true, testModel.TestFailing);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_TestInvalid_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestInvalid")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.TestInvalid = true;
            //------------Assert Results-------------------------
            Assert.AreEqual(true, testModel.TestInvalid);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_TestPassed_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestPassed")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.TestPassed = true;
            //------------Assert Results-------------------------
            Assert.AreEqual(true, testModel.TestPassed);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_Result_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Result")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            var testRunResult = new TestRunResult();
            testModel.Result = testRunResult;
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_Type_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Type")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.Type = StepType.Mock;
            //------------Assert Results-------------------------
            Assert.AreEqual(StepType.Mock, testModel.Type);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_Parent_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Parent")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.Parent = new Mock<IServiceTestStep>().Object;
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_StepOutputs_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "StepOutputs")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.StepOutputs = new ObservableCollection<IServiceTestOutput>();
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_AddNewOutput_WhenEmptyVariable_ShouldNotAddStepOutPut()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var beforeCount = testModel.StepOutputs.Count;
            //------------Execute Test---------------------------
            testModel.AddNewOutput("");
            var afterCount = testModel.StepOutputs.Count;
            //------------Assert Results-------------------------
            Assert.AreEqual(beforeCount, afterCount);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_AddNewOutput_WhenVariableIsRecordSet_ShouldAddStepOutPutWithAddAction()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var beforeCount = testModel.StepOutputs.Count;
            //------------Execute Test---------------------------
            testModel.AddNewOutput("rec().a");
            var serviceTestOutput = (ServiceTestOutput)testModel.StepOutputs.Last();
            //------------Assert Results-------------------------
            Assert.AreEqual(testModel.StepOutputs.Count, beforeCount + 1);
            Assert.AreEqual("rec().a", testModel.StepOutputs.Single().Variable);
            Assert.AreEqual("", testModel.StepOutputs.Single().From);
            Assert.AreEqual("", testModel.StepOutputs.Single().To);
            Assert.AreEqual("", testModel.StepOutputs.Single().Value);
            Assert.IsNotNull(serviceTestOutput.AddNewAction);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ServiceTestStep_AddNewOutput_WhenVariableIsNumericRecordSet_ShouldAddStepOutPutWithAddAction()
        {
            //------------Setup for test--------------------------
            var testModel = CreateDecisionMock();
            var beforeCount = testModel.StepOutputs.Count;
            //------------Execute Test---------------------------
            testModel.AddNewOutput("rec(1).a");
            var serviceTestOutput = (ServiceTestOutput)testModel.StepOutputs.Last();
            //------------Assert Results-------------------------
            Assert.AreEqual(testModel.StepOutputs.Count, beforeCount + 1);
            Assert.AreEqual("rec(2).a", testModel.StepOutputs.Single().Variable);
            Assert.AreEqual("", testModel.StepOutputs.Single().From);
            Assert.AreEqual("", testModel.StepOutputs.Single().To);
            Assert.AreEqual("", testModel.StepOutputs.Single().Value);
            Assert.IsNotNull(serviceTestOutput.AddNewAction);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void MockSelected_GivenIsTrue_ShouldSetUpCorrectly()
        {
            //---------------Set up test pack-------------------
            var serviceTestOutput = new ServiceTestOutput("a", "a", "", "");
            var testModel = new ServiceTestStep(Guid.NewGuid(), typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>()
            { serviceTestOutput }, StepType.Mock);
            //---------------Assert Precondition----------------
            Assert.IsTrue(testModel.MockSelected);
            //---------------Execute Test ----------------------
            testModel.MockSelected = true;
            //---------------Test Result -----------------------
            Assert.AreEqual(false, serviceTestOutput.IsBetweenCriteriaVisible);
            Assert.AreEqual(true, serviceTestOutput.IsSinglematchCriteriaVisible);
            Assert.AreEqual(true, serviceTestOutput.IsSearchCriteriaVisible);
        }

       


        private static ServiceTestStep CreateDecisionMock()
        {
            return new ServiceTestStep(Guid.NewGuid(), typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Mock);
        }
    }

   
}
