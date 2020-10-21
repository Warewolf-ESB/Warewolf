using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;



namespace Warewolf.Studio.ViewModels.Tests
{
    [TestClass]
    public class ServiceTestInputTests
    {
        [TestMethod]
        [Timeout(100)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        [ExpectedException(typeof(ArgumentNullException))]
        public void TestInput_Constructor_WhenNullVariable_ShouldThrowException()
        {
            //------------Setup for test--------------------------


            //------------Execute Test---------------------------
            new ServiceTestInput(null, "someValue");
            //------------Assert Results-------------------------
        } 

        [TestMethod]
        [Timeout(100)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        public void TestInput_Constructor_WhenValidParameters_ShouldSetProperties()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var input = new ServiceTestInput("someVar", "someValue");
            //------------Assert Results-------------------------
            Assert.AreEqual("someVar",input.Variable);
            Assert.AreEqual("someValue",input.Value);
            Assert.IsFalse(input.EmptyIsNull);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        public void TestInput_Variable_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestInput("someVar", "someValue");
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
        [Timeout(100)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        public void TestInput_Value_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestInput("someVar", "someValue");
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

        [TestMethod]
        [Timeout(100)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Value")]
        public void TestInput_Value_WhenSetNonEmpty_ShouldInvokeAddRowAction()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestInput("someVar", "someValue");
            var _wasCalled = false;
            input.AddNewAction += () =>
            {                
               _wasCalled = true;                
            };
            //------------Execute Test---------------------------
            input.Value = "val";
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Value")]
        public void TestInput_Value_WhenSetEmpty_ShouldNotInvokeAddRowAction()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestInput("someVar", "someValue");
            var _wasCalled = false;
            input.AddNewAction += () =>
            {
                _wasCalled = true;
            };
            //------------Execute Test---------------------------
            input.Value = "";
            //------------Assert Results-------------------------
            Assert.IsFalse(_wasCalled);
        }

        [TestMethod]
        [Timeout(100)]
        [Owner("Hagashen Naidu")]
        [TestCategory("TestInput_Constructor")]
        public void TestInput_EmptyIsNull_WhenSet_ShouldFirePropertyChange()
        {
            //------------Setup for test--------------------------
            var input = new ServiceTestInput("someVar", "someValue");
            var _wasCalled = false;
            input.PropertyChanged += (sender, args) =>
            {
                if (args.PropertyName == "EmptyIsNull")
                {
                    _wasCalled = true;
                }
            };
            //------------Execute Test---------------------------
            input.EmptyIsNull = true;
            //------------Assert Results-------------------------
            Assert.IsTrue(_wasCalled);
        }
        
    }
}