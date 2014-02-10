using System;
using System.Activities;
using System.Activities.Statements;
using System.Text.RegularExpressions;
using ActivityUnitTests;
using Dev2.Data.Enums;
using Dev2.Integration.Tests.Helpers;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Integration.Tests.Dev2.Activities.Tests
{

    /// <summary>
    /// Summary description for DsfForEachActivityWFTests
    /// </summary>
    [TestClass]
    public class DsfForEachActivityWFTests : BaseActivityUnitTest
    {
        readonly string WebserverURI = ServerSettings.WebserverURI;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }

        #region ForEach Behaviour Tests

        // Blocked by Bug 7926

        [TestMethod]
        public void ForEachNestedWorkFlow()
        {

            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachNestedForEachTest");
            const string expected = @"<innerScalar>11</innerScalar>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected, StringComparison.Ordinal));
        }

        [TestMethod]
        public void ForEachRecordsetIndexNotToBeReplacedWorkFlow()
        {

            string PostData = String.Format("{0}{1}", WebserverURI, "ForEachWithStarAndStaticIndex");
            const string expected = @"DataList><results><res>50</res></results></DataList";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected, StringComparison.Ordinal));
        }

        #endregion ForEach Behaviour Tests

        #region Iteration Number Tests

        [TestMethod]
        public void ForEachNumber()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachNumber");
            const string expected = "<Rec><Each>1</Each></Rec><Rec><Each>2</Each></Rec><Rec><Each>4</Each></Rec><Rec><Each>8</Each></Rec><Rec><Each>16</Each></Rec>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);
            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected, StringComparison.Ordinal));
        }


        // Sashen: 28-01-2012 : Once the fix is made and this test passes, please put your name and a comment regarding the test.
        // Bug 8366
        [TestMethod]
        public void ForEachAssign_Expected_AssignWorksForEveryIteration()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachAssign");
            string expected = @"<Result> Dummy_String Dummy_String_Inner Dummy_String_Inner Dummy_String_Inner Dummy_String_Inner</Result>    <Input>Dummy_String_Inner</Input>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Regex regex = new Regex(@">\s*<");

            expected = regex.Replace(expected, "><");
            ResponseData = regex.Replace(ResponseData, "><");

            StringAssert.Contains(ResponseData, expected);
        }

        #endregion Iteration Number Tests

        #region Scalar Tests

        [TestMethod]
        // TODO : Update WF in TFS
        public void ForEachInputOutputMappingTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "NewForEachScalarTest");
            const string expected = @"<var>5</var><recset><rec1>1</rec1></recset><recset><rec1>2</rec1></recset><recset><rec1>3</rec1></recset><recset><rec1>4</rec1></recset><recset><rec1>5</rec1></recset><recset><rec1>6</rec1></recset>";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected, StringComparison.Ordinal));
        }

        #endregion Scalar Tests

        #region All Tools Test

        [TestMethod]
        public void ForEachAllToolsTest()
        {
            string PostData = String.Format("{0}{1}", WebserverURI, "ForEachUpgradeTest");
            string expected = @"PASS";

            string ResponseData = TestHelper.PostDataToWebserver(PostData);

            Assert.AreNotEqual(-1, ResponseData.IndexOf(expected), "Expected [ " + expected + "] Got [ " + ResponseData + " ]");
        }

        #endregion Scalar Tests

        #region Private Test Methods

        private DsfActivity CreateWorkflow()
        {
            DsfActivity activity = new DsfActivity
            {
                ServiceName = "MyTestService",
                InputMapping = TestResource.ForEach_Input_Mapping,
                OutputMapping = TestResource.ForEach_Output_Mapping
            };

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        private DsfActivity CreateWorkflow(string mapping, bool isInputMapping)
        {
            DsfActivity activity = new DsfActivity();
            if(isInputMapping)
            {
                activity.InputMapping = mapping;
                activity.OutputMapping = TestResource.ForEach_Output_Mapping;
            }
            else
            {
                activity.InputMapping = TestResource.ForEach_Input_Mapping;
                activity.OutputMapping = mapping;
            }
            activity.ServiceName = "MyTestService";

            TestData = "<ADL><innerrecset><innerrec></innerrec><innerrec2></innerrec2><innerdate></innerdate></innerrecset><innertesting><innertest></innertest></innertesting><innerScalar></innerScalar></ADL>";

            return activity;
        }

        private void SetupArguments(string currentDL, string testData, enForEachType type, bool isInputMapping = false, string inputMapping = null, string from = null, string to = null, string csvIndexes = null, string numberExecutions = null)
        {
            var activityFunction = new ActivityFunc<string, bool>();
            DsfActivity activity;
            if(inputMapping != null)
            {
                activity = CreateWorkflow(inputMapping, isInputMapping);
            }
            else
            {
                activity = CreateWorkflow();
            }

            activityFunction.Handler = activity;

            TestStartNode = new FlowStep
            {
                Action = new DsfForEachActivity
                {
                    DataFunc = activityFunction,
                    ForEachType = type,
                    NumOfExections = numberExecutions,
                    From = from,
                    To = to,
                    CsvIndexes = csvIndexes,
                }
            };

            CurrentDl = testData;
            TestData = currentDL;
        }

        #endregion
    }
}
