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
                  if (args.PropertyName == "Name")
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
                if (args.PropertyName == "Username")
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
    }
}