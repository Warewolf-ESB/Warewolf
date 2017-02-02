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
            var activity = new DsfWebGetActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                QueryString = "[[qstring]]",
                Headers = new List<INameValue> { new NameValue("Content-Type", "[[ctype]]") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]"
            };
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
            var activity = new DsfWebDeleteActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                QueryString = "[[qstring]]",
                Headers = new List<INameValue> { new NameValue("Content-Type", "[[ctype]]") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]"
            };
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
            var activity = new DsfWebPutActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                QueryString = "[[qstring]]",
                Headers = new List<INameValue> { new NameValue("Content-Type", "[[ctype]]") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]",
                PutData = "[[putdata]]"
            };
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
            var activity = new DsfDotNetDllActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]"
            };
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
            var activity = new DsfSqlServerDatabaseActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]"
            };
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
            var activity = new DsfMySqlDatabaseActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]"
            };
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
            var activity = new DsfOracleDatabaseActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]"
            };
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
        [Owner("Pieter Terblanche")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_PostgreSQL_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfPostgreSqlActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]"
            };
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
            var activity = new DsfODBCDatabaseActivity
            {
                CommandText = "[[InputValue1]] [[InputValue2]][[InputValue3]]",
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]"
            };
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(5, fields.Count);
            Assert.IsTrue(fields.Contains("[[InputValue1]] [[InputValue2]][[InputValue3]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue1]]"));
            Assert.IsTrue(fields.Contains("[[rec().OutputValue2]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_DsfEnhancedDotNetDllActivity_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfEnhancedDotNetDllActivity
            {
                ObjectName = "[[@Home]]",
                ConstructorInputs = new List<IServiceInput>
                {
                    new ServiceInput("name", "[[name]]")
                },
                MethodsToRun = new List<IPluginAction>
                {
                    new PluginAction { OutputVariable = "[[name1]]" },
                    new PluginAction { OutputVariable = "[[@nameObj]]" }
                },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]",
                IsObject = true
            };
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(6, fields.Count);
            Assert.IsTrue(fields.Contains("[[@Home]]"));
            Assert.IsTrue(fields.Contains("[[name]]"));
            Assert.IsTrue(fields.Contains("[[name1]]"));
            Assert.IsTrue(fields.Contains("[[@nameObj]]"));
            Assert.IsTrue(fields.Contains("[[err]]"));
            Assert.IsTrue(fields.Contains("[[errSvc]]"));
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DataGridActivityFindMissingStrategy_GetActivityFields")]
        public void DataGridActivityFindMissingStrategy_GetActivityFields_DsfEnhancedDotNetDllActivityWithMethodWithInputs_ShouldReturnResults()
        {
            //------------Setup for test--------------------------
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.DataGridActivity);
            var activity = new DsfEnhancedDotNetDllActivity
            {
                ObjectName = "[[@Home]]",
                ConstructorInputs = new List<IServiceInput>
                {
                    new ServiceInput("name", "[[name]]")
                },
                MethodsToRun = new List<IPluginAction>
                {
                    new PluginAction { OutputVariable = "[[name1]]" , Inputs = new List<IServiceInput>()
                    {
                        new ServiceInput("name","[[name2]]")
                    } },
                    new PluginAction { OutputVariable = "[[@nameObj]]" }
                },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]",
                IsObject = true
            };
            //------------Execute Test---------------------------
            var fields = strategy.GetActivityFields(activity);
            //------------Assert Results-------------------------
            Assert.AreEqual(7, fields.Count);
            Assert.IsTrue(fields.Contains("[[@Home]]"));
            Assert.IsTrue(fields.Contains("[[name]]"));
            Assert.IsTrue(fields.Contains("[[name1]]"));
            Assert.IsTrue(fields.Contains("[[name2]]"));
            Assert.IsTrue(fields.Contains("[[@nameObj]]"));
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
            var activity = new DsfWebPostActivity
            {
                Inputs = new List<IServiceInput> { new ServiceInput("Input1", "[[InputValue1]]"), new ServiceInput("Input2", "[[InputValue2]]"), new ServiceInput("Input3", "[[InputValue3]]") },
                Outputs = new List<IServiceOutputMapping> { new ServiceOutputMapping("Output1", "OutputValue1", "rec"), new ServiceOutputMapping("Output2", "OutputValue2", "rec") },
                QueryString = "[[qstring]]",
                Headers = new List<INameValue> { new NameValue("Content-Type", "[[ctype]]") },
                OnErrorVariable = "[[err]]",
                OnErrorWorkflow = "[[errSvc]]",
                PostData = "[[data]]"
            };
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
