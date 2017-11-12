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
using Dev2.Data.MathOperations;
using Dev2.MathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Dev2.Common;

namespace Dev2.Tests.MathOperationTest
{
    /// <summary>
    /// Summary description for FunctionEvaluatorTest
    /// </summary>
    [TestClass]
    public class FunctionEvaluatorTest
    {
        private IFunctionEvaluator _eval = MathOpsFactory.CreateFunctionEvaluator();

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region TryEvaluateFunction Tests
        /// <summary>
        /// Tests that integer literals passed to the function evaluator with no data list regions are evaluated correctly.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_LiteralsPassedToFunction_EvaluationReturnsCorrectly()
        {
            const string expression = @"Sum(10, 10)";

            _eval = MathOpsFactory.CreateFunctionEvaluator();
            bool hasSuceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);
            if(hasSuceeded)
            {
                Assert.AreEqual(result, "20");
            }
            else
            {
                Assert.Fail("The Evaluation Manager was unable to resolve evaluation, this is a huge problem");
            }
        }

        /// <summary>
        /// Tests that invalid tokens passed to the function evaluator with no data list regions are evaluated as a syntax error
        /// and that the syntax error is correctly returned.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_InvalidExpression_ErrorPopulatedAndReturned()
        {
            const string expression = @"Sum(10, 10,asdasd)";

            _eval = MathOpsFactory.CreateFunctionEvaluator();
            bool hasSuceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);
            if(!hasSuceeded)
            {
                Assert.IsTrue(error.Length > 0);
            }
            else
            {
                Assert.Fail("The Function Evaluator did not correctly error on an invalid expression");
            }
        }

        /// <summary>
        /// Tests that parenthesis with no preceding function name with an invalid token are interpretted as a syntax error
        /// and that the error is correctly returned.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_NoExpression_ErrorPopulatedAndReturnedWithErrorDetailingProblem()
        {
            const string expression = @"(10, 10,asdasd)";


            bool hasSuceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);
            if (!hasSuceeded)
            {
                Assert.IsTrue(error.Length > 0);
            }
            else
            {
                Assert.Fail("The Function Evaluator did not correctly error on an invalid expression");
            }
        }

        /// <summary>
        /// Tests that a simple sequence of integer literals and binary integer operations are evaluated correctly by the function
        /// evaluator.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_UnaryOperation_Expected_SuccesfulUnaryOperation()
        {
            const string expression = @"10 + 10 - 10";

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);

            if (hasSucceeded)
            {
                Assert.AreEqual("10", result);
            }
        }

        /// <summary>
        /// Tests that a mixed expression containing binary integer literals and function calls evaluates as expected and that no error
        /// is encountered for an expected valid input expression.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_MixedUnaryAndFunctions_Expected_EvaluationSucessful()
        {
            const string expression = @"Average(10 + 10, 20*2, 30/2)";

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);

            if (hasSucceeded)
            {
                Assert.AreEqual("25", result);
            }
            else
            {
                Assert.Fail("Unable to resolve mixed unary and functions");
            }
        }

        /// <summary>
        /// Tests that an expression containing integer literals and function calls evaluates as an error given that the input expression is syntacticly correct
        /// but contains an unknown function identifier.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_FunctionDoesNotExist_Expected_ErrorResponseStatingFunctionNotExist()
        {
            const string expression = @"thisDoesNotExist(12,1234,567)";

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);

            if (!hasSucceeded)
            {
                Assert.IsTrue(error.Contains("Invalid function 'thisDoesNotExist'"));
            }
            else
            {
                Assert.Fail("Unexpected behaviour occurred during non-existant function evaluation");
            }
        }

        /// <summary>
        /// Tests that an expression that accesses the date capabilities of infrigistics evaluates correctly.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_DateFunction_Expected_EvaluationOfDateCorrect()
        {
            DateTime date = new DateTime(2012, 2, 2);
            const string expression = @"Date(2012,2,2)";
            string expected = date.ToString(GlobalConstants.Dev2DotNetDefaultDateTimeFormat);

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string actual, out string error);

            if (hasSucceeded)
            {
                Assert.IsTrue(actual.StartsWith(expected));
            }
            else
            {
                Assert.Fail("Date Calculation not being performed as expected");
            }
        }

        /// <summary>
        /// Tests that an expression that accesses the date capabilities of infrigistics mixed with string literal date format results in
        /// a valid evaluation.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_YearFunction_Expected_EvaluationOfDateCorrect()
        {
            const string expression = @"Year(""1989/02/01"")";

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);

            if (hasSucceeded)
            {
                Assert.AreEqual("1989", result);
            }
            else
            {
                Assert.Fail("Evaluator is unable to calculate year given the date");
            }
        }

        /// <summary>
        /// Tests that an expression that accesses the date capabilities of infrigistics mixed with string literal date format results in
        /// a valid evaluation.
        /// </summary>
        [TestMethod]
        public void FindFirstLetter_OfWord_Should_ReturnCorrectly()
        {
            string expression = "LEFT(\"Nkosinathi\",1)&IF(ISERROR(FIND(\" \",\"Nkosinathi\",1)),\"\",MID(\"Nkosinathi\",FIND(\" \",\"Nkosinathi\",1)+1,1))&IF(ISERROR(FIND(\" \",\"Nkosinathi\",FIND(\" \",\"Nkosinathi\",1)+1)),\"\",MID(\"Nkosinathi\",FIND(\" \",\"Nkosinathi\",FIND(\" \",\"Nkosinathi\",1)+1)+1,1))";

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);

            if (hasSucceeded)
            {
                Assert.AreEqual("N", result);
            }
            else
            {
                Assert.Fail("Evaluator is unable to find first letter of word");
            }

             expression = "LEFT(\"Nkosinathi Sangweni\",1)&IF(ISERROR(FIND(\" \",\"Nkosinathi Sangweni\",1)),\"\",MID(\"Nkosinathi Sangweni\",FIND(\" \",\"Nkosinathi Sangweni\",1)+1,1))&IF(ISERROR(FIND(\" \",\"Nkosinathi Sangweni\",FIND(\" \",\"Nkosinathi Sangweni\",1)+1)),\"\",MID(\"Nkosinathi Sangweni\",FIND(\" \",\"Nkosinathi Sangweni\",FIND(\" \",\"Nkosinathi Sangweni\",1)+1)+1,1))";

             hasSucceeded = _eval.TryEvaluateFunction(expression, out string result1, out string error1);

            if (hasSucceeded)
            {
                Assert.AreEqual("NS", result1);
            }
            else
            {
                Assert.Fail("Evaluator is unable to find first letter of word");
            }
        }

        /// <summary>
        /// Tests that the imaginary square root function of the infrigistics library evaluates correctly when used with
        /// a unary operator and an integer literal.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_ImSqrt_Expected_EvaluatioReturnsCorrectResult()
        {
            const string expression = @"Imsqrt(-1)";

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);

            if (hasSucceeded)
            {
                Assert.AreEqual("6.12303176911189E-17+i", result);
            }
            else
            {
                Assert.Fail("Imaginary SQRT did not evaluate correctly");
            }

        }

        /// <summary>
        /// 
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_Oct2Dec_Expected_EvaluationReturnsCorrectResult()
        {
            const string expression = @"Oct2Dec(764)";

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);

            if (hasSucceeded)
            {
                Assert.AreEqual("500", result);
            }
            else
            {
                Assert.Fail("Oct2Dec did not evaluate correctly");
            }

        }

        [TestMethod]
        public void TryEvaluateFunction_ComplexCalculation_Expected_EvaluatioReturnsCorrectResult()
        {
            const string expression = @"Sum(Average(Abs(-100), Min(10,20,2,30,200)), Max(200,300,400)) + 250";

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out string result, out string error);

            if (hasSucceeded)
            {
                Assert.AreEqual("701", result);
            }
            else
            {
                Assert.Fail("Oct2Dec did not evaluate correctly");
            }

        }

        #endregion TryEvaluateFunction Tests


    }
}
