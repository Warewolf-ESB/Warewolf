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
            var a = DataStorage.WarewolfAtom.NewDataString("1") ;
            var b = DataStorage.WarewolfAtom.NewDataString("1") ;
            var c = DataStorage.WarewolfAtom.NewInt(1) ;
            var d = DataStorage.WarewolfAtom.NewInt(1) ;
            var e = DataStorage.WarewolfAtom.NewFloat(1.1) ;
            var f = DataStorage.WarewolfAtom.NewFloat(1.1) ;
            var g = DataStorage.WarewolfAtom.NewFloat(1) ;
            //------------Execute Test---------------------------
            Assert.IsTrue(0 == DataStorage.CompareAtoms(a,b));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(c,d));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(e,f));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(e,f));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(g,a));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(a, g));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(a,c));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(c, a));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(d, c));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(a,c));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(c,g));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(g, d));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(a,c));
            Assert.IsTrue(0 == DataStorage.CompareAtoms(DataStorage.WarewolfAtom.Nothing, DataStorage.WarewolfAtom.Nothing));
            Assert.IsTrue(-1 == DataStorage.CompareAtoms(DataStorage.WarewolfAtom.Nothing, DataStorage.WarewolfAtom.NewFloat(1)));
            //------------Assert Results-------------------------
        }
    }
}
