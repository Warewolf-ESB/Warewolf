using System;
using Dev2.Common.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace Dev2.Runtime.WebServer
{
    internal static class ServiceTestModelTRXResultBuilder
    {
        public static string BuildTestResultTRX(string ServiceName, List<IServiceTestModelTO> TestResults)
        {
            List<Guid> testIDs = new List<Guid>();
            List<Guid> executionIDs = new List<Guid>();
            foreach (var TestResult in TestResults)
            {
                testIDs.Add(Guid.NewGuid());
                executionIDs.Add(Guid.NewGuid());
            }
            var TestStartDateTime = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
            var TestEndDateTime = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
            var TestDuration = "00:00:00.0000001";
            var TestListID = Guid.NewGuid().ToString();
            var WarewolfServerVersion = "0.0.0.0";
            var WarewolfServerHostname = "localhost";
            var WarewolfServerUsername = "User";
            var TotalTests = TestResults.Count;
            var TotalPassed = TestResults.FindAll((result) => { return result.TestPassed; }).Count;
            var TotalFailed = TestResults.FindAll((result) => { return result.TestFailing; }).Count + TestResults.FindAll((result) => { return result.TestInvalid; }).Count;

            StringBuilder TRXFileContents = new StringBuilder("<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n");
            TRXFileContents.Append("<TestRun id=\"" + Guid.NewGuid().ToString() + "\" name=\"Warewolf Service Tests\" ");
            TRXFileContents.Append("runUser=\"" + WarewolfServerUsername + "\" xmlns=\"http://microsoft.com/schemas/VisualStudio/TeamTest/2010\">");
            TRXFileContents.Append("\n    <Times creation=\"" + TestStartDateTime + "\" queuing=\"" + TestStartDateTime + "\" start=\"" + TestStartDateTime + "\" finish=\"" + TestEndDateTime + "\"");
            TRXFileContents.Append(@" />
    <ResultSummary outcome=""Completed""> 
        <Counters total=""" + TotalTests.ToString() + "\" executed=\"" + TotalTests.ToString() + "\" ");
            TRXFileContents.Append("passed=\"" + TotalPassed.ToString() + "\" error=\"0\" ");
            TRXFileContents.Append("failed=\"" + TotalFailed.ToString() + "\" timeout=\"0\" aborted=\"0\" inconclusive=\"0\" passedButRunAborted=\"0\" notRunnable=\"0\" notExecuted=\"0\" disconnected=\"0\" warning=\"0\" completed=\"0\" inProgress=\"0\" pending=\"0\" />");
            TRXFileContents.Append(@"
    </ResultSummary>
    <TestDefinitions>");
            int testIDIndex = 0;
            foreach (var TestResult in TestResults)
            {
                TRXFileContents.Append(@"
        <UnitTest name=""");
                TRXFileContents.Append(TestResult.TestName.Replace(" ", "_") + "\" storage=\"" + TestResult.TestName.Replace(" ", "_").Replace(" ", "_") + ".dll\" id=\"" + testIDs[testIDIndex] + @""">
            <Execution id=""");
                TRXFileContents.Append(executionIDs[testIDIndex] + @""" />
            <TestMethod ");
                TRXFileContents.Append("codeBase=\"" + ServiceName.Replace(" ", "_") + ".dll\" adapterTypeName=\"Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=14.0.0.0, Culture = neutral, PublicKeyToken=b03f5f7f11d50a3a\" ");
                TRXFileContents.Append("className=\"" + ServiceName.Replace(" ", "_") + ", Version=" + WarewolfServerVersion + ", Culture=neutral, PublicKeyToken=null\" ");
                TRXFileContents.Append("name=\"" + TestResult.TestName.Replace(" ", "_") + @""" />
        </UnitTest>");
                testIDIndex += 1;
            }
            TRXFileContents.Append(@"
    </TestDefinitions>
    <TestLists>
        <TestList name=""Results Not in a List"" id=""");
            TRXFileContents.Append(TestListID + @""" />
        <TestList name = ""All Loaded Results"" id=""19431567-8539-422a-85d7-44ee4e166bda"" />
    </TestLists>
    <TestEntries>");
            testIDIndex = 0;
            foreach (var TestResult in TestResults)
            {
                TRXFileContents.Append("\n        <TestEntry testId=\"");
                TRXFileContents.Append(testIDs[testIDIndex] + "\" executionId=\"");
                TRXFileContents.Append(executionIDs[testIDIndex] + "\" testListId=\"");
                TRXFileContents.Append(TestListID + "\" />");
                testIDIndex += 1;
            }
        TRXFileContents.Append(@"
    </TestEntries>
    <Results>");
            testIDIndex = 0;
            foreach (var TestResult in TestResults)
            {
                TRXFileContents.Append("\n    <UnitTestResult ");
                TRXFileContents.Append("executionId=\"" + executionIDs[testIDIndex] + "\" ");
                TRXFileContents.Append("testId=\"" + testIDs[testIDIndex] + "\" ");
                TRXFileContents.Append("testName=\"" + TestResult.TestName.Replace(" ", "_") + "\" computerName=\"" + WarewolfServerHostname + "\" ");
                TRXFileContents.Append("duration=\"" + TestDuration + "\" ");
                TRXFileContents.Append("startTime=\"" + TestStartDateTime + "\" ");
                TRXFileContents.Append("endTime=\"" + DateTime.Now.ToString("o", CultureInfo.InvariantCulture) + "\" ");
                TRXFileContents.Append("testType =\"13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b\" ");
                TRXFileContents.Append("outcome=\"" + TestResult.Result.RunTestResult.ToString().Replace("Test", "") + "\" ");
                TRXFileContents.Append("testListId=\"" + TestListID + "\" ");
                TRXFileContents.Append("relativeResultsDirectory=\"ca6d373f-8816-4969-8999-3dac700d7626\">\n");
                if (TestResult.TestFailing)
                {
                    var TestResultMessage = System.Web.HttpUtility.HtmlEncode(TestResult.FailureMessage);
                    TRXFileContents.Append(@"
    <Output>
        <ErrorInfo>
            <Message>");
                    TRXFileContents.Append(TestResultMessage);
                    TRXFileContents.Append(@"</Message>
            <StackTrace></StackTrace>
        </ErrorInfo>
    </Output>");
                }
	            TRXFileContents.Append("    </UnitTestResult>");
                testIDIndex += 1;
            }
            TRXFileContents.Append("\n    </Results>\n</TestRun>");
            return TRXFileContents.ToString();
        }
    }
}
