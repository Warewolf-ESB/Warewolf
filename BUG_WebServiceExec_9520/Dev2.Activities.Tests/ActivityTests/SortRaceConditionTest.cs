using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Dev2.Tests.Activities;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Activities.Statements;
using Unlimited.Applications.BusinessDesignStudio.Activities;
using Dev2;
using System.Xml.Linq;
using ActivityUnitTests.Utils;

namespace ActivityUnitTests.ActivityTest {
    [TestClass]
    public class SortRaceConditionTest : BaseActivityUnitTest {

        //[TestMethod]
        //public void SortRaceCondition_Test1000Executions_ExpectedResultAlwaysCorrectlySorted() {
        //    for(int i = 0; i <= 1000; i++) {

        //        SetupArguments(
        //                        ActivityStrings.SortDataList
        //                      , ActivityStrings.SortDataList
        //                      , "[[recset().Id]]"
        //                      , "Backwards"
        //                      );
        //        IDSFDataObject result = ExecuteProcess();

        //        List<string> expected = new List<string> { "10"
        //                                                 , "9"
        //                                                 , "8"
        //                                                 , "7" 
        //                                                 , "6"
        //                                                 , "4"
        //                                                 , "3"
        //                                                 , "2"
        //                                                 , "1"
        //                                                 , "1" };
        //        string error = string.Empty;
        //        List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset", "Id", out error);


        //        CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        //    }
        //}


        #region Private Test Methods

        private void SetupArguments(string currentDL, string testData, string sortField, string selectedSort) {
            TestStartNode = new FlowStep {
                Action = new DsfSortRecordsActivity { SortField = sortField, SelectedSort = selectedSort }
            };

            CurrentDl = currentDL;
            TestData = testData;
        }

        #endregion Private Test Methods
    }
}