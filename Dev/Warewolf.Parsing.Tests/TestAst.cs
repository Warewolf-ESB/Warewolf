using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WarewolfParsingTest
{
    [TestClass]
    public class TestAst
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_AtomEquality_TestEquality()
        {
            //------------Setup for test--------------------------
            var a = DataStorage.WarewolfAtom.NewDataString("1") as IComparable;
            var b = DataStorage.WarewolfAtom.NewDataString("1") as IComparable;
            var c = DataStorage.WarewolfAtom.NewInt(1) as IComparable;
            var d = DataStorage.WarewolfAtom.NewInt(1) as IComparable;
            var e = DataStorage.WarewolfAtom.NewFloat(1.1) as IComparable;
            var f = DataStorage.WarewolfAtom.NewFloat(1.1) as IComparable;
            var g = DataStorage.WarewolfAtom.NewFloat(1) as IComparable;
            var h = DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int, DataStorage.WarewolfAtom>(1,DataStorage.WarewolfAtom.NewDataString("1"))) as IComparable;
            var i = DataStorage.WarewolfAtom.NewPositionedValue(new Tuple<int, DataStorage.WarewolfAtom>(1, DataStorage.WarewolfAtom.NewDataString("1"))) as IComparable;
            //------------Execute Test---------------------------
            Assert.IsTrue(0==a.CompareTo(b));
            Assert.IsTrue(0 == c.CompareTo(d));
            Assert.IsTrue(0 == e.CompareTo(f));
            Assert.IsTrue(0 == e.CompareTo(f));
            Assert.IsTrue(0 == g.CompareTo(c));
            Assert.IsTrue(0 == a.CompareTo(c));
            Assert.IsTrue(0 == g.CompareTo(a));
            Assert.IsTrue(0 == (DataStorage.WarewolfAtom.Nothing as IComparable).CompareTo(DataStorage.WarewolfAtom.Nothing));
            Assert.IsFalse(0 == (DataStorage.WarewolfAtom.Nothing as IComparable).CompareTo(c));

            Assert.IsTrue(0 == a.CompareTo(g));
            Assert.IsTrue(0 == c.CompareTo(g));
            Assert.IsTrue(0 == c.CompareTo(a));
            Assert.IsTrue(0 == h.CompareTo(i));
            Assert.IsTrue(-1==(DataStorage.WarewolfAtom.Nothing as IComparable).CompareTo(1));
            Assert.AreEqual(e.ToString(),"1.1");
            Assert.AreEqual(h.ToString(), "1");
            Assert.AreEqual(DataStorage.PositionColumn, "WarewolfPositionColumn");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_GetDecimalPlaces()
        {
           Assert.AreEqual(1,DataStorage.GetDecimalPlaces(123.4));
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_GetDecimalPlaces_BigNumber()
        {
            Assert.AreEqual(9, DataStorage.GetDecimalPlaces(123.456789111));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_TryParseAtom()
        {
            Assert.IsTrue(DataStorage.tryParseAtom("1").IsInt);
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_TryParseAtom_LineBreakOne()
        {
            Assert.IsFalse(DataStorage.tryParseAtom("1\r").IsInt);
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_TryParseAtom_LineBreakTwo()
        {
            Assert.IsFalse(DataStorage.tryParseAtom("1\n").IsInt);
        }
        [TestMethod]
        [Owner("Pieter Terblanche")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_TryParseAtom_LineBreakThree()
        {
            Assert.IsFalse(DataStorage.tryParseAtom("1\r\n").IsInt);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_TryParseAtomFl()
        {
            Assert.IsTrue(DataStorage.tryParseAtom("1.1").IsFloat);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_TryParseAtomStr()
        {
            Assert.IsTrue(DataStorage.tryParseAtom("1.s1").IsDataString);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataStorage_AtomEquality")]
        public void DataStorage_AtomEquality_TestEqualityAtoms()
        {
            //------------Setup for test--------------------------
            var string1 = DataStorage.WarewolfAtom.NewDataString("123");
            var string2 = DataStorage.WarewolfAtom.NewDataString("123");
            var int1 = DataStorage.WarewolfAtom.NewInt(123);
            var int2 = DataStorage.WarewolfAtom.NewInt(123);
            var float1 = DataStorage.WarewolfAtom.NewFloat(1.1);
            var float2 = DataStorage.WarewolfAtom.NewFloat(1.1);
            var stringFloat = DataStorage.WarewolfAtom.NewDataString("1.1");
            var intFloat = DataStorage.WarewolfAtom.NewFloat(123);
            //------------Execute Test---------------------------
            Assert.IsTrue(0 == DataStorage.CompareAtoms(string1, string2));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(int1, int2));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(float1, float2));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(float1, float2));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(float1, stringFloat));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(intFloat, string1));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(string1, intFloat));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(string1, int1));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(int1, string1));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(int2, int1));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(string1, int1));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(int1, intFloat));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(intFloat, int2));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(string1, int1));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(DataStorage.WarewolfAtom.Nothing, DataStorage.WarewolfAtom.Nothing));
            Assert.IsTrue(-1 == DataStorage.CompareAtoms(DataStorage.WarewolfAtom.Nothing, DataStorage.WarewolfAtom.NewFloat(1)));
            //------------Assert Results-------------------------
        }


        [TestMethod]
        [Owner("Rory McGuire")]
        [TestCategory("DataStorage_AtomComparison")]
        public void DataStorage_AtomComparison_TestComparisonAtoms()
        {
            //------------Setup for test--------------------------
            var string1 = DataStorage.WarewolfAtom.NewDataString("123");
            var int1 = DataStorage.WarewolfAtom.NewInt(12);
            var float1 = DataStorage.WarewolfAtom.NewFloat(1.1);
            var stringFloat = DataStorage.WarewolfAtom.NewDataString("1.23");
            var intFloat = DataStorage.WarewolfAtom.NewFloat(321.123);
            var bob = DataStorage.WarewolfAtom.NewDataString("bob");

            //------------Execute Test---------------------------
            Assert.IsTrue(DataStorage.CompareAtoms(string1, float1) > 0);
            Assert.IsTrue(DataStorage.CompareAtoms(float1, string1) < 0);

            Assert.IsTrue(DataStorage.CompareAtoms(string1, int1) > 0);
            Assert.IsTrue(DataStorage.CompareAtoms(int1, string1) < 0);

            Assert.IsTrue(DataStorage.CompareAtoms(string1, stringFloat) > 0);
            Assert.IsTrue(DataStorage.CompareAtoms(stringFloat, string1) < 0);

            Assert.IsTrue(DataStorage.CompareAtoms(string1, intFloat) < 0);
            Assert.IsTrue(DataStorage.CompareAtoms(intFloat, string1) > 0);

            Assert.IsTrue(DataStorage.CompareAtoms(bob, string1) > 0);
            Assert.IsTrue(DataStorage.CompareAtoms(string1, bob) < 0);
            Assert.ThrowsException<DataStorage.WarewolfInvalidComparisonException>(() => DataStorage.CompareAtoms(bob, int1));
            Assert.ThrowsException<DataStorage.WarewolfInvalidComparisonException>(() => DataStorage.CompareAtoms(int1, bob));
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(DataStorage))]
        public void DataStorage_TryParseAtom_GivenInputStringStartsWithZero_ShouldReturnIsIntTrue()
        {
            Assert.IsTrue(DataStorage.tryParseAtom("0123").IsInt);
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(DataStorage))]
        public void DataStorage_TryFloatParseAtom_GivenInputStringStartsWithZero_ShouldReturnIsFloatTrue()
        {
            Assert.IsTrue(DataStorage.tryFloatParseAtom("01.0").IsFloat);
            Assert.IsTrue(DataStorage.tryFloatParseAtom("0.0").IsFloat);
        }

        [TestMethod]
        [Owner("Devaji Chotaliya")]
        [TestCategory(nameof(DataStorage))]
        public void DataStorage_TryFloatParseAtom_GivenInputStringEndsWithZero_ShouldReturnIsFloatTrue()
        {
            Assert.IsTrue(DataStorage.tryFloatParseAtom("1500.50").IsFloat);
        }
    }
}
