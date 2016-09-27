using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Data;
using Dev2.Data.Binary_Objects;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
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
            Assert.AreEqual(resourceId,serviceTestModel.ParentId);
            Assert.IsTrue(serviceTestModel.NeverRunStringVisibility);
            Assert.IsFalse(serviceTestModel.IsTestRunning);
            Assert.AreEqual("Never run", serviceTestModel.NeverRunString);
            Assert.AreEqual(0,serviceTestModel.TestSteps.Count);
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
        [Owner("Nkosinathi Sangweni")]
        public void TestModel_GivenIsNew_ShouldBeClonable()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel(Guid.NewGuid());
            //------------Execute Test---------------------------
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(testModel, typeof(ICloneable));
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
        public void IsDirty_GivenNameChanged_ShouldReurnTrue()
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

            var serviceTestModelClone = new ServiceTestModel(Guid.NewGuid()) { Inputs = serviceTestModel.Inputs, TestName = "NewTestName" };
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

            var serviceTestModelClone = new ServiceTestModel(Guid.NewGuid()) { Inputs = new ObservableCollection<IServiceTestInput>()
            {
                new ServiceTestInput("rec(1).a", "valChanges")
            } };
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
        [Owner("Hagashen Naidu")]
        [TestCategory("ServiceTestModel_AddTestStep")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ServiceTestModel_AddTestStep_EmtpyUniqueID_ShouldThrowException()
        {
            //------------Setup for test--------------------------
            var serviceTestModel = new ServiceTestModel(Guid.NewGuid());

            //------------Execute Test---------------------------
            serviceTestModel.AddTestStep("", "", typeof(DsfDecision).Name,new List<IServiceTestOutput>());
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
            serviceTestModel.AddTestStep(null, "", typeof(DsfDecision).Name,new List<IServiceTestOutput>());
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
            serviceTestModel.AddTestStep(Guid.NewGuid().ToString(), "", null,new List<IServiceTestOutput>());
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
            serviceTestModel.AddTestStep(Guid.NewGuid().ToString(), "", "",new List<IServiceTestOutput>());
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
            serviceTestModel.AddTestStep(Guid.NewGuid().ToString(), "", typeof(DsfDecision).Name,null);
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
            var outputs = new List<IServiceTestOutput>();
            //------------Execute Test---------------------------
            serviceTestModel.AddTestStep(uniqueID, "", activityTypeName,outputs);
            //------------Assert Results-------------------------
            Assert.AreEqual(1,serviceTestModel.TestSteps.Count);
            var testStep = serviceTestModel.TestSteps[0];
            Assert.IsNotNull(testStep);
            Assert.AreEqual(uniqueID,testStep.UniqueId.ToString());
            Assert.AreEqual(activityTypeName,testStep.ActivityType);
            Assert.AreEqual(outputs,testStep.StepOutputs);
            Assert.AreEqual(StepType.Mock,testStep.Type);
        }        
    }
}