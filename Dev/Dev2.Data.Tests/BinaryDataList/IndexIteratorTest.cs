/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;


namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    public class IndexIteratorTest
    {
        [TestMethod]
        public void CanIteratorNormally()
        {
            var gaps = new HashSet<int>();
            var ii = new IndexIterator(gaps, 100);
            var cnt = 0;
            while (ii.HasMore())
            {
                ii.FetchNextIndex();
                cnt++;
            }

            Assert.AreEqual(cnt, 100);
        }

        [TestMethod]
        public void CanIteratorWithGapAt1()
        {
            var gaps = new HashSet<int>(new List<int>{1});
            const int maxValue = 100;
            var ii = new IndexIterator(gaps, 100);
            var cnt = 0;
            var firstIdx = -1;
            while (ii.HasMore())
            {
                var val = ii.FetchNextIndex();
                if (cnt == 0)
                {
                    firstIdx = val;
                }
                cnt++;
            }

            Assert.AreEqual(cnt, maxValue - gaps.Count);
            Assert.AreEqual(2, firstIdx);
        }

        [TestMethod]
        public void CanIteratorWithGapAt1_PlusGapsEvery10()
        {
            var gaps = new HashSet<int>(new List<int> { 1, 11, 21, 31, 41, 51, 61, 71, 81, 91 });
            const int maxValue = 100;
            var ii = new IndexIterator(gaps, 100);
            var cnt = 0;
            var firstIdx = -1;
            while (ii.HasMore())
            {
                var val = ii.FetchNextIndex();
                if (cnt == 0)
                {
                    firstIdx = val;
                }
                cnt++;
            }

            Assert.AreEqual(cnt, maxValue - gaps.Count);
            Assert.AreEqual(2, firstIdx);
        }

        [TestMethod]
        public void MaxValueIsCorrectWhenCurrentValueInGaps()
        {
            var gaps = new HashSet<int>(new List<int> { 2 });
            var ii = new IndexIterator(gaps, 2);


            Assert.AreEqual(1,ii.MaxIndex());
        }


        [TestMethod]
        [TestCategory("IndexIterator,UnitTest")]
        [Owner("Travis")]
        [Description("A test to ensure we quite visiting this 1 based indexing issue ;)")]
        public void IsEmptyIsCorrectWhenTwoElementAndIndex2Removed()
        {
            var gaps = new HashSet<int>(new List<int> { 2 });
            var ii = new IndexIterator(gaps, 2);

            Assert.AreEqual(1, ii.MaxIndex());
            Assert.IsFalse(ii.IsEmpty);
        }


        [TestMethod]
        [TestCategory("IndexIterator,UnitTest")]
        [Owner("Travis")]
        [Description("A test to ensure we quite visiting this 1 based indexing issue ;)")]
        public void IsEmptyIsCorrectWhenThreeElementAndIndex2And3Removed()
        {
            var gaps = new HashSet<int>(new List<int> { 2,3 });
            var ii = new IndexIterator(gaps, 3);

            Assert.AreEqual(1, ii.MaxIndex());
            Assert.IsFalse(ii.IsEmpty);
        }
    }
}
