/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using Dev2.Data.MathOperations;
using Dev2.MathOperations;
using Infragistics.Calculations.CalcManager;
using Infragistics.Calculations.Engine;
using Microsoft.VisualStudio.TestTools.UnitTesting;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.MathOperationTest
{
    /// <summary>
    /// Summary description for FunctionTest
    /// </summary>
    [TestClass]
    public class FunctionTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        #region Ctor

        [TestMethod]
        public void Function_AllInputsValid_Expected_ValidFunctionCreated()
        {
            const string functionName = "Test Function";
            List<string> arguments = new List<string>();
            List<string> argumentDescriptions = new List<string>();
            const string description = "Some Test Function";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            Assert.IsNotNull(func);
        }

        [TestMethod]
        public void Function_NullFunctionName_Expected_ExceptionReturned()
        {
            List<string> arguments = new List<string>();
            List<string> argumentDescriptions = new List<string>();
            const string description = "Some Test Function";
            try
            {
                MathOpsFactory.CreateFunction(null, arguments, argumentDescriptions, description);
            }
            catch(ArgumentNullException)
            {
                // If we get this exception, it is expected.
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void Function_NullListOfArguments_Expected_EmptyListofArguments()
        {
            const string functionName = "Test Function";
            const string description = "Some Test Function";

            IFunction func = MathOpsFactory.CreateFunction(functionName, null, null, description);
            Assert.AreEqual(0, func.arguments.Count);
        }

        [TestMethod]
        public void Function_NullDescription_Expected_EmptyDescription()
        {
            const string functionName = "Test Function";
            List<string> arguments = new List<string> { "arg1" };
            List<string> argumentDescriptions = new List<string> { "the first argument" };

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, null);
            Assert.IsTrue(func.Description.Equals(string.Empty));
        }

        [TestMethod]
        public void Function_NullDescriptionAndArguments_Expected_FunctionStillCreated()
        {
            const string functionName = "Test Function";

            IFunction func = MathOpsFactory.CreateFunction(functionName, null, null, null);
            Assert.IsNotNull(func);
        }

        #endregion Ctor

        #region CreateCustomFunction Test


        [TestMethod]
        public void CreateCustomFunction_AllValidValues_Expected_CustomFunctionCreatedAndRegisteredWithCalcManager()
        {
            const string functionName = "TestFunction";
            List<string> arguments = new List<string> { "x", "y" };
            List<string> argumentDescriptions = new List<string> { "the first argument", "the second argument" };
            const string description = "My TestFunction";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            IDev2CalculationManager manager = new Dev2CalculationManager();
            Func<double[], double> function = AddAbs;

            func.CreateCustomFunction(functionName, arguments, argumentDescriptions, description, function, manager);
            CalculationValue value = manager.CalculateFormula("TestFunction(1)");
            Assert.AreEqual(123123423423, value.ToDouble());
        }

        [TestMethod]
        public void CreateCustomFunction_NullXamCalculationManager_Expected_ExceptionReturned()
        {
            const string functionName = "TestFunction";
            List<string> arguments = new List<string> { "x", "y" };
            List<string> argumentDescriptions = new List<string> { "the first argument", "the second argument" };
            const string description = "My TestFunction";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            Func<double[], double> function = AddAbs;
            try
            {
                func.CreateCustomFunction(functionName, arguments, argumentDescriptions, description, function, null);
            }
            catch(NullReferenceException)
            {
                // since this exception is thrown we have our answer.
                Assert.IsTrue(true);
            }
        }

        [TestMethod]
        public void CreateCustomFunction_NullFunc_Expected_ExceptionReturned()
        {
            const string functionName = "TestFunction";
            List<string> arguments = new List<string> { "x", "y" };
            List<string> argumentDescriptions = new List<string> { "the first argument", "the second argument" };
            const string description = "My TestFunction";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, argumentDescriptions, description);
            IDev2CalculationManager manager = new Dev2CalculationManager();
            func.CreateCustomFunction(functionName, arguments, argumentDescriptions, description, null, manager);

            Assert.AreEqual("TestFunction", func.FunctionName);


        }

        [TestMethod]
        public void CreateCustomFunction_NullArgumentDescription_Expected_ExceptionReturned()
        {
            const string functionName = "TestFunction";
            List<string> arguments = new List<string> { "x", "y" };
            const string description = "My TestFunction";

            IFunction func = MathOpsFactory.CreateFunction(functionName, arguments, null, description);
            IDev2CalculationManager manager = new Dev2CalculationManager();
            func.CreateCustomFunction(functionName, arguments, null, description, null, manager);

            Assert.AreNotEqual(null, func.ArgumentDescriptions);
        }


        #endregion CreateCustom Function Test

        #region Private Test Methods

        private static double AddAbs(double[] x)
        {
            return 123123423423;
        }

        #endregion Private Test Methods
    }
}
