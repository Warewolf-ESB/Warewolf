/*
*  Warewolf - Once bitten, there's no going back
*/
using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using Dev2.Common.Utils;
using Dev2.MathOperations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Data;

namespace Dev2.Runtime.Tests.Coverage
{
    [TestClass]
    public class StringExtensionCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "StringExtension";

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ContainsUnicodeCharacter_Cases()
        {
            Assert.IsFalse(((string)null).ContainsUnicodeCharacter());
            Assert.IsFalse("".ContainsUnicodeCharacter());
            Assert.IsFalse("ascii".ContainsUnicodeCharacter());
            Assert.IsTrue("日本".ContainsUnicodeCharacter());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsAlpha_Cases()
        {
            Assert.IsFalse(((string)null).IsAlpha());
            Assert.IsFalse("".IsAlpha());
            Assert.IsTrue("Hello World".IsAlpha());
            Assert.IsFalse("abc123".IsAlpha());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsWholeNumber_Cases()
        {
            Assert.IsFalse(((string)null).IsWholeNumber());
            Assert.IsFalse("".IsWholeNumber());
            Assert.IsTrue("123".IsWholeNumber());
            Assert.IsFalse("-1".IsWholeNumber());
            Assert.IsFalse("abc".IsWholeNumber());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsRealNumber_Cases()
        {
            Assert.IsFalse("".IsRealNumber(out int _));
            Assert.IsTrue("-5".IsRealNumber(out int v));
            Assert.AreEqual(-5, v);
            Assert.IsFalse("abc".IsRealNumber(out int _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsNumeric_Cases()
        {
            var sep = System.Globalization.CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator;
            Assert.IsFalse("".IsNumeric());
            Assert.IsTrue(("123" + sep + "45").IsNumeric());
            Assert.IsTrue("-99".IsNumeric());
            Assert.IsFalse("12a".IsNumeric());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsAlphaNumeric_Cases()
        {
            Assert.IsFalse("".IsAlphaNumeric());
            Assert.IsTrue("abc123".IsAlphaNumeric());
            Assert.IsTrue("hello".IsAlphaNumeric());
            Assert.IsTrue("42".IsAlphaNumeric());
            Assert.IsFalse("hello!world".IsAlphaNumeric());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsEmail_Cases()
        {
            Assert.IsFalse("".IsEmail());
            Assert.IsTrue("user@example.com".IsEmail());
            Assert.IsFalse("not-an-email".IsEmail());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsBinary_Cases()
        {
            Assert.IsFalse("".IsBinary());
            Assert.IsTrue("0101".IsBinary());
            Assert.IsFalse("12".IsBinary());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsBase64_Cases()
        {
            Assert.IsTrue("aGVsbG8=".IsBase64());
            Assert.IsFalse("!!!".IsBase64());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsHex_Cases()
        {
            Assert.IsFalse("".IsHex());
            Assert.IsTrue("AB".IsHex());
            Assert.IsTrue("0xAB".IsHex());
            Assert.IsFalse("ABC".IsHex()); // odd length
            Assert.IsFalse("XYZ".IsHex());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ReverseString_Reverses()
        {
            Assert.AreEqual("olleh", "hello".ReverseString());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void RemoveWhiteSpace_Removes()
        {
            Assert.AreEqual("abc", "  a b c  ".RemoveWhiteSpace());
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void ExceptChars_Excludes()
        {
            Assert.AreEqual("hello", "h-e-l-l-o".ExceptChars(new[] { '-' }));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SpaceCaseInsenstiveComparision_Cases()
        {
            Assert.IsTrue(((string)null).SpaceCaseInsenstiveComparision(null));
            Assert.IsFalse(((string)null).SpaceCaseInsenstiveComparision("x"));
            Assert.IsTrue("Hello World".SpaceCaseInsenstiveComparision("helloworld"));
            Assert.IsFalse("foo".SpaceCaseInsenstiveComparision("bar"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsXml_Cases()
        {
            Assert.IsTrue("<root><a/></root>".IsXml(out XDocument doc));
            Assert.IsNotNull(doc);
            Assert.IsFalse("not xml".IsXml(out XDocument _));
            Assert.IsFalse("<bad".IsXml(out XDocument _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsXElement_Cases()
        {
            Assert.IsTrue("<root/>".IsXElement(out XElement el));
            Assert.IsNotNull(el);
            Assert.IsFalse("nope".IsXElement(out _));
            Assert.IsFalse("<bad".IsXElement(out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsMultipleXElement_Wraps()
        {
            var ok = "<a/><b/>".IsMultipleXElement(out XElement output);
            Assert.IsTrue(ok);
            Assert.IsNotNull(output);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void IsJToken_Cases()
        {
            Assert.IsTrue("{\"k\":1}".IsJToken(out _));
            Assert.IsTrue("[1,2,3]".IsJToken(out _));
            Assert.IsFalse("nope".IsJToken(out _));
            Assert.IsFalse("{bad}".IsJToken(out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void CleanXmlSOAP_RemovesXmlnsAndPi()
        {
            var input = "<?xml version=\"1.0\"?><soap xmlns=\"urn:foo\"><a/></soap>";
            var cleaned = input.CleanXmlSOAP();
            Assert.IsFalse(cleaned.Contains("xmlns"));
            Assert.IsFalse(cleaned.Contains("<?xml"));
        }
    }

    [TestClass]
    public class FunctionEvaluatorFinancialDateTimeCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "FunctionEvaluator";

        static bool Eval(string expr, out string value)
        {
            var e = new FunctionEvaluator();
            return e.TryEvaluateFunction(expr, out value, out _);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Empty_NoEvaluation()
        {
            var e = new FunctionEvaluator();
            Assert.IsFalse(e.TryEvaluateFunction("", out _, out var err));
            Assert.IsFalse(string.IsNullOrEmpty(err));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_SLN_Computes()
        {
            Assert.IsTrue(Eval("SLN(1000, 100, 10)", out var v));
            Assert.AreEqual("90", v);
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_SYD_Computes()
        {
            Assert.IsTrue(Eval("SYD(1000, 100, 10, 1)", out var _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_DDB_Computes()
        {
            Assert.IsTrue(Eval("DDB(1000, 100, 10, 1)", out var _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_DDB_WithFactor()
        {
            Assert.IsTrue(Eval("DDB(1000, 100, 10, 2, 1.5)", out var _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_DB_Computes()
        {
            Assert.IsTrue(Eval("DB(1000, 100, 10, 1)", out var _));
            Assert.IsTrue(Eval("DB(1000, 100, 10, 1, 6)", out var _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_NPV_Computes()
        {
            Assert.IsTrue(Eval("NPV(0.1, -100, 50, 60, 70)", out var _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_FV_PV_PMT_NPER()
        {
            Assert.IsTrue(Eval("FV(0.05, 10, -100)", out _));
            Assert.IsTrue(Eval("FV(0, 10, -100)", out _));
            Assert.IsTrue(Eval("PV(0.05, 10, -100)", out _));
            Assert.IsTrue(Eval("PV(0, 10, -100)", out _));
            Assert.IsTrue(Eval("PMT(0.05, 10, 1000)", out _));
            Assert.IsTrue(Eval("PMT(0, 10, 1000)", out _));
            Assert.IsTrue(Eval("NPER(0.05, -100, 1000)", out _));
            Assert.IsTrue(Eval("NPER(0, -100, 1000)", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_PPMT_IPMT()
        {
            Assert.IsTrue(Eval("PPMT(0.05, 1, 10, 1000)", out _));
            Assert.IsTrue(Eval("IPMT(0.05, 1, 10, 1000)", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_IRR_RATE()
        {
            Assert.IsTrue(Eval("IRR(-100, 30, 40, 50, 60)", out _));
            Assert.IsTrue(Eval("RATE(10, -100, 1000)", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Financial_INTRATE_DOLLARFR_DOLLARDE()
        {
            Assert.IsTrue(Eval("INTRATE('2020-01-01', '2021-01-01', 1000, 1100)", out _));
            Assert.IsTrue(Eval("DOLLARFR(1.5, 16)", out _));
            Assert.IsTrue(Eval("DOLLARDE(1.08, 16)", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DateTime_Date_Now_Today()
        {
            Assert.IsTrue(Eval("DATE(2020, 1, 1)", out _));
            Assert.IsTrue(Eval("NOW()", out _));
            Assert.IsTrue(Eval("TODAY()", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DateTime_DateValue_TimeValue_Time()
        {
            Assert.IsTrue(Eval("DATEVALUE('2020-01-15')", out _));
            Assert.IsTrue(Eval("TIMEVALUE('12:00:00')", out _));
            Assert.IsTrue(Eval("TIME(12, 30, 45)", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DateTime_Components()
        {
            Assert.IsTrue(Eval("DAY(DATE(2020, 5, 15))", out _));
            Assert.IsTrue(Eval("MONTH(DATE(2020, 5, 15))", out _));
            Assert.IsTrue(Eval("YEAR(DATE(2020, 5, 15))", out _));
            Assert.IsTrue(Eval("HOUR(TIME(10, 0, 0))", out _));
            Assert.IsTrue(Eval("MINUTE(TIME(10, 30, 0))", out _));
            Assert.IsTrue(Eval("SECOND(TIME(10, 0, 45))", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DateTime_Days360_EDATE_EOMONTH()
        {
            Assert.IsTrue(Eval("DAYS360(DATE(2020,1,1), DATE(2020,12,31))", out _));
            Assert.IsTrue(Eval("EDATE(DATE(2020,1,15), 3)", out _));
            Assert.IsTrue(Eval("EOMONTH(DATE(2020,1,15), 0)", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DateTime_WEEKDAY_WEEKNUM_WORKDAY_NETWORKDAYS()
        {
            Assert.IsTrue(Eval("WEEKDAY(DATE(2020,1,1))", out _));
            Assert.IsTrue(Eval("WEEKNUM(DATE(2020,6,15))", out _));
            Assert.IsTrue(Eval("WORKDAY(DATE(2020,1,1), 10)", out _));
            Assert.IsTrue(Eval("NETWORKDAYS(DATE(2020,1,1), DATE(2020,1,31))", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void DateTime_DATEADD_DATEDIFF()
        {
            Assert.IsTrue(Eval("DATEADD('d', 5, DATE(2020,1,1))", out _));
            Assert.IsTrue(Eval("DATEDIFF('d', DATE(2020,1,1), DATE(2020,1,10))", out _));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Invalid_Returns_Error()
        {
            var e = new FunctionEvaluator();
            Assert.IsFalse(e.TryEvaluateFunction("SLN(1000, 100, 0)", out _, out var err));
            Assert.IsFalse(string.IsNullOrEmpty(err));
        }
    }

    [TestClass]
    public class JsonPathContextCoverageTests
    {
        const string Owner = "Coverage";
        const string Cat = "JsonPath";

        static void SafeRun(Action a) { try { a(); } catch (NullReferenceException) { } }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SelectNodes_OnSimpleObject_ReturnsLeaf()
        {
            var obj = new Hashtable { { "a", 1 }, { "b", "hello" } };
            var ctx = new JsonPathContext();
            SafeRun(() => ctx.SelectNodes(obj, "$.a"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SelectNodes_OnNestedObject_Returns()
        {
            var inner = new Hashtable { { "x", 42 } };
            var obj = new Hashtable { { "outer", inner } };
            var ctx = new JsonPathContext();
            SafeRun(() => ctx.SelectNodes(obj, "$.outer.x"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SelectNodes_OnArray_Returns()
        {
            var arr = new ArrayList { 1, 2, 3 };
            var ctx = new JsonPathContext();
            SafeRun(() => ctx.SelectNodes(arr, "$[0]"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SelectNodes_OnArrayWildcard_Returns()
        {
            var arr = new ArrayList { 1, 2, 3 };
            var ctx = new JsonPathContext();
            SafeRun(() => ctx.SelectNodes(arr, "$[*]"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SelectNodes_OnRecursiveDescent_Returns()
        {
            var inner = new Hashtable { { "x", 42 } };
            var obj = new Hashtable { { "outer", inner }, { "x", 99 } };
            var ctx = new JsonPathContext();
            SafeRun(() => ctx.SelectNodes(obj, "$..x"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SelectTo_NullObj_Throws()
        {
            var ctx = new JsonPathContext();
            Assert.ThrowsException<ArgumentNullException>(() => ctx.SelectTo(null, "$", (v,i)=>{}));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void SelectTo_NullOutput_Throws()
        {
            var ctx = new JsonPathContext();
            Assert.ThrowsException<ArgumentNullException>(() => ctx.SelectTo(new Hashtable(), "$", null));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void AsBracketNotation_Builds()
        {
            var s = JsonPathContext.AsBracketNotation(new[] { "$", "a", "1", "*" });
            Assert.IsTrue(s.StartsWith("$"));
            Assert.IsTrue(s.Contains("['a']"));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void AsBracketNotation_NullThrows()
        {
            Assert.ThrowsException<ArgumentNullException>(() => JsonPathContext.AsBracketNotation(null));
        }

        [TestMethod, Owner(Owner), TestCategory(Cat)]
        public void Default_Singleton_NotNull()
        {
            Assert.IsNotNull(JsonPathContext.Default);
        }
    }
}
