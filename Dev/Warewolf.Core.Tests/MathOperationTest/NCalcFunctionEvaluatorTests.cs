/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2024 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*/

using System;
using Dev2.Data.MathOperations;
using Dev2.MathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.MathOperationTest
{
    /// <summary>
    /// Comprehensive NCalc cross-platform coverage for all Infragistics parity functions.
    /// Organised by function category matching <c>UltraCalcFunctionFactory</c> constructor.
    /// </summary>
    [TestClass]
    public class NCalcFunctionEvaluatorTests
    {
        private static IFunctionEvaluator Eval() => MathOpsFactory.CreateFunctionEvaluator();

        private static void AssertEval(string expression, string expected)
        {
            var eval = Eval();
            var ok = eval.TryEvaluateFunction(expression, out var result, out var error);
            Assert.IsTrue(ok, $"Expression '{expression}' failed: {error}");
            Assert.AreEqual(expected, result, $"Expression '{expression}'");
        }

        private static void AssertEvalApprox(string expression, double expected, double tolerance = 1e-6)
        {
            var eval = Eval();
            var ok = eval.TryEvaluateFunction(expression, out var result, out var error);
            Assert.IsTrue(ok, $"Expression '{expression}' failed: {error}");
            Assert.IsTrue(double.TryParse(result, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var actual),
                $"Result '{result}' is not a number.");
            Assert.AreEqual(expected, actual, tolerance, $"Expression '{expression}'");
        }

        private static void AssertError(string expression)
        {
            var eval = Eval();
            var ok = eval.TryEvaluateFunction(expression, out _, out var error);
            Assert.IsFalse(ok, $"Expression '{expression}' should have failed.");
            Assert.IsTrue(error.Length > 0, "Error message should not be empty.");
        }

        #region Math — Aggregates

        [TestMethod] public void Sum_MultipleValues() => AssertEval("SUM(1,2,3,4,5)", "15");
        [TestMethod] public void Sum_Single()          => AssertEval("SUM(42)", "42");
        [TestMethod] public void Average_Basic()       => AssertEval("AVERAGE(10,20,30)", "20");
        [TestMethod] public void Min_Basic()           => AssertEval("MIN(5,3,8,1,7)", "1");
        [TestMethod] public void Max_Basic()           => AssertEval("MAX(5,3,8,1,7)", "8");
        [TestMethod] public void Count_Basic()         => AssertEval("COUNT(1,2,3,4,5)", "5");
        [TestMethod] public void Median_Odd()          => AssertEval("MEDIAN(1,2,3,4,5)", "3");
        [TestMethod] public void Median_Even()         => AssertEval("MEDIAN(1,2,3,4)", "2.5");
        [TestMethod] public void Subtotal_Sum()        => AssertEval("SUBTOTAL(9,10,20,30)", "60");

        [TestMethod]
        public void Stdev_Basic()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("STDEV(2,4,4,4,5,5,7,9)", out var r, out _);
            Assert.IsTrue(double.TryParse(r, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d));
            Assert.AreEqual(2.138, d, 0.001);
        }

        [TestMethod]
        public void AveDev_Basic()
        {
            AssertEvalApprox("AVEDEV(4,5,6,7,8)", 1.2, 0.01);
        }

        #endregion

        #region Math — Rounding

        [TestMethod] public void Round_TwoDigits()   => AssertEval("ROUND(3.14159,2)", "3.14");
        [TestMethod] public void RoundDown_Basic()   => AssertEval("ROUNDDOWN(3.9,0)", "3");
        [TestMethod] public void RoundUp_Basic()     => AssertEval("ROUNDUP(3.1,0)", "4");
        [TestMethod] public void Floor_Basic()       => AssertEval("FLOOR(3.7,1)", "3");
        [TestMethod] public void Ceiling_Basic()     => AssertEval("CEILING(3.2,1)", "4");
        [TestMethod] public void Trunc_Basic()       => AssertEval("TRUNC(3.9)", "3");
        [TestMethod] public void Int_Negative()      => AssertEval("INT(-2.3)", "-3");
        [TestMethod] public void Even_Basic()        => AssertEval("EVEN(3)", "4");
        [TestMethod] public void Odd_Basic()         => AssertEval("ODD(4)", "5");
        [TestMethod] public void MRound_Basic()      => AssertEval("MROUND(10,3)", "9");

        #endregion

        #region Math — Arithmetic

        [TestMethod] public void Mod_Basic()       => AssertEval("MOD(10,3)", "1");
        [TestMethod] public void Quotient_Basic()  => AssertEval("QUOTIENT(10,3)", "3");
        [TestMethod] public void Product_Basic()   => AssertEval("PRODUCT(2,3,4)", "24");
        [TestMethod] public void Power_Basic()     => AssertEval("POWER(2,10)", "1024");
        [TestMethod] public void Abs_Negative()    => AssertEval("ABS(-5)", "5");
        [TestMethod] public void Sign_Negative()   => AssertEval("SIGN(-42)", "-1");
        [TestMethod] public void Sign_Zero()       => AssertEval("SIGN(0)", "0");
        [TestMethod] public void Sqrt_Basic()      => AssertEval("SQRT(9)", "3");
        [TestMethod] public void SqrtPi_Basic()    => AssertEvalApprox("SQRTPI(1)", Math.Sqrt(Math.PI));
        [TestMethod] public void Exp_Basic()       => AssertEvalApprox("EXP(1)", Math.E);
        [TestMethod] public void Ln_Basic()        => AssertEvalApprox("LN(1)", 0);
        [TestMethod] public void Log_Base10()      => AssertEvalApprox("LOG(100,10)", 2);
        [TestMethod] public void Log10_Basic()     => AssertEvalApprox("LOG10(1000)", 3);
        [TestMethod] public void Pi_Basic()        => AssertEvalApprox("PI()", Math.PI);

        #endregion

        #region Math — Trigonometry

        [TestMethod] public void Sin_Zero()    => AssertEval("SIN(0)", "0");
        [TestMethod] public void Cos_Zero()    => AssertEval("COS(0)", "1");
        [TestMethod] public void Tan_Zero()    => AssertEval("TAN(0)", "0");
        [TestMethod] public void Degrees_Pi()  => AssertEvalApprox("DEGREES(PI())", 180);
        [TestMethod] public void Radians_180() => AssertEvalApprox("RADIANS(180)", Math.PI);
        [TestMethod] public void Atan2_Basic() => AssertEvalApprox("ATAN2(1,1)", Math.PI / 4);

        #endregion

        #region Math — Combinatorics

        [TestMethod] public void Fact_Five()        => AssertEval("FACT(5)", "120");
        [TestMethod] public void FactDouble_Seven() => AssertEval("FACTDOUBLE(7)", "105");
        [TestMethod] public void Combin_Basic()     => AssertEval("COMBIN(5,2)", "10");
        [TestMethod] public void Gcd_Basic()        => AssertEval("GCD(12,8)", "4");
        [TestMethod] public void Lcm_Basic()        => AssertEval("LCM(4,6)", "12");
        [TestMethod] public void Delta_Equal()      => AssertEval("DELTA(3,3)", "1");
        [TestMethod] public void Delta_NotEqual()   => AssertEval("DELTA(3,4)", "0");
        [TestMethod] public void GeStep_Above()     => AssertEval("GESTEP(5,3)", "1");
        [TestMethod] public void GeStep_Below()     => AssertEval("GESTEP(2,3)", "0");

        #endregion

        #region Math — Misc

        [TestMethod]
        public void RandBetween_InRange()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("RANDBETWEEN(1,100)", out var r, out _);
            Assert.IsTrue(double.TryParse(r, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out var d));
            Assert.IsTrue(d >= 1 && d <= 100);
        }

        [TestMethod] public void Roman_Basic() => AssertEval("ROMAN(2024)", "MMXXIV");
        [TestMethod] public void Roman_Zero()  => AssertEval("ROMAN(0)", "");

        [TestMethod]
        public void ComplexCalculation_NestedFunctions()
        {
            AssertEval("SUM(AVERAGE(ABS(-100), MIN(10,20,2,30,200)), MAX(200,300,400)) + 250", "701");
        }

        #endregion

        #region String Functions

        [TestMethod] public void Left_Basic()       => AssertEval("LEFT(\"Hello\",3)", "Hel");
        [TestMethod] public void Right_Basic()      => AssertEval("RIGHT(\"Hello\",3)", "llo");
        [TestMethod] public void Mid_Basic()        => AssertEval("MID(\"Hello\",2,3)", "ell");
        [TestMethod] public void Len_Basic()        => AssertEval("LEN(\"Hello\")", "5");
        [TestMethod] public void Trim_Spaces()      => AssertEval("TRIM(\"  hello  world  \")", "hello world");
        [TestMethod] public void Lower_Basic()      => AssertEval("LOWER(\"HELLO\")", "hello");
        [TestMethod] public void Upper_Basic()      => AssertEval("UPPER(\"hello\")", "HELLO");
        [TestMethod] public void Concatenate_Basic() => AssertEval("CONCATENATE(\"Hello\",\" \",\"World\")", "Hello World");
        [TestMethod] public void Value_Basic()      => AssertEval("VALUE(\"3.14\")", "3.14");
        [TestMethod] public void Rept_Basic()       => AssertEval("REPT(\"ab\",3)", "ababab");
        [TestMethod] public void Char_Basic()       => AssertEval("CHAR(65)", "A");
        [TestMethod] public void Code_Basic()       => AssertEval("CODE(\"A\")", "65");
        [TestMethod] public void Exact_Equal()      => AssertEval("EXACT(\"abc\",\"abc\")", "True");
        [TestMethod] public void Exact_NotEqual()   => AssertEval("EXACT(\"Abc\",\"abc\")", "False");

        [TestMethod]
        public void Find_Basic() => AssertEval("FIND(\"l\",\"Hello\",1)", "3");

        [TestMethod]
        public void Search_CaseInsensitive() => AssertEval("SEARCH(\"hello\",\"Hello World\")", "1");

        [TestMethod]
        public void Replace_Basic() => AssertEval("REPLACE(\"Hello World\",7,5,\"NCalc\")", "Hello NCalc");

        [TestMethod]
        public void Substitute_Basic() => AssertEval("SUBSTITUTE(\"Hello World\",\"World\",\"NCalc\")", "Hello NCalc");

        [TestMethod]
        public void IsError_WithError()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("ISERROR(FIND(\"x\",\"Hello\"))", out var r, out _);
            Assert.AreEqual("True", r);
        }

        [TestMethod]
        public void IsError_WithoutError()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("ISERROR(FIND(\"H\",\"Hello\"))", out var r, out _);
            Assert.AreEqual("False", r);
        }

        [TestMethod]
        public void FindFirstLetter_SingleWord()
        {
            const string expr = "LEFT(\"Nkosinathi\",1)&IF(ISERROR(FIND(\" \",\"Nkosinathi\",1)),\"\",MID(\"Nkosinathi\",FIND(\" \",\"Nkosinathi\",1)+1,1))&IF(ISERROR(FIND(\" \",\"Nkosinathi\",FIND(\" \",\"Nkosinathi\",1)+1)),\"\",MID(\"Nkosinathi\",FIND(\" \",\"Nkosinathi\",FIND(\" \",\"Nkosinathi\",1)+1)+1,1))";
            AssertEval(expr, "N");
        }

        [TestMethod]
        public void FindFirstLetter_TwoWords()
        {
            const string expr = "LEFT(\"Nkosinathi Sangweni\",1)&IF(ISERROR(FIND(\" \",\"Nkosinathi Sangweni\",1)),\"\",MID(\"Nkosinathi Sangweni\",FIND(\" \",\"Nkosinathi Sangweni\",1)+1,1))&IF(ISERROR(FIND(\" \",\"Nkosinathi Sangweni\",FIND(\" \",\"Nkosinathi Sangweni\",1)+1)),\"\",MID(\"Nkosinathi Sangweni\",FIND(\" \",\"Nkosinathi Sangweni\",FIND(\" \",\"Nkosinathi Sangweni\",1)+1)+1,1))";
            AssertEval(expr, "NS");
        }

        #endregion

        #region DateTime Functions

        [TestMethod]
        public void Date_Basic()
        {
            var eval = Eval();
            var ok = eval.TryEvaluateFunction("DATE(2024,6,15)", out var r, out _);
            Assert.IsTrue(ok);
            Assert.IsTrue(r.Contains("2024"));
        }

        [TestMethod]
        public void Year_FromString() => AssertEval("YEAR(\"1989/02/01\")", "1989");

        [TestMethod]
        public void Month_FromDate()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("MONTH(\"2024-06-15\")", out var r, out _);
            Assert.AreEqual("6", r);
        }

        [TestMethod]
        public void Day_FromDate()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("DAY(\"2024-06-15\")", out var r, out _);
            Assert.AreEqual("15", r);
        }

        [TestMethod]
        public void Days360_Basic()
        {
            // US method: 2024-01-01 to 2024-07-01 = 180 days in 360-day year
            var eval = Eval();
            eval.TryEvaluateFunction("DAYS360(\"2024-01-01\",\"2024-07-01\")", out var r, out _);
            Assert.AreEqual("180", r);
        }

        [TestMethod]
        public void WeekDay_Sunday()
        {
            // 2024-06-02 is Sunday. returnType=1 → Sunday=1
            var eval = Eval();
            eval.TryEvaluateFunction("WEEKDAY(\"2024-06-02\",1)", out var r, out _);
            Assert.AreEqual("1", r);
        }

        [TestMethod]
        public void DateAdd_Month()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("DATEADD(\"month\",1,\"2024-01-15\")", out var r, out _);
            Assert.IsTrue(r.Contains("2024"));
        }

        [TestMethod]
        public void DateDiff_Days()
        {
            AssertEval("DATEDIFF(\"day\",\"2024-01-01\",\"2024-01-11\")", "10");
        }

        [TestMethod]
        public void EDate_Basic()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("EDATE(\"2024-01-15\",1)", out var r, out _);
            Assert.IsTrue(r.Contains("2024"));
        }

        #endregion

        #region Financial Functions

        [TestMethod]
        public void Sln_Basic() => AssertEvalApprox("SLN(10000,1000,5)", 1800);

        [TestMethod]
        public void Syd_Year1() => AssertEvalApprox("SYD(10000,1000,5,1)", 3000);

        [TestMethod]
        public void Pmt_Basic()
        {
            // PMT(5%/12, 60 months, 10000)
            AssertEvalApprox("PMT(0.05/12,60,10000)", -188.71, 0.01);
        }

        [TestMethod]
        public void Fv_Basic()
        {
            // FV(6%/12, 10, -200, -500) ≈ 2571.18 (type=0, end of period)
            AssertEvalApprox("FV(0.06/12,10,-200,-500)", 2571.18, 0.1);
        }

        [TestMethod]
        public void Pv_Basic()
        {
            // PV(8%/12, 48, 500) ≈ -20,480.96
            AssertEvalApprox("PV(0.08/12,48,500)", -20480.96, 0.1);
        }

        [TestMethod]
        public void Npv_Basic()
        {
            // NPV(10%, -10000, 3000, 4200, 6800) ≈ 1188.44
            AssertEvalApprox("NPV(0.1,-10000,3000,4200,6800)", 1188.44, 0.1);
        }

        [TestMethod]
        public void Nper_Basic()
        {
            // NPER(12%/12, -100, 1000) ≈ 10.58
            AssertEvalApprox("NPER(0.12/12,-100,1000)", 10.58, 0.1);
        }

        #endregion

        #region Base Conversion Functions

        [TestMethod] public void Dec2Bin_Basic() => AssertEval("DEC2BIN(10)", "1010");
        [TestMethod] public void Dec2Hex_Basic() => AssertEval("DEC2HEX(255)", "FF");
        [TestMethod] public void Dec2Oct_Basic() => AssertEval("DEC2OCT(8)", "10");
        [TestMethod] public void Bin2Dec_Basic() => AssertEval("BIN2DEC(1010)", "10");
        [TestMethod] public void Hex2Dec_Basic() => AssertEval("HEX2DEC(\"FF\")", "255");
        [TestMethod] public void Oct2Dec_Basic() => AssertEval("OCT2DEC(764)", "500");
        [TestMethod] public void Bin2Oct_Basic() => AssertEval("BIN2OCT(1010)", "12");
        [TestMethod] public void Bin2Hex_Basic() => AssertEval("BIN2HEX(1010)", "A");
        [TestMethod] public void Oct2Bin_Basic() => AssertEval("OCT2BIN(12)", "1010");
        [TestMethod] public void Oct2Hex_Basic() => AssertEval("OCT2HEX(12)", "A");
        [TestMethod] public void Hex2Bin_Basic() => AssertEval("HEX2BIN(\"A\")", "1010");
        [TestMethod] public void Hex2Oct_Basic() => AssertEval("HEX2OCT(\"A\")", "12");

        #endregion

        #region Complex Functions

        [TestMethod]
        public void ImSqrt_NegativeOne()
        {
            var eval = Eval();
            eval.TryEvaluateFunction("IMSQRT(-1)", out var r, out _);
            Assert.AreEqual("i", r);
        }

        [TestMethod]
        public void Complex_Basic()
        {
            AssertEval("COMPLEX(3,4)", "3+4i");
        }

        [TestMethod]
        public void ImAbs_ThreeFourFive()
        {
            AssertEvalApprox("IMABS(\"3+4i\")", 5.0);
        }

        [TestMethod]
        public void ImReal_Basic()
        {
            AssertEvalApprox("IMREAL(\"3+4i\")", 3.0);
        }

        [TestMethod]
        public void Imaginary_Basic()
        {
            AssertEvalApprox("IMAGINARY(\"3+4i\")", 4.0);
        }

        [TestMethod]
        public void ImSum_Basic()
        {
            AssertEval("IMSUM(\"1+2i\",\"3+4i\")", "4+6i");
        }

        [TestMethod]
        public void ImSub_Basic()
        {
            AssertEval("IMSUB(\"3+4i\",\"1+2i\")", "2+2i");
        }

        #endregion

        #region Information / Logical Functions

        [TestMethod] public void And_TrueTrue()    => AssertEval("AND(1=1,2=2)", "True");
        [TestMethod] public void And_TrueFalse()   => AssertEval("AND(1=1,1=2)", "False");
        [TestMethod] public void Or_FalseTrue()    => AssertEval("OR(1=2,2=2)", "True");
        [TestMethod] public void Or_FalseFalse()   => AssertEval("OR(1=2,1=3)", "False");
        [TestMethod] public void Not_False()       => AssertEval("NOT(1=2)", "True");
        [TestMethod] public void TrueFn()          => AssertEval("TRUE()", "True");
        [TestMethod] public void FalseFn()         => AssertEval("FALSE()", "False");
        [TestMethod] public void IsOdd_Three()     => AssertEval("ISODD(3)", "True");
        [TestMethod] public void IsEven_Four()     => AssertEval("ISEVEN(4)", "True");
        [TestMethod] public void IsNumber_Valid()  => AssertEval("ISNUMBER(42)", "True");
        [TestMethod] public void IsText_Valid()    => AssertEval("ISTEXT(\"hello\")", "True");
        [TestMethod] public void IsBlank_Empty()   => AssertEval("ISBLANK(\"\")", "True");
        [TestMethod] public void IsBlank_NonEmpty() => AssertEval("ISBLANK(\"x\")", "False");

        [TestMethod]
        public void IfError_WithError()
        {
            // Division by zero → error → returns fallback
            var eval = Eval();
            eval.TryEvaluateFunction("IFERROR(1/0,\"error\")", out var r, out _);
            Assert.AreEqual("error", r);
        }

        [TestMethod]
        public void Choose_Basic()
        {
            AssertEval("CHOOSE(2,\"a\",\"b\",\"c\")", "b");
        }

        [TestMethod]
        public void N_Boolean()
        {
            AssertEval("N(TRUE())", "1");
        }

        [TestMethod]
        public void FunctionDoesNotExist_Error()
        {
            AssertError("thisDoesNotExist(12,1234,567)");
        }

        #endregion
    }
}
