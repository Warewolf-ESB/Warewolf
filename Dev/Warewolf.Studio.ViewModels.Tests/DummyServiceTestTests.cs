using System.Collections.Generic;
using Dev2.Common.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class DummyServiceTestTests
    {
        [TestMethod]
        [Timeout(100)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DummyServiceTest_Constructor")]
        public void DummyServiceTest_Constructor_Constructor_NoException()
        {
            //------------Setup for test--------------------------
            var dummyServiceTest = new DummyServiceTest(b => { });
            //------------Execute Test---------------------------
            Assert.IsNotNull(dummyServiceTest);
            //------------Assert Results-------------------------
            
        }

        

        [TestMethod]
        [Timeout(100)]
        [Owner("Nkosinathi Sangweni")]
        [TestCategory("DummyServiceTest_Constructor")]
        public void DummyServiceTest_Constructor_isNew_SetsNeverRunTests()
        {
            //------------Setup for test--------------------------
            var dummyServiceTest = new DummyServiceTest(b => { });
            //------------Execute Test---------------------------
            Assert.IsNotNull(dummyServiceTest);
            //------------Assert Results-------------------------
            Assert.AreEqual("Never run", dummyServiceTest.NeverRunString);

        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Nkosinathi Sangweni")]
        public void Constuctor_GivenNullAction_ShouldPassThrough()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            var dummyServiceTest = new DummyServiceTest(null);
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Timeout(250)]
        [Owner("Nkosinathi Sangweni")]
        public void Constuctor_GivenAction_ShouldDefaultPropertyValues()
        {
            //---------------Set up test pack-------------------
#pragma warning disable 219
            var dummyServiceTest =  new DummyServiceTest(b => { });
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
        [Timeout(100)]
        [Owner("Nkosinathi Sangweni")]
        public void Constuctor_GivenAction_ShouldCreateCommand()
        {
            //---------------Set up test pack-------------------
#pragma warning disable 219
            var dummyServiceTest = new DummyServiceTest(b => { });
#pragma warning restore 219
            //---------------Assert Precondition----------------
            //---------------Execute Test ----------------------

            //---------------Test Result -----------------------
            Assert.IsNotNull(dummyServiceTest.CreateTestCommand);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Nkosinathi Sangweni")]
        public void CreateTestCommand_GivenAction_ShouldExecuteCorrectly()
        {
            //---------------Set up test pack-------------------
#pragma warning disable 219
            var dummyServiceTest = new DummyServiceTest(b => { });
#pragma warning restore 219
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dummyServiceTest.CreateTestCommand);
            //---------------Execute Test ----------------------
            dummyServiceTest.CreateTestCommand.Execute(null);
            //---------------Test Result -----------------------

        }
    }
}
