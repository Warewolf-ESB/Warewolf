
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
using Dev2.DataList.Contract.Binary_Objects.Structs;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Dev2.Data.Tests.BinaryDataList
{
    [TestClass]
    public class SBinaryDataListEntryTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("SBinaryDataListEntry_MoveIndexDataForClone")]
        public void SBinaryDataListEntry_MoveIndexDataForClone_MinMaxAndGapsMove_SameDataBothSides()
        {
            //------------Setup for test--------------------------
            var sBinaryDataListEntry = new SBinaryDataListEntry();
            sBinaryDataListEntry.Init(1);
            sBinaryDataListEntry.ReInstateMinValue(2);
            sBinaryDataListEntry.ReInstateMaxValue(5);
            sBinaryDataListEntry.AddGap(1);
            var targetEntry = new SBinaryDataListEntry();
            targetEntry.Init(1);
            
            //------------Execute Test---------------------------
            targetEntry.MoveIndexDataForClone(sBinaryDataListEntry.Keys.MinIndex(), sBinaryDataListEntry.Keys.MaxIndex(), sBinaryDataListEntry.FetchGaps());

            //------------Assert Results-------------------------

            var targetKeys = targetEntry.Keys;
            var parentKeys = sBinaryDataListEntry.Keys;

            var targetMin = targetKeys.MinIndex();
            var targetMax = targetKeys.MaxIndex();
            var targetCount = targetKeys.Count;

            var parentMin = parentKeys.MinIndex();
            var parentMax = parentKeys.MaxIndex();
            var parentCount = parentKeys.Count;

            // found count we can tell if gaps moved ;)
            Assert.AreEqual(3, parentCount);
            Assert.AreEqual(parentCount, targetCount);

            Assert.AreEqual(parentMin, targetMin);
            Assert.AreEqual(parentMax, targetMax);
            

        }

        [TestMethod]
        public void TestMethod1()
        {
        }
    }
}
