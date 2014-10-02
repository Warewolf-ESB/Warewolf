
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System;
using System.Collections.Generic;
using System.Diagnostics;
using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.Persistence
{
    /// <summary>
    /// Summary description for BinaryDataListStorageTest
    /// </summary>
    [TestClass]
    public class BinaryDataListStorageTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        [TestMethod]
        public void CanStorageDisposeInAResonableAmountOfTime()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());

            // build insert value ;)
            var row = CreateBinaryDataListRow(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            DateTime start = DateTime.Now;

            // Insert information
            for (int i = 1; i <= 100000; i++)
            {
                bdls.Add(i, row);
            }

            DateTime end = DateTime.Now;

            double dif = (end.Ticks - (double)start.Ticks) / TimeSpan.TicksPerSecond;

            bdls.Dispose();

            Assert.IsTrue(dif <= 3.5, "100k rows took too long to insert into storage, should be about 0.5 seconds { " + dif + " }");

            Assert.AreEqual(0, bdls.Count, "Not all items disposed from row storage");
        }

        [TestMethod]
        public void TryGetValuesWithRange()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());

            // build insert value ;)
            var row = CreateBinaryDataListRow(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Insert information
            for (int i = 1; i <= 100000; i++)
            {
                bdls.Add(i, row);
            }
            const int startIndex = 1;
            const int endIndex = 100;
            List<IBinaryDataListRow> rows = bdls.GetValues(startIndex, endIndex);
            Assert.AreEqual(99,rows.Count);
            Assert.IsFalse(rows[0].IsEmpty);
            Assert.IsFalse(rows[5].IsEmpty);
        }


        [TestMethod]
        public void TryGetValuesWithRangeNonStart()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());


            // build insert value ;)
            var row = CreateBinaryDataListRow(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Insert information
            for (int i = 1; i <= 100000; i++)
            {
                bdls.Add(i, row);
            }
            const int startIndex = 101;
            const int endIndex = 110;
            List<IBinaryDataListRow> rows = bdls.GetValues(startIndex, endIndex);
            Assert.AreEqual(9,rows.Count);
            Assert.IsFalse(rows[0].IsEmpty);
            Assert.IsFalse(rows[5].IsEmpty);
        }


        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TryGetValuesWithRangeThrowsExceptionIfNoDataStartIndex()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());


            // build insert value ;)
            var row = CreateBinaryDataListRow(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Insert information
            for (int i = 1; i <= 100000; i++)
            {
                bdls.Add(i, row);
            }
            const int startIndex = 0;
            const int endIndex = 110;
            bdls.GetValues(startIndex, endIndex);
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public void TryGetValuesWithRangeThrowsExceptionIfNoDataEndIndex()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());


            // build insert value ;)
            var row = CreateBinaryDataListRow(Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString(), Guid.NewGuid().ToString());

            // Insert information
            for (int i = 1; i <= 100000; i++)
            {
                bdls.Add(i, row);
            }
            const int startIndex = 1;
            const int endIndex = 100002;
            bdls.GetValues(startIndex, endIndex);
        }

        #region Unique Test

        [TestMethod]
        [TestCategory("DsfUnique")]
        public void DistinctGetValuesWhenHasDistinctValuesShouldReturnOnlyDistinctRows()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());

            // build insert value ;)
            var row = CreateBinaryDataListRow("1", "1", "Barney", "T", "Buchan");
            var row2 = CreateBinaryDataListRow("2", "2", "Huggs", "W", "Naidu");
            var row3 = CreateBinaryDataListRow("3", "3", "Trav", "A", "Fri");
            var row4 = CreateBinaryDataListRow("4", "4", "Trav", "B", "Fri");
            var row5 = CreateBinaryDataListRow("5", "5", "Huggs", "C", "Bear");
            var row6 = CreateBinaryDataListRow("6", "6", "Huggs", "D", "Naidu");
            bdls.Add(1, row);
            bdls.Add(2, row2);
            bdls.Add(3, row3);
            bdls.Add(4, row4);
            bdls.Add(5, row5);
            bdls.Add(6, row6);
            // Insert information

            IIndexIterator keys = new IndexIterator(null,6);
            List<int> distinctCols = new List<int> { 2, 4 };
            List<int> rows = bdls.DistinctGetRows(keys, distinctCols);
            Assert.AreEqual(4, rows.Count);

            // now fetch each item ;)
            IBinaryDataListRow myRow;
            bdls.TryGetValue(rows[0], 1, out myRow);
            Assert.IsFalse(myRow.IsEmpty);
            bdls.TryGetValue(rows[3], 1, out myRow);
            Assert.IsFalse(myRow.IsEmpty);
        }

        [TestMethod]
        [TestCategory("DsfUnique")]
        public void DistinctGetValuesWhenHasDistinctValuesShouldReturnOnlyDistinctRows50KOfRows100Col()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());

            // build insert value ;)
            IIndexIterator keys = AddLotsOfRows(bdls);

            List<int> distinctCols = new List<int> { 2, 4 };
            DateTime start = DateTime.Now;
            List<int> rows = bdls.DistinctGetRows(keys, distinctCols);
            DateTime end = DateTime.Now;
            double dif = (end.Ticks - (double)start.Ticks) / TimeSpan.TicksPerSecond;
            Assert.AreEqual(5, rows.Count);
            Assert.IsTrue(dif < 1, string.Format("Time taken: {0}", dif));
            Console.Write(dif);
        }

        [TestMethod]
        [TestCategory("DsfUnique")]
        public void DistinctGetValuesWhenHasDistinctValuesShouldReturnOnlyDistinctRows1MilOfRows100Col()
        {
            BinaryDataListStorage bdls = new BinaryDataListStorage("MySweetNamespace", Guid.NewGuid());

            // build insert value ;)
            IIndexIterator keys = AddLotsOfRows1Mil(bdls);
            // Insert information
            List<int> distinctCols = new List<int> { 2, 4, 5, 7, 120, 134, 99, 78, 34 };
            DateTime start = DateTime.Now;
            List<int> rows = bdls.DistinctGetRows(keys, distinctCols);
            DateTime end = DateTime.Now;
            double dif = (end.Ticks - (double)start.Ticks) / TimeSpan.TicksPerSecond;
            Assert.AreEqual(7, rows.Count);
            Assert.IsTrue(dif < 20, string.Format("Time taken: {0}", dif));
            Console.Write(dif);
        }

        static IIndexIterator AddLotsOfRows(BinaryDataListStorage bdls)
        {
            var row = CreateBinaryDataListRow("1", "1", "Barney", "T", "Buchan");
            var row2 = CreateBinaryDataListRow("2", "2", "Huggs", "W", "Naidu");
            var row3 = CreateBinaryDataListRow("3", "3", "Trav", "A", "Fri");
            var row4 = CreateBinaryDataListRow("4", "4", "Trav", "B", "Fri");
            var row5 = CreateBinaryDataListRow("5", "5", "Huggs", "C", "Bear");
            var row6 = CreateBinaryDataListRow("6", "6", "Huggs", "D", "Naidu");
            bdls.Add(1, row);
            bdls.Add(2, row2);
            bdls.Add(3, row3);
            bdls.Add(4, row4);
            bdls.Add(5, row5);
            bdls.Add(6, row6);
            for (int i = 7; i <= 10; i++)
            {
                     bdls.Add(i, row2);
            }
            for (int i = 11; i <= 20; i++)
            {
                     bdls.Add(i, row3);
            }
            for (int i = 21; i <= 25; i++)
            {
                     bdls.Add(i, row4);
            }
            for (int i = 26; i <= 30; i++)
            {
                     bdls.Add(i, row5);
            }
            for (int i = 31; i <= 100; i++)
            {
                     bdls.Add(i, row2);
            }
            for (int i = 101; i <= 1000; i++)
            {
                     bdls.Add(i, row5);
            }
            for (int i = 1001; i <= 10000; i++)
            {
                bdls.Add(i, row);
            }
            var row7 = CreateBinaryDataListRow("7", "Seven", "Something", "02252", "Someone");
            for (int i = 10001; i <= 50000; i++)
            {
                bdls.Add(i, row7);
            }

            return new IndexIterator(null, 50000);
        }

        static IIndexIterator AddLotsOfRows1Mil(BinaryDataListStorage bdls)
        {
            var row = CreateBinaryDataListRow("1", "1", "Barney", "T", "Buchan");
            var row2 = CreateBinaryDataListRow("2", "2", "Huggs", "W", "Naidu");
            var row3 = CreateBinaryDataListRow("3", "3", "Trav", "A", "Fri");
            var row4 = CreateBinaryDataListRow("4", "4", "Trav", "B", "Fri");
            var row5 = CreateBinaryDataListRow("5", "5", "Huggs", "C", "Bear");
            var row6 = CreateBinaryDataListRow("6", "6", "Huggs", "D", "Naidu");
            bdls.Add(1, row);
            bdls.Add(2, row2);
            bdls.Add(3, row3);
            bdls.Add(4, row4);
            bdls.Add(5, row5);
            bdls.Add(6, row6);
            for (int i = 7; i <= 10; i++)
            {
                     bdls.Add(i, row2);
            }
            for (int i = 11; i <= 20; i++)
            {
                     bdls.Add(i, row3);
            }
            for (int i = 21; i <= 25; i++)
            {
                     bdls.Add(i, row4);
            }
            for (int i = 26; i <= 30; i++)
            {
                     bdls.Add(i, row5);
            }
            for (int i = 31; i <= 100; i++)
            {
                     bdls.Add(i, row2);
            }
            for (int i = 101; i <= 1000; i++)
            {
                     bdls.Add(i, row5);
            }
            for (int i = 1001; i <= 10000; i++)
            {
                bdls.Add(i, row);
            }
            var row7 = CreateBinaryDataListRow("7", "Seven", "Something", "02252", "Someone");
            for (int i = 10001; i <= 50000; i++)
            {
                bdls.Add(i, row7);
            }
            for (int i = 50001; i <= 50100; i++)
            {
                bdls.Add(i, row);
            }
            for (int i = 50101; i <= 51000; i++)
            {
                bdls.Add(i, row2);
            }
            for (int i = 51001; i <= 60000; i++)
            {
                bdls.Add(i, row3);
            }
            for (int i = 60001; i <= 70000; i++)
            {
                bdls.Add(i, row4);
            }
            for (int i = 70001; i <= 80000; i++)
            {
                bdls.Add(i, row5);
            }
            for (int i = 80001; i <= 100000; i++)
            {
                bdls.Add(i, row6);
            }
            for (int i = 100001; i <= 1000000; i++)
            {
                bdls.Add(i, row7);
            }

            return new IndexIterator(null, 1000000);
        }

        static BinaryDataListRow CreateBinaryDataListRow(string col1Value, string col2Value, string col3Value, string col4Value, string col5Value)
        {
            BinaryDataListRow row = new BinaryDataListRow(160);

            row.UpdateValue(col1Value, 0);
            row.UpdateValue(col2Value, 1);
            row.UpdateValue(col3Value, 2);
            row.UpdateValue(col4Value, 3);
            row.UpdateValue(col5Value, 4);
            row.UpdateValue(col1Value, 5);
            row.UpdateValue(col2Value, 6);
            row.UpdateValue(col3Value, 7);
            row.UpdateValue(col4Value, 8);
            row.UpdateValue(col5Value, 9);
            row.UpdateValue(col1Value, 10);
            row.UpdateValue(col2Value, 11);
            row.UpdateValue(col3Value, 12);
            row.UpdateValue(col4Value, 13);
            row.UpdateValue(col5Value, 14);
            row.UpdateValue(col1Value, 15);
            row.UpdateValue(col2Value, 16);
            row.UpdateValue(col3Value, 17);
            row.UpdateValue(col4Value, 18);
            row.UpdateValue(col5Value, 19);
            row.UpdateValue(col1Value, 20);
            row.UpdateValue(col2Value, 21);
            row.UpdateValue(col3Value, 22);
            row.UpdateValue(col4Value, 23);
            row.UpdateValue(col5Value, 24);
            row.UpdateValue(col1Value, 25);
            row.UpdateValue(col2Value, 26);
            row.UpdateValue(col3Value, 27);
            row.UpdateValue(col4Value, 28);
            row.UpdateValue(col5Value, 29);
            row.UpdateValue(col1Value, 30);
            row.UpdateValue(col2Value, 31);
            row.UpdateValue(col3Value, 32);
            row.UpdateValue(col4Value, 33);
            row.UpdateValue(col5Value, 34);
            row.UpdateValue(col1Value, 35);
            row.UpdateValue(col2Value, 36);
            row.UpdateValue(col3Value, 37);
            row.UpdateValue(col4Value, 38);
            row.UpdateValue(col5Value, 39);
            row.UpdateValue(col1Value, 40);
            row.UpdateValue(col2Value, 41);
            row.UpdateValue(col3Value, 42);
            row.UpdateValue(col4Value, 43);
            row.UpdateValue(col5Value, 44);
            row.UpdateValue(col1Value, 45);
            row.UpdateValue(col2Value, 46);
            row.UpdateValue(col3Value, 47);
            row.UpdateValue(col4Value, 48);
            row.UpdateValue(col5Value, 49);
            row.UpdateValue(col1Value, 50);
            row.UpdateValue(col2Value, 51);
            row.UpdateValue(col3Value, 52);
            row.UpdateValue(col4Value, 53);
            row.UpdateValue(col5Value, 54);
            row.UpdateValue(col1Value, 55);
            row.UpdateValue(col2Value, 56);
            row.UpdateValue(col3Value, 57);
            row.UpdateValue(col4Value, 58);
            row.UpdateValue(col5Value, 59);
            row.UpdateValue(col1Value, 60);
            row.UpdateValue(col2Value, 61);
            row.UpdateValue(col3Value, 62);
            row.UpdateValue(col4Value, 63);
            row.UpdateValue(col5Value, 64);
            row.UpdateValue(col1Value, 65);
            row.UpdateValue(col2Value, 66);
            row.UpdateValue(col3Value, 67);
            row.UpdateValue(col4Value, 68);
            row.UpdateValue(col5Value, 69);
            row.UpdateValue(col1Value, 70);
            row.UpdateValue(col2Value, 71);
            row.UpdateValue(col3Value, 72);
            row.UpdateValue(col4Value, 73);
            row.UpdateValue(col5Value, 74);
            row.UpdateValue(col1Value, 75);
            row.UpdateValue(col2Value, 76);
            row.UpdateValue(col3Value, 77);
            row.UpdateValue(col4Value, 78);
            row.UpdateValue(col5Value, 79);
            row.UpdateValue(col1Value, 80);
            row.UpdateValue(col2Value, 81);
            row.UpdateValue(col3Value, 82);
            row.UpdateValue(col4Value, 83);
            row.UpdateValue(col5Value, 84);
            row.UpdateValue(col1Value, 85);
            row.UpdateValue(col2Value, 86);
            row.UpdateValue(col3Value, 87);
            row.UpdateValue(col4Value, 88);
            row.UpdateValue(col5Value, 89);
            row.UpdateValue(col1Value, 90);
            row.UpdateValue(col2Value, 91);
            row.UpdateValue(col3Value, 92);
            row.UpdateValue(col4Value, 93);
            row.UpdateValue(col5Value, 94);
            row.UpdateValue(col1Value, 95);
            row.UpdateValue(col2Value, 96);
            row.UpdateValue(col3Value, 97);
            row.UpdateValue(col4Value, 98);
            row.UpdateValue(col5Value, 99);
            row.UpdateValue(col1Value, 100);
            row.UpdateValue(col2Value, 101);
            row.UpdateValue(col3Value, 102);
            row.UpdateValue(col4Value, 103);
            row.UpdateValue(col5Value, 104);
            row.UpdateValue(col1Value, 105);
            row.UpdateValue(col2Value, 106);
            row.UpdateValue(col3Value, 107);
            row.UpdateValue(col4Value, 108);
            row.UpdateValue(col5Value, 109);
            row.UpdateValue(col1Value, 110);
            row.UpdateValue(col2Value, 111);
            row.UpdateValue(col3Value, 112);
            row.UpdateValue(col4Value, 113);
            row.UpdateValue(col5Value, 114);
            row.UpdateValue(col1Value, 115);
            row.UpdateValue(col2Value, 116);
            row.UpdateValue(col3Value, 117);
            row.UpdateValue(col4Value, 118);
            row.UpdateValue(col5Value, 119);
            row.UpdateValue(col1Value, 120);
            row.UpdateValue(col2Value, 121);
            row.UpdateValue(col3Value, 122);
            row.UpdateValue(col4Value, 123);
            row.UpdateValue(col5Value, 124);
            row.UpdateValue(col1Value, 125);
            row.UpdateValue(col2Value, 126);
            row.UpdateValue(col3Value, 127);
            row.UpdateValue(col4Value, 128);
            row.UpdateValue(col5Value, 129);
            row.UpdateValue(col1Value, 130);
            row.UpdateValue(col2Value, 131);
            row.UpdateValue(col3Value, 132);
            row.UpdateValue(col4Value, 133);
            row.UpdateValue(col5Value, 134);
            row.UpdateValue(col1Value, 135);
            row.UpdateValue(col2Value, 136);
            row.UpdateValue(col3Value, 137);
            row.UpdateValue(col4Value, 138);
            row.UpdateValue(col5Value, 139);
            row.UpdateValue(col1Value, 140);
            row.UpdateValue(col2Value, 141);
            row.UpdateValue(col3Value, 142);
            row.UpdateValue(col4Value, 143);
            row.UpdateValue(col5Value, 144);
            row.UpdateValue(col1Value, 145);
            row.UpdateValue(col2Value, 146);
            row.UpdateValue(col3Value, 147);
            row.UpdateValue(col4Value, 148);
            row.UpdateValue(col5Value, 149);
            row.UpdateValue(col1Value, 150);
            row.UpdateValue(col2Value, 151);
            row.UpdateValue(col3Value, 152);
            row.UpdateValue(col4Value, 153);
            row.UpdateValue(col5Value, 154);
            row.UpdateValue(col1Value, 155);
            row.UpdateValue(col2Value, 156);
            row.UpdateValue(col3Value, 157);
            row.UpdateValue(col4Value, 158);
            row.UpdateValue(col5Value, 159);
            return row;
        }

        #endregion
    }
}
