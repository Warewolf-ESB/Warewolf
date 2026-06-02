using System.Collections.Generic;
using System.Linq;
using Dev2.BussinessLogic;
using Dev2.DataList;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Tests.Activities.BussinessLogic
{
    [TestClass]
    public class RsOpSearchValidationTests
    {
        static DataStorage.WarewolfAtom Atom(string value) => DataStorage.WarewolfAtom.NewDataString(value);

        static List<DataStorage.WarewolfAtom> Vals(params string[] values) => values.Select(Atom).ToList();

        static readonly DataStorage.WarewolfAtom Nothing = DataStorage.WarewolfAtom.Nothing;

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsBase64_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsBase64();
            Assert.AreEqual("Is Base64", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("YWJj")));
            Assert.IsFalse(func(Atom("@@@")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotBase64_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotBase64();
            Assert.AreEqual("Not Base64", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("YWJj")));
            Assert.IsTrue(func(Atom("@@@")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsBinary_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsBinary();
            Assert.AreEqual("Is Binary", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("0101")));
            Assert.IsFalse(func(Atom("abc")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotBinary_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotBinary();
            Assert.AreEqual("Not Binary", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("0101")));
            Assert.IsTrue(func(Atom("abc")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsHex_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsHex();
            Assert.AreEqual("Is Hex", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("1A2B")));
            Assert.IsFalse(func(Atom("ZZ")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotHex_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotHex();
            Assert.AreEqual("Not Hex", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("1A2B")));
            Assert.IsTrue(func(Atom("ZZ")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsAlphanumeric_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsAlphanumeric();
            Assert.AreEqual("Is Alphanumeric", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("abc123")));
            Assert.IsFalse(func(Atom("@@@")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotAlphanumeric_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotAlphanumeric();
            Assert.AreEqual("Not Alphanumeric", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("abc123")));
            Assert.IsTrue(func(Atom("@@@")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsDate_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsDate();
            Assert.AreEqual("Is Date", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("2020-01-01")));
            Assert.IsFalse(func(Atom("notadate")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotDate_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotDate();
            Assert.AreEqual("Not Date", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("2020-01-01")));
            Assert.IsTrue(func(Atom("notadate")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsEmail_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsEmail();
            Assert.AreEqual("Is Email", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("test@example.com")));
            Assert.IsFalse(func(Atom("notanemail")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotEmail_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotEmail();
            Assert.AreEqual("Not Email", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("test@example.com")));
            Assert.IsTrue(func(Atom("notanemail")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsNumeric_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsNumeric();
            Assert.AreEqual("Is Numeric", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("123")));
            Assert.IsFalse(func(Atom("abc")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotNumeric_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotNumeric();
            Assert.AreEqual("Not Numeric", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("123")));
            Assert.IsTrue(func(Atom("abc")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsText_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsText();
            Assert.AreEqual("Is Text", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("abc")));
            Assert.IsFalse(func(Atom("123")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotText_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotText();
            Assert.AreEqual("Not Text", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("abc")));
            Assert.IsTrue(func(Atom("123")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsXML_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsXML();
            Assert.AreEqual("Is XML", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(func(Atom("<root/>")));
            Assert.IsFalse(func(Atom("not xml")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotXML_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpNotXML();
            Assert.AreEqual("Not XML", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("<root/>")));
            Assert.IsTrue(func(Atom("not xml")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsError_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsError();
            Assert.AreEqual("There is An Error", op.HandlesType());
            Assert.AreEqual(0, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("anything")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsNoError_HandlesType_ArgumentCount_And_Func()
        {
            var op = new RsOpIsNoError();
            Assert.AreEqual("There is No Error", op.HandlesType());
            Assert.AreEqual(0, op.ArgumentCount);
            var func = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsFalse(func(Atom("anything")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsNull_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpIsNull();
            Assert.AreEqual("Is NULL", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(allFunc(Nothing));
            Assert.IsFalse(allFunc(Atom("value")));

            var anyFunc = op.CreateFunc(Vals("x"), null, null, false);
            Assert.IsTrue(anyFunc(Nothing));
            Assert.IsFalse(anyFunc(Atom("value")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpIsNotNull_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpIsNotNull();
            Assert.AreEqual("Is Not NULL", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("x"), null, null, true);
            Assert.IsTrue(allFunc(Atom("value")));
            Assert.IsFalse(allFunc(Nothing));

            var anyFunc = op.CreateFunc(Vals("x"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("value")));
            Assert.IsFalse(anyFunc(Nothing));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpEqual_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpEqual();
            Assert.AreEqual("=", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("abc"), null, null, true);
            Assert.IsTrue(allFunc(Atom("abc")));
            Assert.IsFalse(allFunc(Atom("xyz")));

            var anyFunc = op.CreateFunc(Vals("abc"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("abc")));
            Assert.IsFalse(anyFunc(Atom("xyz")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotEqual_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpNotEqual();
            Assert.AreEqual("<> (Not Equal)", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("abc"), null, null, true);
            Assert.IsTrue(allFunc(Atom("xyz")));
            Assert.IsFalse(allFunc(Atom("abc")));

            var anyFunc = op.CreateFunc(Vals("abc"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("xyz")));
            Assert.IsFalse(anyFunc(Atom("abc")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpGreaterThan_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpGreaterThan();
            Assert.AreEqual(">", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("a"), null, null, true);
            Assert.IsTrue(allFunc(Atom("b")));
            Assert.IsFalse(allFunc(Atom("a")));

            var anyFunc = op.CreateFunc(Vals("a"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("b")));
            Assert.IsFalse(anyFunc(Atom("a")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpGreaterThanOrEqualTo_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpGreaterThanOrEqualTo();
            Assert.AreEqual(">=", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("a"), null, null, true);
            Assert.IsTrue(allFunc(Atom("a")));
            Assert.IsFalse(allFunc(Atom("0")));

            var anyFunc = op.CreateFunc(Vals("a"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("b")));
            Assert.IsFalse(anyFunc(Atom("0")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpLessThan_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpLessThan();
            Assert.AreEqual("<", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("b"), null, null, true);
            Assert.IsTrue(allFunc(Atom("a")));
            Assert.IsFalse(allFunc(Atom("b")));

            var anyFunc = op.CreateFunc(Vals("b"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("a")));
            Assert.IsFalse(anyFunc(Atom("b")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpLessThanOrEqualTo_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpLessThanOrEqualTo();
            Assert.AreEqual("<=", op.HandlesType());
            Assert.AreEqual(1, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("b"), null, null, true);
            Assert.IsTrue(allFunc(Atom("b")));
            Assert.IsFalse(allFunc(Atom("c")));

            var anyFunc = op.CreateFunc(Vals("b"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("a")));
            Assert.IsFalse(anyFunc(Atom("c")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpContains_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpContains();
            Assert.AreEqual("Contains", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("bc"), null, null, true);
            Assert.IsTrue(allFunc(Atom("abcd")));
            Assert.IsFalse(allFunc(Atom("xyz")));

            var anyFunc = op.CreateFunc(Vals("bc"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("abcd")));
            Assert.IsFalse(anyFunc(Atom("xyz")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotContains_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpNotContains();
            Assert.AreEqual("Doesn't Contain", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("zz"), null, null, true);
            Assert.IsTrue(allFunc(Atom("abcd")));
            Assert.IsFalse(allFunc(Atom("azzd")));

            var anyFunc = op.CreateFunc(Vals("zz"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("abcd")));
            Assert.IsFalse(anyFunc(Atom("azzd")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpStartsWith_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpStartsWith();
            Assert.AreEqual("Starts With", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("ab"), null, null, true);
            Assert.IsTrue(allFunc(Atom("abcd")));
            Assert.IsFalse(allFunc(Atom("xycd")));

            var anyFunc = op.CreateFunc(Vals("ab"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("abcd")));
            Assert.IsFalse(anyFunc(Atom("xycd")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotStartsWith_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpNotStartsWith();
            Assert.AreEqual("Doesn't Start With", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("ab"), null, null, true);
            Assert.IsTrue(allFunc(Atom("xycd")));
            Assert.IsFalse(allFunc(Atom("abcd")));

            var anyFunc = op.CreateFunc(Vals("ab"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("xycd")));
            Assert.IsFalse(anyFunc(Atom("abcd")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpEndsWith_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpEndsWith();
            Assert.AreEqual("Ends With", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("cd"), null, null, true);
            Assert.IsTrue(allFunc(Atom("abcd")));
            Assert.IsFalse(allFunc(Atom("abxy")));

            var anyFunc = op.CreateFunc(Vals("cd"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("abcd")));
            Assert.IsFalse(anyFunc(Atom("abxy")));
        }

        [TestMethod]
        [Timeout(60000)]
        [Owner("Ashley Lewis")]
        public void RsOpNotEndsWith_HandlesType_ArgumentCount_And_BothBranches()
        {
            var op = new RsOpNotEndsWith();
            Assert.AreEqual("Doesn't End With", op.HandlesType());
            Assert.AreEqual(2, op.ArgumentCount);

            var allFunc = op.CreateFunc(Vals("cd"), null, null, true);
            Assert.IsTrue(allFunc(Atom("abxy")));
            Assert.IsFalse(allFunc(Atom("abcd")));

            var anyFunc = op.CreateFunc(Vals("cd"), null, null, false);
            Assert.IsTrue(anyFunc(Atom("abxy")));
            Assert.IsFalse(anyFunc(Atom("abcd")));
        }
    }
}
