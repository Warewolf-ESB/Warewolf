/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2026 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System.Globalization;
using Dev2.Data.MathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.MathOperationTest
{
    [TestClass]
    [DoNotParallelize]
    public class FunctionEvaluatorCultureTests
    {
        CultureInfo _original;

        [TestInitialize]
        public void Setup()
        {
            _original = CultureInfo.CurrentCulture;
            CultureInfo.CurrentCulture = new CultureInfo("en-ZA");
        }

        [TestCleanup]
        public void Cleanup() => CultureInfo.CurrentCulture = _original;

        [TestMethod]
        [TestCategory("UnitTest")]
        [DataRow("ROUND(123.45*100,0)", "12345")]
        [DataRow("ROUND(\"123.45\"*100,0)", "12345")]
        [DataRow("10/4", "2.5")]
        [DataRow("ROUND(0.5,1)", "0.5")]
        public void TryEvaluateFunction_DecimalCommaCulture_UsesInvariantNumbers(string expression, string expected)
        {
            Assert.AreEqual(",", CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator);

            var evaluator = MathOpsFactory.CreateFunctionEvaluator();
            var ok = evaluator.TryEvaluateFunction(expression, out var result, out var error);

            Assert.IsTrue(ok, error);
            Assert.AreEqual(expected, result);
        }
    }
}
