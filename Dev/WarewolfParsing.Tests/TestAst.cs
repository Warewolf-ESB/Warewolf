using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace WarewolfParsingTest
{
    [TestClass]
    public class TestAst
    {
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataAstMutable_AtomEquality")]
        public void DataAstMutable_AtomEquality_TestEquality()
        {
            //------------Setup for test--------------------------
            var a = DataASTMutable.WarewolfAtom.NewDataString("1") as IComparable;
            var b = DataASTMutable.WarewolfAtom.NewDataString("1") as IComparable;
            var c = DataASTMutable.WarewolfAtom.NewInt(1) as IComparable;
            var d = DataASTMutable.WarewolfAtom.NewInt(1) as IComparable;
            var e = DataASTMutable.WarewolfAtom.NewFloat(1.1) as IComparable;
            var f = DataASTMutable.WarewolfAtom.NewFloat(1.1) as IComparable;
            var g = DataASTMutable.WarewolfAtom.NewFloat(1) as IComparable;
            var h = DataASTMutable.WarewolfAtom.NewPositionedValue(new Tuple<int, DataASTMutable.WarewolfAtom>(1,DataASTMutable.WarewolfAtom.NewDataString("1"))) as IComparable;
            var i = DataASTMutable.WarewolfAtom.NewPositionedValue(new Tuple<int, DataASTMutable.WarewolfAtom>(1, DataASTMutable.WarewolfAtom.NewDataString("1"))) as IComparable;
            //------------Execute Test---------------------------
            Assert.IsTrue(0==a.CompareTo(b));
            Assert.IsTrue(0 == c.CompareTo(d));
            Assert.IsTrue(0 == e.CompareTo(f));
            Assert.IsTrue(0 == e.CompareTo(f));
            Assert.IsTrue(0 == g.CompareTo(c));
            Assert.IsTrue(0 == a.CompareTo(c));
            Assert.IsTrue(0 == g.CompareTo(a));
            Assert.IsTrue(0 == (DataASTMutable.WarewolfAtom.Nothing as IComparable).CompareTo(DataASTMutable.WarewolfAtom.Nothing));
            Assert.IsFalse(0 == (DataASTMutable.WarewolfAtom.Nothing as IComparable).CompareTo(c));

            Assert.IsTrue(0 == a.CompareTo(g));
            Assert.IsTrue(0 == c.CompareTo(g));
            Assert.IsTrue(0 == c.CompareTo(a));
            Assert.IsTrue(0 == h.CompareTo(i));
            Assert.IsTrue(-1==(DataASTMutable.WarewolfAtom.Nothing as IComparable).CompareTo(1));
            Assert.AreEqual(e.ToString(),"1.1");
            Assert.AreEqual(h.ToString(), "1");
            Assert.AreEqual(DataASTMutable.PositionColumn, "WarewolfPositionColumn");
            //------------Assert Results-------------------------
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataAstMutable_AtomEquality")]
        public void DataAstMutable_GetDecimalPlaces()
        {
           Assert.AreEqual(1,DataASTMutable.GetDecimalPlaces(123.4));
        }



        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataAstMutable_AtomEquality")]
        public void DataAstMutable_GetDecimalPlaces_BigNumber()
        {
            Assert.AreEqual(9, DataASTMutable.GetDecimalPlaces(123.456789111));
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataAstMutable_AtomEquality")]
        public void DataAstMutable_TryParseAtom()
        {
            Assert.IsTrue(DataASTMutable.tryParseAtom("1").IsInt);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataAstMutable_AtomEquality")]
        public void DataAstMutable_TryParseAtomFl()
        {
            Assert.IsTrue(DataASTMutable.tryParseAtom("1.1").IsFloat);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataAstMutable_AtomEquality")]
        public void DataAstMutable_TryParseAtomStr()
        {
            Assert.IsTrue(DataASTMutable.tryParseAtom("1.s1").IsDataString);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataAstMutable_AtomEquality")]
        public void DataAstMutable_AtomEquality_TestEqualityAtoms()
        {
            //------------Setup for test--------------------------
            var a = DataASTMutable.WarewolfAtom.NewDataString("1") ;
            var b = DataASTMutable.WarewolfAtom.NewDataString("1") ;
            var c = DataASTMutable.WarewolfAtom.NewInt(1) ;
            var d = DataASTMutable.WarewolfAtom.NewInt(1) ;
            var e = DataASTMutable.WarewolfAtom.NewFloat(1.1) ;
            var f = DataASTMutable.WarewolfAtom.NewFloat(1.1) ;
            var g = DataASTMutable.WarewolfAtom.NewFloat(1) ;
            //------------Execute Test---------------------------
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(a,b));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(c,d));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(e,f));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(e,f));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(g,a));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(a, g));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(a,c));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(c, a));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(d, c));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(a,c));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(c,g));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(g, d));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(a,c));
            Assert.IsTrue(0 == DataASTMutable.CompareAtoms(DataASTMutable.WarewolfAtom.Nothing, DataASTMutable.WarewolfAtom.Nothing));
            Assert.IsTrue(-1 == DataASTMutable.CompareAtoms(DataASTMutable.WarewolfAtom.Nothing, DataASTMutable.WarewolfAtom.NewFloat(1)));
            //------------Assert Results-------------------------
        }
    }
}
