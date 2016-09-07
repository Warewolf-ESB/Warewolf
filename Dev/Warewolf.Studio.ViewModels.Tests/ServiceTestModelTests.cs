using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
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
            var testModel = new ServiceTestModel();
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
        [TestCategory("TestModel_UserName")]
        public void TestModel_Username_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
        [TestCategory("TestModel_IsPublic")]
        public void TestModel_IsPublic_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "IsPublic")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.IsPublic = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(testModel.IsPublic);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("TestModel_IsTestSelected")]
        public void TestModel_IsTestSelected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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
        [TestCategory("TestModel_ErrorExpected")]
        public void TestModel_ErrorExpected_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new ServiceTestModel();
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
            var testModel = new ServiceTestModel();
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


    }
}