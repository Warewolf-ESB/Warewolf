using System;
using System.Collections.Generic;
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
            testModel.Inputs = new List<IServiceTestInput>();
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
            testModel.Outputs = new List<IServiceTestOutput>();
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
            var serviceTestInput = new ServiceTestInput("rec(1).a","val");
            serviceTestModel.Inputs = new List<IServiceTestInput>
            {
                serviceTestInput
            };
            //------------Execute Test---------------------------
            var dataListModel = new DataListModel();
            var shapeRecordSets = new List<IRecordSet>();
            var recordSet = new RecordSet();
            recordSet.IODirection = enDev2ColumnArgumentDirection.Input;
            recordSet.Name = "rec";
            var recordSetColumns = new Dictionary<int, List<IScalar>>();
            recordSetColumns.Add(1,new List<IScalar>
            {
                new Scalar
                {
                    Name = "a",
                    IODirection = enDev2ColumnArgumentDirection.Input
                }
            });
            recordSet.Columns = recordSetColumns;
            shapeRecordSets.Add(recordSet);
            dataListModel.ShapeRecordSets = shapeRecordSets;
            serviceTestModel.AddRow(serviceTestInput,dataListModel);
            //------------Assert Results-------------------------
            Assert.AreEqual(2,serviceTestModel.Inputs.Count);
            Assert.AreEqual("rec(2).a",serviceTestModel.Inputs[1].Variable );
            Assert.AreEqual("",serviceTestModel.Inputs[1].Value );
        }
    }
}