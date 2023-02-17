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
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Warewolf.Storage.Tests
{
    [TestClass]
    public class WarewolfAtomIteratorTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfAtomIterator))]
        public void WarewolfAtomIterator_GetLength_ShouldBeEqualToMaxVal_AreEqual_ExpectTrue()
        {
            //-------------------------Arrange--------------------------
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>();
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            //-------------------------Act------------------------------
            var length = warewolfAtomIterator.GetLength();
            //-------------------------Assert---------------------------
            Assert.AreEqual(0, length);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfAtomIterator))]
        public void WarewolfAtomIterator_GetNextValue_AreEqual_ExpectTrue()
        {
            //-------------------------Arrange--------------------------
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>
            {
                DataStorage.WarewolfAtom.NewDataString("a"),
                DataStorage.WarewolfAtom.NewDataString("b"),
                DataStorage.WarewolfAtom.NewDataString("c")
            };
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            //-------------------------Act------------------------------
            var value = warewolfAtomIterator.GetNextValue();
            //-------------------------Assert---------------------------
            Assert.AreEqual(listResult.First().ToString(), value);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfAtomIterator))]
        public void WarewolfAtomIterator_GetNextValue_IsNotNull_ExpectTrue()
        {
            //-------------------------Arrange--------------------------
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>();
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            //-------------------------Act------------------------------
            warewolfAtomIterator.GetNextValue();
            //-------------------------Assert---------------------------
            Assert.IsNotNull(warewolfAtomIterator);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(WarewolfAtomIterator))]
        public void WarewolfAtomIterator_HasMoreData_GivenEmptyListResult_ExpectFalse()
        {
            //-------------------------Arrange--------------------------
            IEnumerable<DataStorage.WarewolfAtom> listResult = new List<DataStorage.WarewolfAtom>();
            var warewolfAtomIterator = new WarewolfAtomIterator(listResult);
            //-------------------------Assert---------------------------
            var hasMoreData = warewolfAtomIterator.HasMoreData();
            //-------------------------Act------------------------------
            Assert.IsFalse(hasMoreData);
        }
    }
}