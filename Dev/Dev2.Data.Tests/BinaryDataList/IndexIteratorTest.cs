
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using Dev2.Data.Binary_Objects;

namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class IndexIteratorTest
    {
        [TestMethod]
        public void CanIteratorNormally()
        {
            HashSet<int> gaps = new HashSet<int>();
            IndexIterator ii = new IndexIterator(gaps, 100);
            int cnt = 0;
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
            HashSet<int> gaps = new HashSet<int>(new List<int>{1});
            int maxValue = 100;
            IndexIterator ii = new IndexIterator(gaps, 100);
            int cnt = 0;
            int firstIdx = -1;
            while (ii.HasMore())
            {
                int val = ii.FetchNextIndex();
                if (cnt == 0)
                {
                    firstIdx = val;
                }
                cnt++;
            }

            Assert.AreEqual(cnt, (maxValue - gaps.Count));
            Assert.AreEqual(2, firstIdx);
        }

        [TestMethod]
        public void CanIteratorWithGapAt1_PlusGapsEvery10()
        {
            HashSet<int> gaps = new HashSet<int>(new List<int> { 1, 11, 21, 31, 41, 51, 61, 71, 81, 91 });
            int maxValue = 100;
            IndexIterator ii = new IndexIterator(gaps, 100);
            int cnt = 0;
            int firstIdx = -1;
            while (ii.HasMore())
            {
                int val = ii.FetchNextIndex();
                if (cnt == 0)
                {
                    firstIdx = val;
                }
                cnt++;
            }

            Assert.AreEqual(cnt, (maxValue - gaps.Count));
            Assert.AreEqual(2, firstIdx);
        }

        [TestMethod]
        public void MaxValueIsCorrectWhenCurrentValueInGaps()
        {
            HashSet<int> gaps = new HashSet<int>(new List<int> { 2 });
            IndexIterator ii = new IndexIterator(gaps, 2);

            
            Assert.AreEqual(1,ii.MaxIndex());
        }


        [TestMethod]
        [TestCategory("IndexIterator,UnitTest")]
        [Owner("Travis")]
        [Description("A test to ensure we quite visiting this 1 based indexing issue ;)")]
        public void IsEmptyIsCorrectWhenTwoElementAndIndex2Removed()
        {
            HashSet<int> gaps = new HashSet<int>(new List<int> { 2 });
            IndexIterator ii = new IndexIterator(gaps, 2);

            Assert.AreEqual(1, ii.MaxIndex());
            Assert.IsFalse(ii.IsEmpty);
        }


        [TestMethod]
        [TestCategory("IndexIterator,UnitTest")]
        [Owner("Travis")]
        [Description("A test to ensure we quite visiting this 1 based indexing issue ;)")]
        public void IsEmptyIsCorrectWhenThreeElementAndIndex2And3Removed()
        {
            HashSet<int> gaps = new HashSet<int>(new List<int> { 2,3 });
            IndexIterator ii = new IndexIterator(gaps, 3);

            Assert.AreEqual(1, ii.MaxIndex());
            Assert.IsFalse(ii.IsEmpty);
        }
    }
}
