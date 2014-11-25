
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/


using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using Dev2.Activities;
using Dev2.Enums;
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
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class MixedActivityFindMissingStrategyTests
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region DataSplit Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDataSplitActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfDataSplitActivity dataSplitActivity = new DsfDataSplitActivity();
            dataSplitActivity.OnErrorVariable = "[[onErr]]";
            dataSplitActivity.ResultsCollection = new List<DataSplitDTO> { new DataSplitDTO("[[OutputVariable1]]", "Index", "[[At1]]", 1) { EscapeChar = "[[Escaped1]]" }, new DataSplitDTO("[[OutputVariable2]]", "Index", "[[At2]]", 2) { EscapeChar = "[[Escaped2]]" } };
            dataSplitActivity.SourceString = "[[SourceString]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);
            List<string> actual = strategy.GetActivityFields(dataSplitActivity);
            List<string> expected = new List<string> { "[[Escaped1]]", "[[OutputVariable1]]", "[[At1]]", "[[Escaped2]]", "[[OutputVariable2]]", "[[At2]]", "[[SourceString]]", "[[onErr]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion

        #region DataMerge Activity Tests

        [TestMethod]
        public void GetActivityFieldsOffDataMergeActivityExpectedAllFindMissingFieldsToBeReturned()
        {
            DsfDataMergeActivity dataMergeActivity = new DsfDataMergeActivity();
            dataMergeActivity.OnErrorVariable = "[[onErr]]";
            dataMergeActivity.MergeCollection = new List<DataMergeDTO> { new DataMergeDTO("[[InputVariable1]]", "None", "[[At1]]", 1, "[[Padding1]]", "Left"), new DataMergeDTO("[[InputVariable2]]", "None", "[[At2]]", 2, "[[Padding2]]", "Left") };
            dataMergeActivity.Result = "[[Result]]";
            Dev2FindMissingStrategyFactory fac = new Dev2FindMissingStrategyFactory();
            IFindMissingStrategy strategy = fac.CreateFindMissingStrategy(enFindMissingType.MixedActivity);
            List<string> actual = strategy.GetActivityFields(dataMergeActivity);
            List<string> expected = new List<string> { "[[Padding1]]", "[[InputVariable1]]", "[[At1]]", "[[Padding2]]", "[[InputVariable2]]", "[[At2]]", "[[Result]]", "[[onErr]]" };
            CollectionAssert.AreEqual(expected, actual);
        }

        #endregion


        [TestMethod]
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
    }
}
