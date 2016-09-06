using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class TestModelTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestModel_Name")]
        public void TestModel_Name_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new TestModel();
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
            Assert.AreEqual("Test Name",testModel.TestName);
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestModel_Name")]
        public void TestModel_Username_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new TestModel();
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
        [TestCategory("TestModel_Name")]
        public void TestModel_Password_WhenSet_ShouldFirePropertyChanged()
        {
            //------------Setup for test--------------------------
            var testModel = new TestModel();
            var _wasCalled = false;
            testModel.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "Password")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            testModel.Password = "thePassword";
            //------------Assert Results-------------------------
            Assert.AreEqual("thePassword", testModel.Password);
            Assert.IsTrue(_wasCalled);
        }
    }
}