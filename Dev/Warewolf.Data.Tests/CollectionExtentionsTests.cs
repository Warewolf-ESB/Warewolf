/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2021 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/



using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace Warewolf.Data.Tests
{
    [TestClass]
    [TestCategory(nameof(CollectionExtentions))]
    public class CollectionExtentionsTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void CollectionExtentions_IsItemDuplicate_ExpectTrue()
        {
            var testItem = "item one";
            var testList = new List<string> { { "item one" } };
            
            var sut = testList.IsItemDuplicate(testItem);

            Assert.IsTrue(sut);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void CollectionExtentions_IsItemDuplicate_False_ExpectNoDuplicates()
        {
            var testItem = "item one";
            var testList = new HashSet<string> { { "item one" } };

            testList.AddItem(testItem, false);

            Assert.AreEqual(1, testList.Count());
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        public void CollectionExtentions_IsItemDuplicate_False_ExpectDuplicates()
        {
            var testItem = "item one";
            var testList = new List<string> { { "item one" } };

            testList.AddItem(testItem, false);

            Assert.AreEqual(2, testList.Count());
        }
    }
}
