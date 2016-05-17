
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.Convertors.Case;
using Dev2.Common.Interfaces.DB;
using Dev2.Data.Enums;
using Dev2.Factories;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Warewolf.Core;
// ReSharper disable InconsistentNaming

namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    /// <summary>
    /// Summary description for DataGridActivityFindMissingStrategyTests
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataGridActivityFindMissingStrategyTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region BaseConvert Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffBaseConvertActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfBaseConvertActivity baseConvertActivity = new DsfBaseConvertActivity { ConvertCollection = new List<BaseConvertTO> { new BaseConvertTO("[[FromExpression]]", "Text", "Binary", "[[ToExpression]]", 1), new BaseConvertTO("[[FromExpression2]]", "Text", "Binary", "[[ToExpression2]]", 2) } };
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            List<string> actual = strategy.GetActivityFields(baseConvertActivity);
            List<string> expected = new List<string> { "[[FromExpression]]", "[[ToExpression]]", "[[FromExpression2]]", "[[ToExpression2]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        [TestMethod]
        public void GetActivityFieldsOffGatherSystemInfoExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfGatherSystemInformationActivity baseConvertActivity = new DsfGatherSystemInformationActivity{SystemInformationCollection  = new List<GatherSystemInformationTO> {new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "res",1)} };
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            List<string> actual = strategy.GetActivityFields(baseConvertActivity);
            List<string> expected = new List<string> { "res" };
            CollectionAssert.AreEqual(expected, actual);
        }


        #region CaseConvert Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffCaseConvertActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfCaseConvertActivity caseConvertActivity = new DsfCaseConvertActivity { ConvertCollection = new List<ICaseConvertTO> { new CaseConvertTO("[[StringToConvert]]", "UPPER", "[[Result]]", 1), new CaseConvertTO("[[StringToConvert2]]", "UPPER", "[[Result2]]", 2) } };
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            List<string> actual = strategy.GetActivityFields(caseConvertActivity);
            List<string> expected = new List<string> { "[[StringToConvert]]", "[[Result]]", "[[StringToConvert2]]", "[[Result2]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region MultiAssign Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffMultiAssignActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfMultiAssignActivity multiAssignActivity = new DsfMultiAssignActivity { FieldsCollection = new List<ActivityDTO> { new ActivityDTO("[[AssignRight1]]", "[[AssignLeft1]]", 1), new ActivityDTO("[[AssignRight2]]", "[[AssignLeft2]]", 2) } };
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            List<string> actual = strategy.GetActivityFields(multiAssignActivity);
            List<string> expected = new List<string> { "[[AssignRight1]]", "[[AssignLeft1]]", "[[AssignRight2]]", "[[AssignLeft2]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_WebGetActivity_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfWebGetActivity();
            activity.Inputs = new List<IServiceInput> {new ServiceInput("Input1","[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]") , new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> {new ServiceOutputMapping("Output1","OutputValue1","rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.QueryString = "[[qstring]]";
            activity.Headers = new List<INameValue> {new NameValue("Content-Type","[[ctype]]")};
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(10,fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[qstring]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
            Assert.IsTrue(fields.Contains("Content-Type"));
            Assert.IsTrue(fields.Contains("[[ctype]]"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_WebDeleteActivity_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfWebDeleteActivity();
            activity.Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.QueryString = "[[qstring]]";
            activity.Headers = new List<INameValue> { new NameValue("Content-Type", "[[ctype]]") };
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(10, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[qstring]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
            Assert.IsTrue(fields.Contains("Content-Type"));
            Assert.IsTrue(fields.Contains("[[ctype]]"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_WebPutActivity_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfWebPutActivity();
            activity.Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.QueryString = "[[qstring]]";
            activity.Headers = new List<INameValue> { new NameValue("Content-Type", "[[ctype]]") };
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            activity.PutData = "[[putdata]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(11, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[qstring]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
            Assert.IsTrue(fields.Contains("Content-Type"));
            Assert.IsTrue(fields.Contains("[[ctype]]"));
            Assert.IsTrue(fields.Contains("[[putdata]]"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_DotNetDll_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfDotNetDllActivity();
            activity.Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(7, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_SqlServer_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfSqlServerDatabaseActivity();
            activity.Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(7, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_MySql_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfMySqlDatabaseActivity();
            activity.Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(7, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
        }
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_Oracle_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfOracleDatabaseActivity();
            activity.Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(7, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_ODBC_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfODBCDatabaseActivity();
            activity.Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(7, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
        }

        
        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_WebPostActivity_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfWebPostActivity();
            activity.Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") };
            activity.Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") };
            activity.QueryString = "[[qstring]]";
            activity.Headers = new List<INameValue> { new NameValue("Content-Type", "[[ctype]]") };
            activity.OnErrorVariable = "[[err]]";
            activity.OnErrorWorkflow = "[[errSvc]]";
            activity.PostData = "[[data]]";
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(11, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]]"));
            Assert.IsTrue(fields.Contains("[[InputValue2]]"));
            Assert.IsTrue(fields.Contains("[[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[qstring]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
            Assert.IsTrue(fields.Contains("Content-Type"));
            Assert.IsTrue(fields.Contains("[[ctype]]"));
            Assert.IsTrue(fields.Contains("[[data]]"));
        }
    }
}
