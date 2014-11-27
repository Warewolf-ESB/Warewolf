
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
using System.Diagnostics.CodeAnalysis;
using Dev2.Runtime;
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    // PBI: 801
    // BUG: 8477

    /// <author>trevor.williams-ros</author>
    /// <date>2013/02/13</date>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class RecordsetRecordTests
    {
        #region CTOR

        [TestMethod]
        public void ConstructorWithNoParametersExpectedCreatesEmpty()
        {
            var record = new RecordsetRecord();
            Assert.AreEqual(0, record.Count);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void ConstructorWithNullExpectedThrowsArgumentNullException()
        {
            // ReSharper disable ObjectCreationAsStatement
            new RecordsetRecord(null);
            // ReSharper restore ObjectCreationAsStatement
        }

        [TestMethod]
        public void ConstructorWithRecordsetCellsExpectedAddsRecordsetCells()
        {
            var cells = new[]
            {
                new RecordsetCell(),
                new RecordsetCell()
            };
            var record = new RecordsetRecord(cells);
            Assert.AreEqual(2, record.Count);
            Assert.AreSame(cells[0], record[0]);
            Assert.AreSame(cells[1], record[1]);
        }

        #endregion

        #region AddRange

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddRangeWithNullExpectedThrowsArgumentNullException()
        {
            var record = new RecordsetRecord();
            record.AddRange(null);
        }

        [TestMethod]
        public void AddRangeWithRecordsetCellsExpectedAddsRecordsetCells()
        {
            var record = new RecordsetRecord();
            var cells = new[]
            {
                new RecordsetCell(),
                new RecordsetCell()
            };
            record.AddRange(cells);
            Assert.AreEqual(2, record.Count);
            Assert.AreSame(cells[0], record[0]);
            Assert.AreSame(cells[1], record[1]);
        }
        #endregion

        #region Add

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void AddWithNullExpectedThrowsArgumentNullException()
        {
            var record = new RecordsetRecord();
            record.Add(null);
        }

        [TestMethod]
        public void AddWithRecordsetCellExpectedAddsRecordsetCell()
        {
            var record = new RecordsetRecord();
            var cell = new RecordsetCell();
            record.Add(cell);

            Assert.AreEqual(1, record.Count);
            Assert.AreSame(cell, record[0]);
        }

        #endregion

        #region Clear

        [TestMethod]
        public void ClearWithCellsExpectedRemovesCells()
        {
            var record = new RecordsetRecord(new[]
            {
                new RecordsetCell(),
                new RecordsetCell()
            });
            Assert.AreEqual(2, record.Count);
            record.Clear();
            Assert.AreEqual(0, record.Count);
        }

        #endregion

        #region JsonSerialization

        [TestMethod]
        public void JsonSerializationExpectedDoesNotImplementIEnumerable()
        {
            var record = new RecordsetRecord();
            var enumerable = record as IEnumerable<RecordsetCell>;
            Assert.IsNull(enumerable);
        }

        [TestMethod]
        public void JsonSerializationExpectedIncludesLabel()
        {
            var record = new RecordsetRecord
            {
                Label = "TestRec(2)"
            };
            var random = new Random();
            for(var i = 0; i < 10; i++)
            {
                var colName = "Column" + (i + 1);
                record.Add(new RecordsetCell
                {
                    Name = record.Label + "." + colName,
                    Value = random.GenerateString(30, string.Empty, true)
                });
            }
            var jsonStr = JsonConvert.SerializeObject(record);
            dynamic jsonObj = JsonConvert.DeserializeObject(jsonStr);
            Assert.AreEqual(record.Label, jsonObj.Label.Value);
        }

        #endregion


    }
}
