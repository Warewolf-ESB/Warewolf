using System;
using Dev2.Data.MathOperations;
using Infragistics.Calculations.CalcManager;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.MathOperations
{
    [TestClass]
    public class FunctionsTests
    {
        [TestMethod]
        public void Function_ShouldHAveConstructor()
        {
            var function = MathOpsFactory.CreateFunction();
            Assert.IsNotNull(function);
        }

        [TestMethod]
        public void Function_CreateCustomFunction_ShouldCreateFunction()
        {
            var function = MathOpsFactory.CreateFunction();
            Assert.IsNotNull(function);
            function.CreateCustomFunction("SomeFunction", null, null, "Perfomes Something", null, new Dev2CalculationManager());
        }

        [TestMethod]
        [ExpectedException(typeof(NullReferenceException))]
        public void GivenNullCalculationManager_Function_CreateCustomFunction_ShouldThrowException()
        {
            var function = MathOpsFactory.CreateFunction();
            Assert.IsNotNull(function);
            function.CreateCustomFunction("SomeFunction", null, null, "Perfomes Something", null, null);
        }
    }
}
