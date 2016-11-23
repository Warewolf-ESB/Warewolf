using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServiceTestModelTests
    {

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestModel_Name")]
        public void TestModel_Name_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "TestName")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.TestName = "Test Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("Test Name", testModel.TestName);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_Constructor")]
        public void ServiceTestModel_Constructor_WithResourceId_ShouldSetProperties()
        {
            //------------Setup for test--------------------------
            var resourceId = Guid.NewGuid();
            //------------Execute Test---------------------------
            var serviceTestModel = new ServiceTestModel(resourceId);
            //------------Assert Results-------------------------
            Assert.AreEqual(resourceId, serviceTestModel.ParentId);
            Assert.IsTrue(serviceTestModel.NeverRunStringVisibility);
            Assert.IsFalse(serviceTestModel.IsTestRunning);
            Assert.AreEqual("Never run", serviceTestModel.NeverRunString);
            Assert.AreEqual(0, serviceTestModel.TestSteps.Count);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NerverRunStringVisibility_GivenisNew_ShouldBeVisible()
        {
            //---------------Set up test pack-------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testModel.NeverRunStringVisibility);
            Assert.AreEqual(default(DateTime), testModel.LastRunDate);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsTrue(testModel.NeverRunStringVisibility);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void NerverRunStringVisibility_GivenLastRunDateHasValue_ShouldCollapsed()
        {
            //---------------Set up test pack-------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testModel.NeverRunStringVisibility);
            Assert.AreEqual(default(DateTime), testModel.LastRunDate);
            //---------------Execute Test ----------------------
            testModel.LastRunDate = DateTime.Now;
            //---------------Test Result -----------------------
            Assert.IsFalse(testModel.NeverRunStringVisibility);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LastRunDateVisibility_GivenLastRunDateHasValue_ShouldBeVisible()
        {
            //---------------Set up test pack-------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testModel.LastRunDateVisibility);
            Assert.AreEqual(default(DateTime), testModel.LastRunDate);
            //---------------Execute Test ----------------------
            testModel.LastRunDate = DateTime.Now;
            //---------------Test Result -----------------------
            Assert.IsTrue(testModel.LastRunDateVisibility);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void LastRunDateVisibility_GivenisNew_ShouldBeCollapsed()
        {
            //---------------Set up test pack-------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(testModel.LastRunDateVisibility);
            Assert.AreEqual(default(DateTime), testModel.LastRunDate);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.IsFalse(testModel.LastRunDateVisibility);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void TestModel_NeverRunString_WhenNew_ShouldSetNeverRunString()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            //------------Execute Test---------------------------
            Assert.IsFalse(string.IsNullOrEmpty(testModel.NeverRunString));
            //------------Assert Results-------------------------
            Assert.AreEqual("Never run", testModel.NeverRunString);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_NameForDisplay")]
        public void TestModel_NameForDisplay_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "NameForDisplay")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.NameForDisplay = "Test Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("Test Name", testModel.NameForDisplay);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_IsTestRunning")]
        public void TestModel_IsTestRunning_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsTestRunning")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.IsTestRunning = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(testModel.IsTestRunning);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_OldTestName")]
        public void TestModel_OldTest_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "OldTestName")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.OldTestName = "Old Test Name";
            //------------Assert Results-------------------------
            Assert.AreEqual("Old Test Name", testModel.OldTestName);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestModel_OldTestName")]
        public void TestModel_DebugForTest_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DebugForTest")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            var debugForTest = new List<IDebugState>();
            testModel.DebugForTest = debugForTest;
            //------------Assert Results-------------------------
            Assert.IsTrue(ReferenceEquals(debugForTest, testModel.DebugForTest));
            Assert.IsTrue(_wasCalled);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestModel_OldTestName")]
        public void TestModel_DuplicateTestTooltip_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "DuplicateTestTooltip")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.DuplicateTestTooltip = "jjj";
            //------------Assert Results-------------------------
            Assert.AreEqual("jjj", testModel.DuplicateTestTooltip);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestModel_UserName")]
        public void TestModel_Username_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "UserName")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.UserName = "theUser";
            //------------Assert Results-------------------------
            Assert.AreEqual("theUser", testModel.UserName);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestModel_Password")]
        public void TestModel_Password_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Password")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.Password = "pword";
            //------------Assert Results-------------------------
            Assert.AreEqual("pword", testModel.Password);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestModel_RunSelectedTestUrl")]
        public void TestModel_RunSelectedTestUrl_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "RunSelectedTestUrl")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.RunSelectedTestUrl = "Url";
            //------------Assert Results-------------------------
            Assert.AreEqual("Url", testModel.RunSelectedTestUrl);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestModel_Enabled")]
        public void TestModel_Enabled_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Enabled")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.Enabled = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(testModel.Enabled);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestModel_LastRunDate")]
        public void TestModel_LastRunDate_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "LastRunDate")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            var lastRunDate = DateTime.Now;
            testModel.LastRunDate = lastRunDate;
            //------------Assert Results-------------------------
            Assert.AreEqual(lastRunDate, testModel.LastRunDate);
            Assert.IsTrue(_wasCalled);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_TestPassed")]
        public void TestModel_TestPassed_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
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
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_TestFailing")]
        public void TestModel_TestFailing_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
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
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_TestInvalid")]
        public void TestModel_TestInvalid_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
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
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_TestPending")]
        public void TestModel_TestPending_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
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
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_Inputs")]
        public void TestModel_Inputs_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Inputs")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.Inputs = new ObservableCollection<IServiceTestInput>();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, testModel.Inputs.Count);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_Outputs")]
        public void TestModel_Outputs_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Outputs")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.Outputs = new ObservableCollection<IServiceTestOutput>();
            //------------Assert Results-------------------------
            Assert.AreEqual(0, testModel.Outputs.Count);
            Assert.IsTrue(_wasCalled);
        }


        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_AuthenticationType")]
        public void TestModel_AuthenticationType_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "AuthenticationType")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.AuthenticationType = AuthenticationType.User;
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthenticationType.User, testModel.AuthenticationType);
            Assert.IsTrue(testModel.UserAuthenticationSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_UserAuthenticationSelected")]
        public void TestModel_UserAuthenticationSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "UserAuthenticationSelected")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.AuthenticationType = AuthenticationType.Windows;
            //------------Assert Results-------------------------
            Assert.AreEqual(AuthenticationType.Windows, testModel.AuthenticationType);
            Assert.IsFalse(testModel.UserAuthenticationSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_IsTestSelected")]
        public void TestModel_IsTestSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsTestSelected")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.IsTestSelected = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(testModel.IsTestSelected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_IsNewTest")]
        public void TestModel_IsNewTest_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsNewTest")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.IsNewTest = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(testModel.IsNewTest);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_ParentId")]
        public void TestModel_ParentId_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ParentId")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            var guid = Guid.NewGuid();
            testModel.ParentId = guid;
            //------------Assert Results-------------------------
            Assert.AreEqual(guid, testModel.ParentId);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_ErrorExpected")]
        public void TestModel_ErrorExpected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ErrorExpected")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.ErrorExpected = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(testModel.ErrorExpected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_ErrorContainsText")]
        public void TestModel_ErrorContainsText_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "ErrorContainsText")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.ErrorContainsText = "Test";
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", testModel.ErrorContainsText);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_NoErrorExpected")]
        public void TestModel_NoErrorExpected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "NoErrorExpected")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.NoErrorExpected = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(testModel.NoErrorExpected);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_NewTest")]
        public void TestModel_NewTest_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "NewTest")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.NewTest = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(testModel.NewTest);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("TestModel_NewTest")]
        public void TestModel_SelectedTestStep_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "SelectedTestStep")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.SelectedTestStep = new Mock<IServiceTestStep>().Object;
            //------------Assert Results-------------------------
            Assert.IsNotNull(testModel.SelectedTestStep);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_AddRow")]
        public void ServiceTestModel_AddRow_WhenRecordsetValueUpdated_ShouldAddNewRow()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput
            };
            //------------Execute Test---------------------------
            var dataListModel = new DataListModel();
            var shapeRecordSets = new List<IRecordSet>();
            var recordSet = new RecordSet
            {
                IODirection = enDev2ColumnArgumentDirection.Input,
                Name = "rec"
            };
            var recordSetColumns = new Dictionary<int, List<IScalar>>
            {
                {
                    1, new List<IScalar>
                    {
                        new Scalar
                        {
                            Name = "a",
                            IODirection = enDev2ColumnArgumentDirection.Input
                        }
                    }
                }
            };
            recordSet.Columns = recordSetColumns;
            shapeRecordSets.Add(recordSet);
            dataListModel.ShapeRecordSets = shapeRecordSets;
            serviceTestModel.AddRow(serviceTestInput, dataListModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, serviceTestModel.Inputs.Count);
            Assert.AreEqual("rec(2).a", serviceTestModel.Inputs[1].Variable);
            Assert.AreEqual("", serviceTestModel.Inputs[1].Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("ServiceTestModel_AddRow")]
        public void ServiceTestModel_AddRow_WhenRecordsetValueUpdated_ShouldAddNewOutPutRow()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestOutput("rec(1).a", "val", "", "");
            serviceTestModel.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                serviceTestInput
            };
            //------------Execute Test---------------------------
            var dataListModel = new DataListModel();
            var shapeRecordSets = new List<IRecordSet>();
            var recordSet = new RecordSet
            {
                IODirection = enDev2ColumnArgumentDirection.Output,
                Name = "rec"
            };
            var recordSetColumns = new Dictionary<int, List<IScalar>>
            {
                {
                    1, new List<IScalar>
                    {
                        new Scalar
                        {
                            Name = "a",
                            IODirection = enDev2ColumnArgumentDirection.Output
                        }
                    }
                }
            };
            recordSet.Columns = recordSetColumns;
            shapeRecordSets.Add(recordSet);
            dataListModel.ShapeRecordSets = shapeRecordSets;
            serviceTestModel.AddRow(serviceTestInput, dataListModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, serviceTestModel.Outputs.Count);
            Assert.AreEqual("rec(2).a", serviceTestModel.Outputs[1].Variable);
            Assert.AreEqual("", serviceTestModel.Outputs[1].Value);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsDirty_GivenNameChanged_ShouldReurnTrue()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput
            };
            serviceTestModel.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                new ServiceTestOutput("name","vale","","")
            };
            serviceTestModel.TestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(Guid.NewGuid(), "Switch", new ObservableCollection<IServiceTestOutput>(), StepType.Mock )
                {
                    StepOutputs = new ObservableCollection<IServiceTestOutput>
                    {
                        new ServiceTestOutput("j","Jvalue","","")
                    }
                }
            };
            var dataListModel = new DataListModel();
            var shapeRecordSets = new List<IRecordSet>();
            var recordSet = new RecordSet
            {
                IODirection = enDev2ColumnArgumentDirection.Input,
                Name = "rec"
            };
            var recordSetColumns = new Dictionary<int, List<IScalar>>
            {
                {
                    1, new List<IScalar>
                    {
                        new Scalar
                        {
                            Name = "a",
                            IODirection = enDev2ColumnArgumentDirection.Input
                        }
                    }
                }
            };
            recordSet.Columns = recordSetColumns;
            shapeRecordSets.Add(recordSet);
            dataListModel.ShapeRecordSets = shapeRecordSets;
            serviceTestModel.AddRow(serviceTestInput, dataListModel);

            var serviceTestModelClone = new ServiceTestModel(Guid.NewGuid()) { Inputs = serviceTestModel.Inputs, TestName = "NewTestName", Outputs = serviceTestModel.Outputs, TestSteps = serviceTestModel.TestSteps };
            serviceTestModelClone.AddRow(serviceTestInput, dataListModel);

            serviceTestModel.SetItem(serviceTestModelClone);



            //---------------Assert Precondition----------------
            Assert.AreNotEqual(serviceTestModel.TestName, serviceTestModelClone.TestName);
            //---------------Execute Test ----------------------
            var isDirty = serviceTestModel.IsDirty;
            //---------------Test Result -----------------------
            Assert.IsTrue(isDirty);
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsDirty_GivenInputsChanges_ShouldReurnTrue()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput
            };
            var dataListModel = new DataListModel();
            var shapeRecordSets = new List<IRecordSet>();
            var recordSet = new RecordSet
            {
                IODirection = enDev2ColumnArgumentDirection.Input,
                Name = "rec"
            };
            var recordSetColumns = new Dictionary<int, List<IScalar>>
            {
                {
                    1, new List<IScalar>
                    {
                        new Scalar
                        {
                            Name = "a",
                            IODirection = enDev2ColumnArgumentDirection.Input
                        }
                    }
                }
            };
            recordSet.Columns = recordSetColumns;
            shapeRecordSets.Add(recordSet);
            dataListModel.ShapeRecordSets = shapeRecordSets;
            serviceTestModel.AddRow(serviceTestInput, dataListModel);

            var serviceTestModelClone = new ServiceTestModel(Guid.NewGuid())
            {
                Inputs = new ObservableCollection<IServiceTestInput>
                {
                new ServiceTestInput("rec(1).a", "valChanges")
            }
            };
            serviceTestModelClone.AddRow(serviceTestInput, dataListModel);
            serviceTestModel.SetItem(serviceTestModelClone);
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var isDirty = serviceTestModel.IsDirty;
            //---------------Test Result -----------------------
            Assert.IsTrue(isDirty);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void IsDirty_GivenNoChanges_ShouldReurnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput
            };
            var dataListModel = new DataListModel();
            var shapeRecordSets = new List<IRecordSet>();
            var recordSet = new RecordSet
            {
                IODirection = enDev2ColumnArgumentDirection.Input,
                Name = "rec"
            };
            var recordSetColumns = new Dictionary<int, List<IScalar>>
            {
                {
                    1, new List<IScalar>
                    {
                        new Scalar
                        {
                            Name = "a",
                            IODirection = enDev2ColumnArgumentDirection.Input
                        }
                    }
                }
            };
            recordSet.Columns = recordSetColumns;
            shapeRecordSets.Add(recordSet);
            dataListModel.ShapeRecordSets = shapeRecordSets;
            serviceTestModel.AddRow(serviceTestInput, dataListModel);

            var serviceTestModelClone = new ServiceTestModel(Guid.NewGuid()) { Inputs = serviceTestModel.Inputs };
            serviceTestModelClone.AddRow(serviceTestInput, dataListModel);

            serviceTestModel.SetItem(serviceTestModelClone);



            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------
            var isDirty = serviceTestModel.IsDirty;
            //---------------Test Result -----------------------
            Assert.IsFalse(isDirty);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Clone_GivenObjects_ShouldReturnANewShallowCopyOfTheObject()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");

            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput
            };
            serviceTestModel.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                new ServiceTestOutput("name","value","from","to"),
                new ServiceTestOutput("name2","value","from","to"),

            };
            serviceTestModel.TestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(Guid.NewGuid(), "DsfDecision", new ObservableCollection<IServiceTestOutput>(), StepType.Mock )
            };
            var dataListModel = new DataListModel();
            var shapeRecordSets = new List<IRecordSet>();
            var recordSet = new RecordSet
            {
                IODirection = enDev2ColumnArgumentDirection.Input,
                Name = "rec"
            };
            var recordSetColumns = new Dictionary<int, List<IScalar>>
            {
                {
                    1, new List<IScalar>
                    {
                        new Scalar
                        {
                            Name = "a",
                            IODirection = enDev2ColumnArgumentDirection.Input
                        }
                    }
                }
            };
            recordSet.Columns = recordSetColumns;
            shapeRecordSets.Add(recordSet);
            dataListModel.ShapeRecordSets = shapeRecordSets;
            serviceTestModel.AddRow(serviceTestInput, dataListModel);

            var clone = serviceTestModel.Clone();
            //---------------Assert Precondition----------------
            var isCorrectType = clone is ServiceTestModel;
            Assert.IsTrue(isCorrectType);
            //---------------Execute Test ----------------------
            var referenceEquals = ReferenceEquals(serviceTestModel, clone);
            //---------------Test Result -----------------------
            Assert.IsFalse(referenceEquals);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Clone_GivenObjectsWithDifferntInputs_ShouldFalseEquality()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };
            serviceTestModel.TestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(Guid.NewGuid(),typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
            };
            var dataListModel = new DataListModel();
            var shapeRecordSets = new List<IRecordSet>();
            var recordSet = new RecordSet
            {
                IODirection = enDev2ColumnArgumentDirection.Input,
                Name = "rec"
            };
            var recordSetColumns = new Dictionary<int, List<IScalar>>
            {
                {
                    1, new List<IScalar>
                    {
                        new Scalar
                        {
                            Name = "a",
                            IODirection = enDev2ColumnArgumentDirection.Input
                        }
                    }
                }
            };
            recordSet.Columns = recordSetColumns;
            shapeRecordSets.Add(recordSet);
            dataListModel.ShapeRecordSets = shapeRecordSets;
            serviceTestModel.AddRow(serviceTestInput, dataListModel);


            var clone = serviceTestModel.Clone();
            //---------------Assert Precondition----------------
            var isCorrectType = clone is ServiceTestModel;
            Assert.IsTrue(isCorrectType);
            var referenceEquals = ReferenceEquals(serviceTestModel, clone);
            var inputRefs = ReferenceEquals(serviceTestModel.Inputs, ((ServiceTestModel)clone).Inputs);
            Assert.IsFalse(referenceEquals);
            Assert.IsFalse(inputRefs);
            //---------------Execute Test ----------------------


            ((ServiceTestModel)clone).Inputs[0].Variable = "NewVariable";
            //---------------Test Result -----------------------
            var condition = serviceTestModel.Equals(clone);
            Assert.IsFalse(condition);

        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void StepChildrenCompare_GivenSameServiceModelSteps_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };
            var serviceTestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(Guid.NewGuid(),typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
            };
            serviceTestModel.TestSteps = serviceTestSteps;
            var methodInfo = typeof(ServiceTestModel).GetMethod("StepChildrenCompare", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var areEqual = methodInfo.Invoke(null, new object[] { serviceTestModel.TestSteps, serviceTestSteps });

            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void StepChildrenCompare_GivenDifferentServiceModelSteps_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };
            var uniqueId = Guid.NewGuid();
            var serviceTestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
            };
            serviceTestModel.TestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Mock)
            }; ;
            var methodInfo = typeof(ServiceTestModel).GetMethod("StepChildrenCompare", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var areEqual = methodInfo.Invoke(null, new object[] { serviceTestModel.TestSteps, serviceTestSteps });

            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void StepChildrenCompare_GivenDifferentServiceModelSteps_ShouldReturnFalse_recursive()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };
            var uniqueId = Guid.NewGuid();
            var serviceTestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                {
                     StepOutputs = new ObservableCollection<IServiceTestOutput>
                    {
                        new ServiceTestOutput("a","b","from","to")
                    } ,
                    Children = new ObservableCollection<IServiceTestStep>
                    {
                          new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                                {
                                     StepOutputs = new ObservableCollection<IServiceTestOutput>
                                    {
                                        new ServiceTestOutput("a","b","from","to")
                                    } ,
                                }
                    }
                }
            };
            serviceTestModel.TestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                {
                    StepOutputs = new ObservableCollection<IServiceTestOutput>
                    {
                        new ServiceTestOutput("a","a","from","to")
                    }
                     ,
                    Children = new ObservableCollection<IServiceTestStep>
                    {
                          new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                                {
                                     StepOutputs = new ObservableCollection<IServiceTestOutput>
                                    {
                                        new ServiceTestOutput("a","b","from","to")
                                    } ,
                                }
                    }
                }
            }; ;
            var methodInfo = typeof(ServiceTestModel).GetMethod("StepChildrenCompare", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var areEqual = methodInfo.Invoke(null, new object[] { serviceTestModel.TestSteps, serviceTestSteps });

            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void StepChildrenCompare_GivenSameServiceModelSteps_ShouldReturnTrue_recursive()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };
            var uniqueId = Guid.NewGuid();
            var serviceTestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                {
                     StepOutputs = new ObservableCollection<IServiceTestOutput>
                    {
                        new ServiceTestOutput("a","b","from","to")
                    } ,
                    Children = new ObservableCollection<IServiceTestStep>
                    {
                          new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                                {
                                     StepOutputs = new ObservableCollection<IServiceTestOutput>
                                    {
                                        new ServiceTestOutput("a","b","from","to")
                                    } ,
                                }
                    }
                }
            };
            serviceTestModel.TestSteps = new ObservableCollection<IServiceTestStep>
            {
                new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                {
                    StepOutputs = new ObservableCollection<IServiceTestOutput>
                    {
                        new ServiceTestOutput("a","b","from","to")
                    }
                     ,
                    Children = new ObservableCollection<IServiceTestStep>
                    {
                          new ServiceTestStep(uniqueId,typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>(), StepType.Assert)
                                {
                                     StepOutputs = new ObservableCollection<IServiceTestOutput>
                                    {
                                        new ServiceTestOutput("a","b","from","to")
                                    } ,
                                }
                    }
                }
            }; ;
            var methodInfo = typeof(ServiceTestModel).GetMethod("StepChildrenCompare", BindingFlags.NonPublic | BindingFlags.Static);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var areEqual = methodInfo.Invoke(null, new object[] { serviceTestModel.TestSteps, serviceTestSteps });

            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(areEqual.ToString()));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OutputCompare_GivenDifferentVariable_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestOutput("rec(1).a", "val", "", "");
            serviceTestModel.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("OutputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var testModel = new ServiceTestModel
            {
                Outputs = new ObservableCollection<IServiceTestOutput>()
                {
                    new ServiceTestOutput("rec(1).b", "val", "", "")
                }
            };
            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { testModel, true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OutputCompare_GivenDifferentAssertOp_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestOutput("rec(1).a", "val", "", "") { AssertOp = "=" };
            serviceTestModel.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("OutputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var testModel = new ServiceTestModel
            {
                Outputs = new ObservableCollection<IServiceTestOutput>()
                {
                    new ServiceTestOutput("rec(1).a", "val", "", "") { AssertOp =">" }
                }
            };
            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { testModel, true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OutputCompare_GivenDifferentFrom_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestOutput("rec(1).a", "val", "a", "") { };
            serviceTestModel.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("OutputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var testModel = new ServiceTestModel
            {
                Outputs = new ObservableCollection<IServiceTestOutput>()
                {
                    new ServiceTestOutput("rec(1).a", "val", "", "") {  }
                }
            };
            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { testModel, true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OutputCompare_GivenDifferentTo_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestOutput("rec(1).a", "val", "a", "b") { };
            serviceTestModel.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("OutputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var testModel = new ServiceTestModel
            {
                Outputs = new ObservableCollection<IServiceTestOutput>()
                {
                    new ServiceTestOutput("rec(1).a", "val", "a", "") {  }
                }
            };
            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { testModel, true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OutputCompare_GivenDifferentValue_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestOutput("rec(1).a", "val", "", "");
            serviceTestModel.Outputs = new ObservableCollection<IServiceTestOutput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("OutputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            var testModel = new ServiceTestModel();
            testModel.Outputs = new ObservableCollection<IServiceTestOutput>()
            {
                new ServiceTestOutput("rec(1).a", "val1","","")
            };
            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { testModel, true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void StepOutputsCompare_GivenHasError_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("StepOutputsCompare", BindingFlags.NonPublic | BindingFlags.Static);

            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            try
            {
                var areEqual = methodInfo.Invoke(null, new object[] { serviceTestModel.Outputs, default(ObservableCollection<IServiceTestOutput>) });
            }
            catch (TargetInvocationException ex)
            {
                // ReSharper disable once PossibleNullReferenceException
                var b = ex.InnerException.GetType() == typeof(NullReferenceException);
                //---------------Test Result -----------------------
                Assert.IsTrue(b);
            }
        
            
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputCompare_GivenNullOther_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("InputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------
            try
            {
                methodInfo.Invoke(serviceTestModel, new object[] { default(ServiceTestModel), true });
            }
            catch (TargetInvocationException ex)
            {
                // ReSharper disable once PossibleNullReferenceException
                var b = ex.InnerException.GetType() == typeof(NullReferenceException);

                Assert.IsTrue(b);
            }

            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputCompare_GivenNull_inputs_ShouldReturnTrue()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("InputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            var fieldInfo = typeof(ServiceTestModel).GetField("_inputs", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            Assert.IsNotNull(fieldInfo);
            //---------------Execute Test ----------------------

            fieldInfo.SetValue(serviceTestModel, default(ObservableCollection<IServiceTestInput>));
            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { new ServiceTestModel(), true });
            //---------------Test Result -----------------------
            Assert.IsTrue(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputCompare_GivenDifferentVariables_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("InputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------

            var testModel = new ServiceTestModel
            {
                Inputs = new ObservableCollection<IServiceTestInput>
                {
                    new ServiceTestInput("rec(1).b", "val")
                }
            };

            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { testModel, true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputCompare_GivenDifferentValue_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("InputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------

            var testModel = new ServiceTestModel
            {
                Inputs = new ObservableCollection<IServiceTestInput>
                {
                    new ServiceTestInput("rec(1).a", "val1")
                }
            };

            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { testModel, true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void InputCompare_GivenDifferentEmptyAsNull_ShouldReturnFalse()
        {
            //---------------Set up test pack-------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var serviceTestInput = new ServiceTestInput("rec(1).a", "val");
            serviceTestModel.Inputs = new ObservableCollection<IServiceTestInput>
            {
                serviceTestInput,
            };

            var methodInfo = typeof(ServiceTestModel).GetMethod("InputCompare", BindingFlags.NonPublic | BindingFlags.Instance);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(methodInfo);
            //---------------Execute Test ----------------------

            var testModel = new ServiceTestModel
            {
                Inputs = new ObservableCollection<IServiceTestInput>
                {
                    new ServiceTestInput("rec(1).a", "val")
                    {
                        EmptyIsNull = true
                    }
                }
            };

            var areEqual = methodInfo.Invoke(serviceTestModel, new object[] { testModel, true });
            //---------------Test Result -----------------------
            Assert.IsFalse(bool.Parse(areEqual.ToString()));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_AddTestStep")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceTestModel_AddTestStep_EmtpyUniqueID_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());

            //------------Execute Test---------------------------
            serviceTestModel.AddTestStep("", "", typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_AddTestStep")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceTestModel_AddTestStep_NullUniqueID_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());

            //------------Execute Test---------------------------
            serviceTestModel.AddTestStep(null, "", typeof(DsfDecision).Name, new ObservableCollection<IServiceTestOutput>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_AddTestStep")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceTestModel_AddTestStep_NullTypeName_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());

            //------------Execute Test---------------------------
            serviceTestModel.AddTestStep(Guid.NewGuid().ToString(), "", null, new ObservableCollection<IServiceTestOutput>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_AddTestStep")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceTestModel_AddTestStep_EmptyTypeName_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());

            //------------Execute Test---------------------------
            serviceTestModel.AddTestStep(Guid.NewGuid().ToString(), "", "", new ObservableCollection<IServiceTestOutput>());
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_AddTestStep")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceTestModel_AddTestStep_NullOutputs_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());

            //------------Execute Test---------------------------
            serviceTestModel.AddTestStep(Guid.NewGuid().ToString(), "", typeof(DsfDecision).Name, null);
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_AddTestStep")]
        public void ServiceTestModel_AddTestStep_ValidArguments_ShouldAddToTestSteps()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());
            var uniqueID = Guid.NewGuid().ToString();
            var activityTypeName = typeof(DsfDecision).Name;
            var outputs = new ObservableCollection<IServiceTestOutput>();
            //------------Execute Test---------------------------
            serviceTestModel.AddTestStep(uniqueID, "", activityTypeName, outputs);
            //------------Assert Results-------------------------
            Assert.AreEqual(1, serviceTestModel.TestSteps.Count);
            var testStep = serviceTestModel.TestSteps[0];
            Assert.IsNotNull(testStep);
            Assert.AreEqual(uniqueID, testStep.UniqueId.ToString());
            Assert.AreEqual(activityTypeName, testStep.ActivityType);
            Assert.AreEqual(outputs, testStep.StepOutputs);
            Assert.AreEqual(StepType.Assert, testStep.Type);
        }
    }
}