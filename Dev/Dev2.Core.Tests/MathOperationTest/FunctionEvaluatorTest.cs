
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.MathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.MathOperationTest
{
    /// <summary>
    /// Summary description for FunctionEvaluatorTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
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
            string expression = @"Sum(10, 10)";
            string result;
            string error;

            _eval = MathOpsFactory.CreateFunctionEvaluator();
            bool hasSuceeded = _eval.TryEvaluateFunction(expression, out result, out error);
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
            string expression = @"Sum(10, 10,asdasd)";
            string result;
            string error;

            _eval = MathOpsFactory.CreateFunctionEvaluator();
            bool hasSuceeded = _eval.TryEvaluateFunction(expression, out result, out error);
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
            string expression = @"(10, 10,asdasd)";
            string result;
            string error;


            bool hasSuceeded = _eval.TryEvaluateFunction(expression, out result, out error);
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
        /// Tests that a simple sequence of integer literals and binary integer operations are evaluated correctly by the function
        /// evaluator.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_UnaryOperation_Expected_SuccesfulUnaryOperation()
        {
            string expression = @"10 + 10 - 10";

            string result;
            string error;

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out result, out error);

            if(hasSucceeded)
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
            string expression = @"Average(10 + 10, 20*2, 30/2)";
            string result;
            string error;

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out result, out error);

            if(hasSucceeded)
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
            string expression = @"thisDoesNotExist(12,1234,567)";
            string result;
            string error;

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out result, out error);

            if(!hasSucceeded)
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
            string expression = @"Date(2012,2,2)";
            string actual;
            string expected = date.ToShortDateString();
            string error;

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out actual, out error);

            if(hasSucceeded)
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
            string expression = @"Year(""1989/02/01"")";
            string result;
            string error;

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out result, out error);

            if(hasSucceeded)
            {
                Assert.AreEqual("1989", result);
            }
            else
            {
                Assert.Fail("Evaluator is unable to calculate year given the date");
            }
        }

        /// <summary>
        /// Tests that the imaginary square root function of the infrigistics library evaluates correctly when used with
        /// a unary operator and an integer literal.
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunction_ImSqrt_Expected_EvaluatioReturnsCorrectResult()
        {
            string expression = @"Imsqrt(-1)";
            string result;
            string error;

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out result, out error);

            if(hasSucceeded)
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
            string expression = @"Oct2Dec(764)";
            string result;
            string error;

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out result, out error);

            if(hasSucceeded)
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
            string expression = @"Sum(Average(Abs(-100), Min(10,20,2,30,200)), Max(200,300,400)) + 250";
            string result;
            string error;

            bool hasSucceeded = _eval.TryEvaluateFunction(expression, out result, out error);

            if(hasSucceeded)
            {
                Assert.AreEqual("701", result);
            }
            else
            {
                Assert.Fail("Oct2Dec did not evaluate correctly");
            }

        }

        #endregion TryEvaluateFunction Tests

        #region TryEvaluateAtomicFunction Tests

        /// <summary>
        ///  Try Evaluate Atomic Function with complex function expected function evaluated
        /// </summary>
        [TestMethod]
        public void TryEvaluateAtomicFunction_ComplexCalculation_Expected_EvaluatioReturnsCorrectResult()
        {
            string expression = @"Sum(Average(Abs(-100), Min(10,20,2,30,200)), Max(200,300,400)) + 250";
            string result;
            string error;
            bool hasSucceeded = new FunctionEvaluator().TryEvaluateAtomicFunction(expression, out result, out error);


            if(hasSucceeded)
            {
                Assert.AreEqual("701", result);
            }
            else
            {
                Assert.Fail("Oct2Dec did not evaluate correctly");
            }

        }

        /// <summary>
        /// Try Evaluate Atomic Function with empty function expected error message populated
        /// </summary>
        [TestMethod]
        public void TryEvaluateAtomicFunction_EmptyFunction_Expected_EvaluatioReturnsCorrectResult()
        {
            string expression = @"";
            string result;
            string error;
            bool hasSucceeded = new FunctionEvaluator().TryEvaluateAtomicFunction(expression, out result, out error);

            Assert.IsTrue(!string.IsNullOrEmpty(error));

        }

        /// <summary>
        /// Try Evaluate Atomic Function with invalid function expected error message populated
        /// </summary>
        [TestMethod]
        public void TryEvaluateAtomicFunction_InvalidFunction_Expected_EvaluatioReturnsCorrectResult()
        {
            string expression = @"abcdefg";
            string result;
            string error;
            bool hasSucceeded = new FunctionEvaluator().TryEvaluateAtomicFunction(expression, out result, out error);

            Assert.IsTrue(!string.IsNullOrEmpty(error));

        }


        #endregion TryEvaluateAtomicFunction Tests

        #region TryEvaluateFunction<T> Tests

        /// <summary>
        ///  Try Evaluate Atomic Function with complex function expected function evaluated
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunctionType_ComplexCalculation_Expected_EvaluatioReturnsCorrectResult()
        {
            string expression = @"Sum";
            List<int> values = new List<int> { 10, 20, 30 };
            string result;
            string error;
            bool hasSucceeded = _eval.TryEvaluateFunction(values, expression, out result, out error);

            if(hasSucceeded)
            {
                Assert.AreEqual("60", result);
            }
            else
            {
                Assert.Fail("Oct2Dec did not evaluate correctly");
            }

        }

        /// <summary>
        /// Try Evaluate Atomic Function with empty function expected error message populated
        /// </summary>
        [TestMethod]
        public void TryEvaluateFunctionType_EmptyFunction_Expected_EvaluatioReturnsCorrectResult()
        {
            string expression = @"";
            List<int> values = new List<int> { 10, 20, 30 };
            string result;
            string error;
            bool hasSucceeded = _eval.TryEvaluateFunction(values, expression, out result, out error);

            Assert.IsTrue(!string.IsNullOrEmpty(error) && !hasSucceeded);

        }

        /// <summary>
        /// Try Evaluate Atomic Function with invalid function expected error message populated
        /// </summary>
        [TestMethod]
        public void TryEvaluateType_InvalidFunction_Expected_EvaluatioReturnsCorrectResult()
        {
            string expression = @"abcdefg";
            List<int> values = new List<int> { 10, 20, 30 };
            string result;
            string error;
            bool hasSucceeded = _eval.TryEvaluateFunction(values, expression, out result, out error);

            Assert.IsTrue(!string.IsNullOrEmpty(error) && !hasSucceeded);

        }

        /// <summary>
        /// Try Evaluate Atomic Function with invalid function expected error message populated
        /// </summary>
        [TestMethod]
        public void TryEvaluateType_EmptyList_Expected_EvaluatioReturnsCorrectResult()
        {
            string expression = @"Sum";
            List<int> values = new List<int>();
            string result;
            string error;
            bool hasSucceeded = _eval.TryEvaluateFunction(values, expression, out result, out error);

            Assert.IsTrue(!string.IsNullOrEmpty(error) & !hasSucceeded);

        }

        #endregion TryEvaluateFunction<T> Tests

    }
}
