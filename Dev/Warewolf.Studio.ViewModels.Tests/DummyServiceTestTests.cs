using System;
using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DummyServiceTestTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DummyServiceTest_Constructor")]
        public void DummyServiceTest_Constructor_Constructor_NoException()
        {
            //------------Setup for test--------------------------
            var dummyServiceTest = new DummyServiceTest(() => { });
            //------------Execute Test---------------------------
            Assert.IsNotNull(dummyServiceTest);
            //------------Assert Results-------------------------
            
        }
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DummyServiceTest_Constructor")]
        public void DummyServiceTest_Constructor_isNew_SetsNeverRunTests()
        {
            //------------Setup for test--------------------------
            var dummyServiceTest = new DummyServiceTest(() => { });
            //------------Execute Test---------------------------
            Assert.IsNotNull(dummyServiceTest);
            //------------Assert Results-------------------------
            Assert.AreEqual("Never run", dummyServiceTest.NeverRunString);

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constuctor_GivenNullAction_ShouldThrowArgumentNull()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            // ReSharper disable once UnusedVariable
            var dummyServiceTest = new DummyServiceTest(null);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constuctor_GivenAction_ShouldDefaultPropertyValues()
        {
            //---------------Set up test pack-------------------
#pragma warning disable 219
            var dummyServiceTest = new DummyServiceTest(() => { var a = 2; });
#pragma warning restore 219
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dummyServiceTest);
            //---------------Execute Test ----------------------
            //---------------Test Result -----------------------
            Assert.AreEqual(default(bool), dummyServiceTest.Enabled);
            Assert.AreEqual(default(bool), dummyServiceTest.ErrorExpected);
            Assert.AreEqual(default(string), dummyServiceTest.ErrorContainsText);
            Assert.AreEqual(default(bool), dummyServiceTest.IsDirty);
            Assert.AreEqual(true, dummyServiceTest.IsNewTest);
            Assert.AreEqual(default(bool), dummyServiceTest.IsTestSelected);
            Assert.AreEqual(default(bool), dummyServiceTest.NoErrorExpected);
            Assert.AreEqual(default(bool), dummyServiceTest.TestFailing);
            Assert.AreEqual(default(bool), dummyServiceTest.TestInvalid);
            Assert.AreEqual(default(bool), dummyServiceTest.TestPassed);
            Assert.AreEqual(default(bool), dummyServiceTest.TestPending);
            Assert.AreEqual(default(bool), dummyServiceTest.UserAuthenticationSelected);
            Assert.AreEqual(default(string), dummyServiceTest.UserName);
            Assert.AreNotEqual(default(string), dummyServiceTest.NameForDisplay);
            Assert.AreEqual(default(string), dummyServiceTest.Password);
            Assert.AreEqual(default(string), dummyServiceTest.TestName);
            Assert.AreEqual(default(string), dummyServiceTest.UserName);
            Assert.AreEqual(default(List<IServiceTestInput>), dummyServiceTest.Inputs);
            Assert.AreEqual(default(List<IServiceTestOutput>), dummyServiceTest.Outputs);
        }


        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Constuctor_GivenAction_ShouldCreateCommand()
        {
            //---------------Set up test pack-------------------
#pragma warning disable 219
            var dummyServiceTest = new DummyServiceTest(()=> { var a = 2; });
#pragma warning restore 219
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(dummyServiceTest.CreateTestCommand);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTestCommand_GivenAction_ShouldExecuteCorrectly()
        {
            //---------------Set up test pack-------------------
#pragma warning disable 219
            var dummyServiceTest = new DummyServiceTest(()=> { var a = 2; });
#pragma warning restore 219
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dummyServiceTest.CreateTestCommand);
            //---------------Execute Test ----------------------
            dummyServiceTest.CreateTestCommand.Execute(null);
            //---------------Test Result -----------------------

        }
    }
}
