using System;
using Dev2.CustomControls.Converters;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using WPF.JoshSmith.Data.ValueConverters;
// ReSharper disable InconsistentNaming

namespace Dev2.CustomControls.Tests
{
    [TestClass]
    public class ValueConverterGroupTests
    {
        public class Person
        {
            string Name { get; set; }
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void OnConstruction_ShouldCreateInstance()
        {
            //---------------Set up test pack-------------------

            //---------------Assert Precondition----------------

            //---------------Execute Test ----------------------
            try
            {
                // ReSharper disable once UnusedVariable
                var converterGroup = new ValueConverterGroup();
            }
            catch (Exception exception)
            {
                Assert.Fail(exception.Message);
            }
            //---------------Test Result -----------------------
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Converters_GivenIsNew_ShouldReturn0()
        {
            //---------------Set up test pack-------------------
            var converterGroup = new ValueConverterGroup();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(converterGroup);
            //---------------Execute Test ----------------------
            var observableCollection = converterGroup.Converters;
            //---------------Test Result -----------------------
            Assert.AreEqual(0, observableCollection.Count);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetTargetType_GivenValidArgsAndNoConverters_ShouldReturnType()
        {
            //---------------Set up test pack-------------------
            var converterGroup = new ValueConverterGroup();
            PrivateObject privateObject = new PrivateObject(converterGroup);
            var type = typeof(Person);
            //---------------Assert Precondition----------------
            Assert.IsNotNull(converterGroup);
            //---------------Execute Test ----------------------
            // Type GetTargetType(int converterIndex, Type finalTargetType, bool convert)
            var invoke = privateObject.Invoke("GetTargetType", 1, type, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(invoke);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GetTargetType_GivenValidArgsAndHasConverters_ShouldReturnType()
        {
            //---------------Set up test pack-------------------
            var converterGroup = new ValueConverterGroup();
            PrivateObject privateObject = new PrivateObject(converterGroup);
            var type = typeof(Person);
            converterGroup.Converters.Add(new EmptyStringToBoolConverter());
            converterGroup.Converters.Add(new EmptyStringToBoolConverter());
            converterGroup.Converters.Add(new EmptyStringToBoolConverter());
            converterGroup.Converters.Add(new EmptyStringToBoolConverter());
            //---------------Assert Precondition----------------
            Assert.IsNotNull(converterGroup);
            //---------------Execute Test ----------------------
            // Type GetTargetType(int converterIndex, Type finalTargetType, bool convert)
            var invoke = privateObject.Invoke("GetTargetType", 1, type, true);
            //---------------Test Result -----------------------
            Assert.IsNotNull(invoke);
        }
    }
}
