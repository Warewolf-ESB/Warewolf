/*
*  Warewolf - Once bitten, there's no going back
*/
using Dev2.MathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Runtime.Tests.Coverage
{
    [TestClass]
    public class NCalcExtendedCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "NCalcExtended";

        static FunctionEvaluator E => new FunctionEvaluator();

        static void Ok(string expr)
        {
            var ok = E.TryEvaluateFunction(expr, out var _, out var err);
            Assert.IsTrue(ok, $"{expr} -> {err}");
        }

        static void Try(string expr) { try { E.TryEvaluateFunction(expr, out _, out _); } catch { } }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Math_Aggregates() { Ok("SUM(1,2,3,4)"); Ok("AVERAGE(1,2,3)"); Ok("MIN(5,3,7)"); Ok("MAX(5,3,7)"); Ok("COUNT(1,2,3)"); Ok("COUNTA(1,'a',3)"); Ok("MEDIAN(1,2,3,4,5)"); Ok("VAR(1,2,3,4,5)"); Ok("STDEV(1,2,3,4,5)"); Ok("AVEDEV(1,2,3,4,5)"); Try("SUBTOTAL(9,1,2,3)"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Math_Rounding() { Ok("ROUND(3.14159, 2)"); Ok("ROUNDDOWN(3.7, 0)"); Ok("ROUNDUP(3.1, 0)"); Ok("FLOOR(3.7, 1)"); Ok("CEILING(3.1, 1)"); Ok("TRUNC(3.7)"); Ok("INT(3.7)"); Ok("EVEN(3)"); Ok("ODD(2)"); Ok("MROUND(10, 3)"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Math_Algebra() { Ok("MOD(10, 3)"); Ok("QUOTIENT(10, 3)"); Ok("PRODUCT(2, 3, 4)"); Ok("POWER(2, 8)"); Ok("ABS(-5)"); Ok("SIGN(-5)"); Ok("SQRT(16)"); Ok("SQRTPI(2)"); Ok("EXP(1)"); Ok("LN(2.718281828)"); Ok("LOG(100, 10)"); Ok("LOG10(1000)"); Ok("PI()"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Math_Trig() { Ok("SIN(0)"); Ok("COS(0)"); Ok("TAN(0)"); Ok("ASIN(0)"); Ok("ACOS(1)"); Ok("ATAN(0)"); Ok("ATAN2(1, 1)"); Ok("SINH(0)"); Ok("COSH(0)"); Ok("TANH(0)"); Ok("ASINH(0)"); Ok("ACOSH(1)"); Ok("ATANH(0)"); Ok("DEGREES(3.14159)"); Ok("RADIANS(180)"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Math_Combinatorics() { Ok("FACT(5)"); Ok("FACTDOUBLE(5)"); Ok("COMBIN(5, 2)"); Ok("MULTINOMIAL(2, 3)"); Ok("GCD(12, 18)"); Ok("LCM(4, 6)"); Ok("DELTA(1, 1)"); Ok("GESTEP(5, 3)"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Math_RandAndSeries() { Ok("RAND()"); Ok("RANDBETWEEN(1, 100)"); Ok("SERIESSUM(2, 0, 1, 1, 2, 3)"); Ok("ROMAN(4)"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Math_Convert() { Try("CONVERT(1, 'lbm', 'kg')"); Try("CONVERT(100, 'cm', 'm')"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void String_Basic() { Ok("LEFT('hello', 2)"); Ok("RIGHT('hello', 2)"); Ok("MID('hello', 2, 2)"); Ok("LEN('hello')"); Ok("TRIM('  hi  ')"); Ok("LOWER('HELLO')"); Ok("UPPER('hello')"); Ok("CONCATENATE('a', 'b', 'c')"); Ok("VALUE('42')"); Try("TEXT(123, '0.00')"); Try("REPLACE('hello', 1, 2, 'X')"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void String_Search() { Ok("FIND('l', 'hello')"); Ok("SEARCH('l', 'hello')"); Ok("SEARCHB('l', 'hello')"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void String_Misc() { Ok("CHAR(65)"); Ok("CODE('A')"); Ok("REPT('a', 3)"); Try("FIXED(1234.5678, 2)"); Try("ISERROR(1/0)"); Ok("EXACT('a', 'a')"); Ok("PROPER('hello world')"); Ok("SUBSTITUTE('hello', 'l', 'L')"); Ok("T('hi')"); Ok("CLEAN('hi')"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Info_Logical() { Ok("AND(true, true)"); Ok("OR(true, false)"); Ok("NOT(false)"); Ok("TRUE()"); Ok("FALSE()"); Ok("IF(true, 1, 2)"); Try("IFERROR(1/0, 0)"); Ok("CHOOSE(2, 'a', 'b', 'c')"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Info_Type() { Try("TYPE(1)"); Ok("N(5)"); Try("NA()"); Try("INFO('directory')"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Info_IsChecks() { Try("ISBLANK('')"); Try("ISERR(1/0)"); Ok("ISLOGICAL(true)"); Try("ISNA('x')"); Ok("ISNONTEXT(123)"); Ok("ISNUMBER(123)"); Ok("ISREF('x')"); Ok("ISTEXT('hi')"); Ok("ISODD(3)"); Ok("ISEVEN(2)"); Try("ISNULL('x')"); Try("ISDBNULL('x')"); Try("NULL()"); Try("DBNULL()"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Complex_Basic() { Ok("COMPLEX(3, 4)"); Ok("IMABS('3+4i')"); Ok("IMAGINARY('3+4i')"); Ok("IMREAL('3+4i')"); Ok("IMARGUMENT('3+4i')"); Ok("IMCONJUGATE('3+4i')"); Ok("IMCOS('3+4i')"); Ok("IMSIN('3+4i')"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void Complex_Ops() { Ok("IMDIV('3+4i', '1+2i')"); Ok("IMPRODUCT('3+4i', '1+2i')"); Ok("IMSUM('3+4i', '1+2i')"); Ok("IMSUB('3+4i', '1+2i')"); Ok("IMEXP('3+4i')"); Ok("IMLN('3+4i')"); Ok("IMLOG10('3+4i')"); Ok("IMLOG2('3+4i')"); Ok("IMSQRT('3+4i')"); Ok("IMPOWER('3+4i', 2)"); }

        [TestMethod, Owner(Owner), TestCategory(Cat)] public void BaseConversion_All() { Ok("DEC2BIN(10)"); Ok("DEC2HEX(255)"); Ok("DEC2OCT(8)"); Ok("BIN2DEC('1010')"); Ok("BIN2HEX('1010')"); Ok("BIN2OCT('1010')"); Ok("HEX2BIN('FF')"); Ok("HEX2DEC('FF')"); Ok("HEX2OCT('FF')"); Ok("OCT2BIN('17')"); Ok("OCT2DEC('17')"); Ok("OCT2HEX('17')"); }
    }
}
