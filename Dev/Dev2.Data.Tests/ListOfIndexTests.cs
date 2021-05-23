/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.Binary_Objects;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace Dev2.Data.Tests
{
    [TestClass]
    public class ListOfIndexTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ListOfIndex))]
        public void ListOfIndex_Count_AreEqual_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            List<int> indexes = new List<int>();
            indexes.Add(12);
            indexes.Add(13);
            indexes.Add(14);
            indexes.Add(15);

            var listOfIndex = new ListOfIndex(indexes);
            //-----------------------Act----------------------------
            //-----------------------Assert-------------------------
            Assert.AreEqual(4 , listOfIndex.Count());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ListOfIndex))]
        public void ListOfIndex_GetMaxIndex_AreEqual_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            List<int> indexes = new List<int>();
            indexes.Add(12);
            indexes.Add(13);
            indexes.Add(14);
            indexes.Add(15);

            var listOfIndex = new ListOfIndex(indexes);
            //-----------------------Act----------------------------
            //-----------------------Assert-------------------------
            Assert.AreEqual(15, listOfIndex.GetMaxIndex());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(ListOfIndex))]
        public void ListOfIndex_SetProperty_AreEqual_SetPropertyValue_ExpectTrue()
        {
            //-----------------------Arrange------------------------
            List<int> indexes = new List<int>();
            indexes.Add(12);
            indexes.Add(13);
            indexes.Add(14);
            indexes.Add(15);
            //-----------------------Act----------------------------
            var listOfIndex = new ListOfIndex(indexes)
            {
                MaxValue = 25,
                MinValue = 15
            };
            //-----------------------Assert-------------------------
            Assert.AreEqual(15, listOfIndex.MinValue);
            Assert.AreEqual(25, listOfIndex.MaxValue);
            Assert.AreEqual(4, listOfIndex.Indexes.Count);
        }
    }
}
