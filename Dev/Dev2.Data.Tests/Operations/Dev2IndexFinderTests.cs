
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Enums;
using Dev2.Data.Interfaces;
using Dev2.Data.Operations;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace Dev2.Data.Tests.Operations
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class Dev2IndexFinderTests
    {
        private IDev2IndexFinder _indexFinder;

        public Dev2IndexFinderTests()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        #region Additional test attributes

        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        [TestInitialize()]
        public void MyTestInitialize()
        {
            _indexFinder = new Dev2IndexFinder();
        }

        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //

        #endregion

        #region Index Tests

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_First_LeftToRight_Expected_Index_Of_Four_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.FirstOccurrence, "F",
                                   enIndexFinderDirection.LeftToRight, false, 0);
            Assert.AreEqual(4, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_Last_LeftToRight_Expected_Index_Of_Seven_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.LastOccurrence, "F",
                                   enIndexFinderDirection.LeftToRight, false, 0);
            Assert.AreEqual(7, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_Last_RightToLeft_Expected_Index_Of_Eight_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.LastOccurrence, "F",
                                   enIndexFinderDirection.RightToLeft, false, 0);
            Assert.AreEqual(8, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_ALL_RightToLeft_Expected_Index_Of_Eight_AND_FIVE_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.AllOccurrences, "F",
                                   enIndexFinderDirection.RightToLeft, false, 0);
            Assert.AreEqual(5, actual.ElementAt(0));
            Assert.AreEqual(8, actual.ElementAt(1));
        }

        //Juries Bug 8725
        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_ALL_RightToLeft_WITH_DOUBLE_Expected_Index_Of_Eight_AND_FIVE_Returned()
        {
            var actual = _indexFinder.FindIndex("ABFFDFFGHIB", enIndexFinderOccurrence.AllOccurrences, "FF",
                                   enIndexFinderDirection.RightToLeft, false, 0);
            Assert.AreEqual(5, actual.ElementAt(0));
            Assert.AreEqual(8, actual.ElementAt(1));
        }

        //Juries Bug 8725
        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_FIRST_RightToLeft_WITH_DOUBLE_Expected_Index_Of_FIVE_Returned()
        {
            var actual = _indexFinder.FindIndex("ABFFDFFGHIB", enIndexFinderOccurrence.FirstOccurrence, "FF",
                                   enIndexFinderDirection.RightToLeft, false, 0);
            Assert.AreEqual(5, actual.ElementAt(0));
        }

        //Juries Bug 8725
        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_LAST_RightToLeft_WITH_DOUBLE_Expected_Index_Of_Eight_Returned()
        {
            var actual = _indexFinder.FindIndex("ABFFDFFGHIB", enIndexFinderOccurrence.LastOccurrence, "FF",
                                   enIndexFinderDirection.RightToLeft, false, 0);
            Assert.AreEqual(8, actual.ElementAt(0));
        }

        //Juries Bug 8725
        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_ALL_LeftToRight_WITH_DOUBLE_Expected_Index_Of_Three_AND_Six_Returned()
        {
            var actual = _indexFinder.FindIndex("ABFFDFFGHIB", enIndexFinderOccurrence.AllOccurrences, "FF",
                                   enIndexFinderDirection.LeftToRight, false, 0);
            Assert.AreEqual(3, actual.ElementAt(0));
            Assert.AreEqual(6, actual.ElementAt(1));
        }

        //Juries Bug 8725
        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_FIRST_LeftToRight_WITH_DOUBLE_Expected_Index_Of_Three_Returned()
        {
            var actual = _indexFinder.FindIndex("ABFFDFFGHIB", enIndexFinderOccurrence.FirstOccurrence, "FF",
                                   enIndexFinderDirection.LeftToRight, false, 0);
            Assert.AreEqual(3, actual.ElementAt(0));
        }

        //Juries Bug 8725
        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_LAST_LeftToRight_WITH_DOUBLE_Expected_Index_Of_6_Returned()
        {
            var actual = _indexFinder.FindIndex("ABFFDFFGHIB", enIndexFinderOccurrence.LastOccurrence, "FF",
                                   enIndexFinderDirection.LeftToRight, false, 0);
            Assert.AreEqual(6, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_First_RightToLeft_Expected_Index_Of_Five_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.FirstOccurrence, "F",
                                   enIndexFinderDirection.RightToLeft, false, 0);
            Assert.AreEqual(5, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_First_LeftToRight_MatchCase_Expected_Index_Of_Four_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.FirstOccurrence, "F",
                                   enIndexFinderDirection.LeftToRight, true, 0);
            Assert.AreEqual(4, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_First_LeftToRight_MatchCase_Expected_Index_Of_Negative_One_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.FirstOccurrence, "f",
                                   enIndexFinderDirection.LeftToRight, true, 0);
            Assert.AreEqual(-1, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_First_LeftToRight_NoMatch_Expected_Index_Of_Negative_One_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.FirstOccurrence, "Z",
                                   enIndexFinderDirection.LeftToRight, false, 0);
            Assert.AreEqual(-1, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_First__Null_Direction_Expected_Index_Of_Negative_One_Returned()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", "First Occurrence", "Z",
                                   null, false, "0");
            Assert.AreEqual(-1, actual.ElementAt(0));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_All_LeftToRight_Expected_Index_Of_Four_And_Seven()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.AllOccurrences, "F",
                                   enIndexFinderDirection.LeftToRight, false, 0);
            Assert.AreEqual(4, actual.ElementAt(0));
            Assert.AreEqual(7, actual.ElementAt(1));
        }

        [TestMethod]
        public void FindIndex_RawData_Search_In_RawData_All_LeftToRight_StartIndex_Five_Expected_Index_Seven()
        {
            var actual = _indexFinder.FindIndex("ABCFDEFGHIB", enIndexFinderOccurrence.AllOccurrences, "F",
                                   enIndexFinderDirection.LeftToRight, false, 4);
            Assert.AreEqual(7, actual.ElementAt(0));
        }



        #endregion
    }
}
