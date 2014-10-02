
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
using Dev2.Runtime.ServiceModel.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Framework.Converters.Graph.String.Json;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests.Runtime.ServiceModel.Data
{
    [TestClass]
    public class RecordsetListHelperTests
    {
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RecordsetListHelper_SplitRecordsetAndFieldNames")]
        public void RecordsetListHelper_SplitRecordsetAndFieldNames_WhenPathContainsEndingRecordset_SingleLevel_ShouldConvertToField()
        {
            //------------Setup for test--------------------------
            var jsonPath = new JsonPath();
            jsonPath.ActualPath = "OneRecordset().AnotherRecset()";
            //------------Execute Test---------------------------
            Tuple<string, string> splitRecordsetAndFieldNames = RecordsetListHelper.SplitRecordsetAndFieldNames(jsonPath);
            //------------Assert Results-------------------------
            Assert.AreEqual("OneRecordset", splitRecordsetAndFieldNames.Item1);
            Assert.AreEqual("AnotherRecset", splitRecordsetAndFieldNames.Item2);
        }  
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RecordsetListHelper_SplitRecordsetAndFieldNames")]
        public void RecordsetListHelper_SplitRecordsetAndFieldNames_WhenPathContainsEndingField_SingleLevel_ShouldConvertToField()
        {
            //------------Setup for test--------------------------
            var jsonPath = new JsonPath();
            jsonPath.ActualPath = "OneRecordset().AnotherRecset()";
            //------------Execute Test---------------------------
            Tuple<string, string> splitRecordsetAndFieldNames = RecordsetListHelper.SplitRecordsetAndFieldNames(jsonPath);
            //------------Assert Results-------------------------
            Assert.AreEqual("OneRecordset", splitRecordsetAndFieldNames.Item1);
            Assert.AreEqual("AnotherRecset", splitRecordsetAndFieldNames.Item2);
        }        
        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RecordsetListHelper_SplitRecordsetAndFieldNames")]
        public void RecordsetListHelper_SplitRecordsetAndFieldNames_WhenPathContainsEndingRecordset_MultiLevel_ShouldConvertToField()
        {
            //------------Setup for test--------------------------
            var jsonPath = new JsonPath();
            jsonPath.ActualPath = "OneRecordset().AnotherRecset().AndAnotherRecset()";
            //------------Execute Test---------------------------
            Tuple<string, string> splitRecordsetAndFieldNames = RecordsetListHelper.SplitRecordsetAndFieldNames(jsonPath);
            //------------Assert Results-------------------------
            Assert.AreEqual("OneRecordset_AnotherRecset", splitRecordsetAndFieldNames.Item1);
            Assert.AreEqual("AndAnotherRecset", splitRecordsetAndFieldNames.Item2);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RecordsetListHelper_SplitRecordsetAndFieldNames")]
        public void RecordsetListHelper_SplitRecordsetAndFieldNames_WhenPathContainsEndingField_MultiLevel_ShouldConvertToField()
        {
            //------------Setup for test--------------------------
            var jsonPath = new JsonPath();
            jsonPath.ActualPath = "OneRecordset().AnotherRecset().AndAnotherRecset";
            //------------Execute Test---------------------------
            Tuple<string, string> splitRecordsetAndFieldNames = RecordsetListHelper.SplitRecordsetAndFieldNames(jsonPath);
            //------------Assert Results-------------------------
            Assert.AreEqual("OneRecordset_AnotherRecset", splitRecordsetAndFieldNames.Item1);
            Assert.AreEqual("AndAnotherRecset", splitRecordsetAndFieldNames.Item2);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("RecordsetListHelper_SplitRecordsetAndFieldNames")]
        public void RecordsetListHelper_SplitRecordsetAndFieldNames_WhenPathContainsScalar_SingleLevel_ShouldConvertToField()
        {
            //------------Setup for test--------------------------
            var jsonPath = new JsonPath();
            jsonPath.ActualPath = "ScalarValue";
            //------------Execute Test---------------------------
            Tuple<string, string> splitRecordsetAndFieldNames = RecordsetListHelper.SplitRecordsetAndFieldNames(jsonPath);
            //------------Assert Results-------------------------
            Assert.AreEqual("", splitRecordsetAndFieldNames.Item1);
            Assert.AreEqual("ScalarValue", splitRecordsetAndFieldNames.Item2);
        }  
    }
}
