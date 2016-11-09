using System;
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
            Assert.AreEqual("someVar",input.Variable);
            Assert.AreEqual("someValue",input.Value);
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
        
    }
}