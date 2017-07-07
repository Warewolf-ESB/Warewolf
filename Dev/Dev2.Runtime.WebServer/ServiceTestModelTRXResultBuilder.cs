using System;
using Dev2.Common.Interfaces;
using System.Collections.Generic;
using System.Globalization;

namespace Dev2.Runtime.WebServer
{
    internal static class ServiceTestModelTRXResultBuilder
    {
        public static string BuildTestResultTRX(List<IServiceTestModelTO> testResults)
        {
            List<Guid> testIDs = new List<Guid>();
            List<Guid> executionIDs = new List<Guid>();
            foreach (var TestResult in testResults)
            {
                testIDs.Add(Guid.NewGuid());
                executionIDs.Add(Guid.NewGuid());
            }
            var Duration = "00:00:00.0000001";
            var ServerUsername = "User";
            var TestStartDateTime = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
            var TestEndDateTime = DateTime.Now.ToString("o", CultureInfo.InvariantCulture);
            var TestListID = Guid.NewGuid().ToString();
            var TRXFileContents = @"<?xml version=""1.0"" encoding=""UTF-8""?>
    <TestRun id = """;
            TRXFileContents += Guid.NewGuid().ToString() + @""" name=""Warewolf Service Tests"" runUser=""";
            TRXFileContents += ServerUsername + @""" xmlns=""http://microsoft.com/schemas/VisualStudio/TeamTest/2010"">
    <TestSettings name = ""Default Test Settings"" id = """;
            TRXFileContents += Guid.NewGuid().ToString() + @""">
      <Execution>
        <TestTypeSpecific/>
      </Execution > 
      <Deployment enabled=""false"" />  
      <Properties />  
    </TestSettings>  
    <Times creation = """;
            TRXFileContents += TestStartDateTime + "\" queuing=\"" + TestStartDateTime + "\" start=\"" + TestStartDateTime + "\" finish=\"" + TestEndDateTime + "\"";
            TRXFileContents += @" />
    <ResultSummary outcome=""Completed""> 
        <Counters total=""";
            TRXFileContents += testResults.Count.ToString() + @""" executed=""";
            TRXFileContents += testResults.Count.ToString() + @""" passed=""";
            TRXFileContents += testResults.FindAll((result) => { return result.TestPassed; }).Count.ToString() + @""" error=""0"" failed=""";
            TRXFileContents += testResults.FindAll((result) => { return result.TestFailing; }).Count.ToString() + @""" timeout=""0"" aborted=""0"" inconclusive=""0"" passedButRunAborted=""0"" notRunnable=""0"" notExecuted=""0"" disconnected=""0"" warning=""0"" completed=""0"" inProgress=""0"" pending=""0"" />
    </ResultSummary>
    <TestDefinitions>";
            int testIDIndex = 0;
            foreach (var TestResult in testResults) {
                TRXFileContents += @"
        <UnitTest name=""";
                TRXFileContents += TestResult.TestName + @""" storage=""";
                TRXFileContents += TestResult.TestName + @".dll"" id=""";
                TRXFileContents += testIDs[testIDIndex] + @""">
            <Execution id=""";
                TRXFileContents += executionIDs[testIDIndex] + @""" />
            <TestMethod codeBase=""";
                TRXFileContents += TestResult.TestName + @".dll"" adapterTypeName=""Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version = 14.0.0.0, Culture = neutral, PublicKeyToken = b03f5f7f11d50a3a"" className=""";
                TRXFileContents += TestResult.TestName + @", Version=WarewolfServerVersion, Culture=neutral, PublicKeyToken=null"" name=""";
                TRXFileContents += TestResult.TestName + @""" />
        </UnitTest>";
                testIDIndex += 1;
            }
            TRXFileContents += @"
    </TestDefinitions>
    <TestLists>
        <TestList name=""Results Not in a List"" id=""";
            TRXFileContents += TestListID + @""" />
        <TestList name = ""All Loaded Results"" id=""19431567-8539-422a-85d7-44ee4e166bda"" />   
    </TestLists>   
    <TestEntries>";
            testIDIndex = 0;
            foreach (var TestResult in testResults) {
            TRXFileContents += @"
        <TestEntry testId=""";
                TRXFileContents += testIDs[testIDIndex] + @""" executionId=""";
                TRXFileContents += executionIDs[testIDIndex] + @""" testListId=""";
                TRXFileContents += TestListID + @""" />";
                testIDIndex += 1;
            }
        TRXFileContents += @"
    </TestEntries>
    <Results>";
            testIDIndex = 0;
            foreach (var TestResult in testResults) {
                TRXFileContents += "\n    <UnitTestResult executionId=\"";
                TRXFileContents += executionIDs[testIDIndex] + "\" testId=\"";
                TRXFileContents += testIDs[testIDIndex] + "\" testName=\"";
                TRXFileContents += TestResult.TestName + "\" computerName=\"ServerPath\" duration=\"";
                TRXFileContents += Duration + "\" startTime=\"";
                TRXFileContents += TestStartDateTime + "\" endTime=\"";
                TRXFileContents += DateTime.Now.ToString("o", CultureInfo.InvariantCulture) + "\" testType=\"13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b\" outcome=\"";
                TRXFileContents += TestResult.Result.RunTestResult.ToString().Replace("Test", "") + "\" testListId=\"";
                TRXFileContents += TestListID + "\" relativeResultsDirectory=\"ca6d373f-8816-4969-8999-3dac700d7626\">";
                if (TestResult.Result.RunTestResult.ToString().Replace("Test", "") == "Failed") {
                        var TestResultMessage = System.Web.HttpUtility.HtmlEncode(TestResult.FailureMessage);
                        TRXFileContents += @"
    <Output>
        <ErrorInfo>
            <Message>";
                    TRXFileContents += TestResultMessage;
                    TRXFileContents += @"</Message>
            <StackTrace></StackTrace>
        </ErrorInfo>
    </Output>";
                    }
	            TRXFileContents += @"    </UnitTestResult>";
                testIDIndex += 1;
            }
            TRXFileContents += @"
    </Results>
</TestRun>";
            return TRXFileContents;
        }
    }
}
