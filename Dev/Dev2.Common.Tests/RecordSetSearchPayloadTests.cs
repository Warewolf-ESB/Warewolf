/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later.
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Warewolf.Data;

namespace Dev2.Common.Tests
{
    [TestClass]
    public class RecordSetSearchPayloadTests
    {
        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RecordSetSearchPayload))]
        public void RecordSetSearchPayload_NoParamsConstractor_SetProperty_ExpectSetValue()
        {
            //--------------------Arrange--------------------
            var testIndex = 0;
            var testPayload = "TestPayload";

            var recordSetSearchPayload = new RecordSetSearchPayload();
            //--------------------Act------------------------
            recordSetSearchPayload.Index = testIndex;
            recordSetSearchPayload.Payload = testPayload;
            //--------------------Assert---------------------
            Assert.AreEqual(testIndex, recordSetSearchPayload.Index);
            Assert.AreEqual(testPayload, recordSetSearchPayload.Payload);
        }

        [TestMethod]
        [Owner("Siphamandla Dube")]
        [TestCategory(nameof(RecordSetSearchPayload))]
        public void RecordSetSearchPayload_WithParamsConstractor_SetProperty_ExpectSetValue()
        {
            //--------------------Arrange--------------------
            var testIndex = 0;
            var testPayload = "TestPayload";
            //--------------------Act------------------------
            var recordSetSearchPayload = new RecordSetSearchPayload(testIndex, testPayload);
            //--------------------Assert---------------------
            Assert.AreEqual(testIndex, recordSetSearchPayload.Index);
            Assert.AreEqual(testPayload, recordSetSearchPayload.Payload);
        }
    }
}
