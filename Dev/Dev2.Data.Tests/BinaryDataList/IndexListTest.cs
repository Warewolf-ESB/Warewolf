/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    public class IndexListTest
    {
        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure the IndexList can init properly")]
        [TestCategory("UnitTest,IndexList")]
        public void IndexList_UnitTest_CanInitNormally()
        {

            IndexList il = new IndexList(null, 5);

            Assert.AreEqual(1, il.MinValue);
            Assert.AreEqual(5, il.MaxValue);
        }

        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure the IndexList can init properly")]
        [TestCategory("UnitTest,IndexList")]
        public void IndexList_UnitTest_CanInitWithGaps()
        {

            HashSet<int> gaps = new HashSet<int> { 1, 3 };
            IndexList il = new IndexList(gaps, 5);

            Assert.AreEqual(1, il.MinValue);
            Assert.AreEqual(5, il.MaxValue);
            Assert.AreEqual(3, il.Count());
        }
        
        [TestMethod]
        [Owner("Travis")]
        [Description("Ensure the IndexList can count correctly when the min value is not 1")]
        [TestCategory("UnitTest,IndexList")]
        public void IndexList_UnitTest_CanCountCorrectlyWhenMinValueGreaterThan1()
        {

            HashSet<int> gaps = new HashSet<int> { 1, 5 };
            IndexList il = new IndexList(gaps, 4, 3);

            Assert.AreEqual(3, il.MinValue);
            Assert.AreEqual(4, il.MaxValue);
            Assert.AreEqual(1, il.Count());
        }
        
    }
}
