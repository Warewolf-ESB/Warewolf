/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System.Collections.Generic;
using Dev2.Activities;
using Dev2.Factories;
using Dev2.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.TO;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Tests.Activities.FindMissingStrategyTest
{
    /// <summary>
    /// Summary description for MixedActivityFindMissingStrategyTests
    /// </summary>
    [TestClass]
    
    public class MixedActivityFindMissingStrategyTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region DataSplit Activity Tests

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOffDataSplitActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var dataSplitActivity = new DsfDataSplitActivity();
            dataSplitActivity.OnErrorVariable = "[[onErr]]";
            dataSplitActivity.ResultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[OutputVariable1]]", "Index", "[[At1]]", 1) { EscapeChar = "[[Escaped1]]" }, new DataSplitDTO("[[OutputVariable2]]", "Index", "[[At2]]", 2) { EscapeChar = "[[Escaped2]]" } };
            dataSplitActivity.SourceString = "[[SourceString]]";
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);
            var actual = strategy.GetActivityFields(dataSplitActivity);
            var expected = new List<string> { "[[Escaped1]]", "[[OutputVariable1]]", "[[At1]]", "[[Escaped2]]", "[[OutputVariable2]]", "[[At2]]", "[[SourceString]]", "[[onErr]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region DataMerge Activity Tests

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOffDataMergeActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            var dataMergeActivity = new DsfDataMergeActivity();
            dataMergeActivity.OnErrorVariable = "[[onErr]]";
            dataMergeActivity.MergeCollection = new List<DataMergeDTO> { new DataMergeDTO("[[InputVariable1]]", "None", "[[At1]]", 1, "[[Padding1]]", "Left"), new DataMergeDTO("[[InputVariable2]]", "None", "[[At2]]", 2, "[[Padding2]]", "Left") };
            dataMergeActivity.Result = "[[Result]]";
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);
            var actual = strategy.GetActivityFields(dataMergeActivity);
            var expected = new List<string> { "[[Padding1]]", "[[InputVariable1]]", "[[At1]]", "[[Padding2]]", "[[InputVariable2]]", "[[At2]]", "[[Result]]", "[[onErr]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion


        [TestMethod]
        [Timeout(60000)]
        [Owner("Trevor Williams-Ros")]
        [TestCategory("MixedActivityFindMissingStrategy_GetActivityFields")]
        public void MixedActivityFindMissingStrategy_GetActivityFields_DsfSqlBulkInsertActivity_AllFindMissingFieldsToBeReturned()
        {
            //------------Setup for test--------------------------
            var activity = new DsfSqlBulkInsertActivity
                {
                    Result = "[[Result]]", OnErrorVariable = "[[onErr]]", InputMappings = new List<DataColumnMapping>
                        {
                            new DataColumnMapping { InputColumn = "[[rs().Field1]]", OutputColumn = new DbColumn() },
                            new DataColumnMapping { InputColumn = "[[rs().Field2]]", OutputColumn = new DbColumn() },
                        }
                };

            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);

            //------------Execute Test---------------------------
            var actual = strategy.GetActivityFields(activity);

            //------------Assert Results-------------------------
            var expected = new List<string> { "[[rs().Field1]]", "[[rs().Field2]]", "[[Result]]", "[[onErr]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOf_DsfCreateJsonActivity_ExpectedAllFindMissingFieldsToBeReturned()
        {
            var act = new DsfCreateJsonActivity {
                JsonMappings = new List<JsonMappingTo>
                {
                    new JsonMappingTo("[[sourceName1]]", 1, true),
                    new JsonMappingTo("[[sourceName2]]", 2, true),
                },
                OnErrorVariable = "[[onErr]]",
                JsonString = "[[SourceString]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);
            var actual = strategy.GetActivityFields(act);
            var expected = new List<string> { "[[sourceName1]]", "[[sourceName2]]", "[[SourceString]]", "[[onErr]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOf_DsfXPathActivity_ExpectedAllFindMissingFieldsToBeReturned()
        {
            var act = new DsfXPathActivity
            {
                ResultsCollection = new List<XPathDTO>
                {
                    new XPathDTO("[[outputVar1]]", "//the/path1", 1),
                    new XPathDTO("[[outputVar2]]", "//the/path2", 2)
                },
                OnErrorVariable = "[[onErr]]",
                SourceString = "[[SourceString]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);
            var actual = strategy.GetActivityFields(act);
            var expected = new List<string> { "[[outputVar1]]", "//the/path1", "[[outputVar2]]", "//the/path2", "[[SourceString]]", "[[onErr]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOf_DsfFindRecordsMultipleCriteriaActivity_ExpectedAllFindMissingFieldsToBeReturned()
        {
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                ResultsCollection = new List<FindRecordsTO>
                {
                    new FindRecordsTO("criteria1", "any", 1),
                    new FindRecordsTO("criteria2", "any", 2)
                },
                OnErrorVariable = "[[onErr]]",
                FieldsToSearch = "field1",
                Result = "[[result]]"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);
            var actual = strategy.GetActivityFields(act);
            var expected = new List<string> { "", "", "criteria1", "", "", "criteria2", "field1", "[[result]]", "[[onErr]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        [TestMethod]
        [Timeout(60000)]
        public void GetActivityFieldsOf_NativeActivity_ExpectErrorWorkflowFieldToBeReturned()
        {
            var act = new DsfFindRecordsMultipleCriteriaActivity
            {
                ResultsCollection = new List<FindRecordsTO>
                {
                    new FindRecordsTO("criteria1", "any", 1),
                    new FindRecordsTO("criteria2", "any", 2)
                },
                OnErrorVariable = "[[onErr]]",
                FieldsToSearch = "field1",
                Result = "[[result]]",
                OnErrorWorkflow = "errorWorkflow"
            };
            var fac = new Dev2FindMissingStrategyFactory();
            var strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);
            var actual = strategy.GetActivityFields(act);
            var expected = new List<string> { "", "", "criteria1", "", "", "criteria2", "field1", "[[result]]", "[[onErr]]", "errorWorkflow" };
            CollectionAssert.AreEqual(expected, actual);
        }
    }
}
