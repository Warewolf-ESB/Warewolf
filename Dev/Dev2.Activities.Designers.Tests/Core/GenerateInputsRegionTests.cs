using Dev2.Activities.Designers2.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Dev2.Activities.Designers.Tests.Core
{
    [TestClass]
    public class GenerateInputsRegionTests
    {
        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void GenerateInputsRegion_GivenIsNew_ShouldSetToolRegionName_To_GenerateInputsRegion()
        {
            var inputRegion = new GenerateInputsRegion();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(inputRegion.ToolRegionName);
            Assert.IsTrue(inputRegion.IsEnabled);
            //---------------Execute Test ----------------------
            Assert.AreEqual("GenerateInputsRegion", inputRegion.ToolRegionName);
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void Errors_GivenIsNew_ShouldHaveEmptyErrors()
        {
            var inputRegion = new GenerateInputsRegion();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(inputRegion.ToolRegionName);
            Assert.IsTrue(inputRegion.IsEnabled);
            Assert.IsFalse(inputRegion.IsInputCountEmpty);
            Assert.IsNull(inputRegion.Dependants);
            Assert.AreEqual("GenerateInputsRegion", inputRegion.ToolRegionName);
            //---------------Execute Test ----------------------
            Assert.AreEqual(0, inputRegion.Errors.Count);
            //---------------Test Result -----------------------

        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void CloneRegion_ShouldReturnNull()
        {
            var inputRegion = new GenerateInputsRegion();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(inputRegion.ToolRegionName);
            Assert.IsTrue(inputRegion.IsEnabled);
            Assert.IsFalse(inputRegion.IsInputCountEmpty);
            Assert.IsNull(inputRegion.Dependants);
            Assert.AreEqual("GenerateInputsRegion", inputRegion.ToolRegionName);
            Assert.AreEqual(0, inputRegion.Errors.Count);
            //---------------Execute Test ----------------------
            var cloneRegion = inputRegion.CloneRegion();
            //---------------Test Result -----------------------
            Assert.IsNull(cloneRegion);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ErrorsHandler_GivenIsNew_ShouldReturnNull()
        {
            var inputRegion = new GenerateInputsRegion();
            //---------------Assert Precondition----------------
            Assert.IsNotNull(inputRegion.ToolRegionName);
            Assert.IsTrue(inputRegion.IsEnabled);
            Assert.IsFalse(inputRegion.IsInputCountEmpty);
            Assert.IsNull(inputRegion.Dependants);
            Assert.AreEqual("GenerateInputsRegion", inputRegion.ToolRegionName);
            Assert.AreEqual(0, inputRegion.Errors.Count);
            //---------------Execute Test ----------------------
            var cloneRegion = inputRegion.ErrorsHandler;
            //---------------Test Result -----------------------
            Assert.IsNull(cloneRegion);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void ErrorsHandler_GivenIsIset_ShouldBeInvokable()
        {
            var inputRegion = new GenerateInputsRegion()
            {

            };
            bool wasInvoked = false;
            inputRegion.ErrorsHandler += (sender, list) =>
            {
                wasInvoked = true;
            };
            //---------------Assert Precondition----------------
            Assert.IsNotNull(inputRegion.ToolRegionName);
            Assert.IsTrue(inputRegion.IsEnabled);
            Assert.IsFalse(inputRegion.IsInputCountEmpty);
            Assert.IsNull(inputRegion.Dependants);
            Assert.AreEqual("GenerateInputsRegion", inputRegion.ToolRegionName);
            Assert.AreEqual(0, inputRegion.Errors.Count);
            //---------------Execute Test ----------------------
            inputRegion.ErrorsHandler.Invoke(new object(), new System.Collections.Generic.List<string>());
            //---------------Test Result -----------------------
            Assert.IsTrue(wasInvoked);
        }
    }
}
