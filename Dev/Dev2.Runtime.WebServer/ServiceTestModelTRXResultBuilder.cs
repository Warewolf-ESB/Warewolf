#pragma warning disable CC0091, S1226, S100, CC0044, CC0045, CC0021, S1449, S1541, S1067, S3235, CC0015, S107, S2292, S1450, S105, CC0074, S1135, S101, S3776, CS0168, S2339, CC0031, S3240, CC0020, CS0108, S1694, S1481, CC0008, AD0001, S2328, S2696, S1643, CS0659, CS0067, S104, CC0030, CA2202, S3376, S1185, CS0219, S3253, S1066, CC0075, S3459, S1871, S1125, CS0649, S2737, S1858, CC0082, CC0001, S3241, S2223, S1301, CC0013, S2955, S1944, CS4014, S3052, S2674, S2344, S1939, S1210, CC0033, CC0002, S3458, S3254, S3220, S2197, S1905, S1699, S1659, S1155, CS0105, CC0019, S3626, S3604, S3440, S3256, S2692, S2345, S1109, FS0058, CS1998, CS0661, CS0660, CS0162, CC0089, CC0032, CC0011, CA1001
﻿/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.Common.Interfaces;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Xml;

namespace Dev2.Runtime.WebServer
{
    static class ServiceTestModelTRXResultBuilder
    {
        public static string BuildTestResultTRX(string ServiceName, List<IServiceTestModelTO> TestResults)
        {
            var testIDs = new List<Guid>();
            var executionIDs = new List<Guid>();
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
            var WarewolfServerHostname = Environment.MachineName;
            if (string.IsNullOrWhiteSpace(ServiceName) || ServiceName == "*")
            {
                ServiceName = WarewolfServerHostname;
            }
            var WarewolfServerUsername = "User";
            var TotalTests = TestResults.FindAll((result) => { return result != null; }).Count;
            var TotalPassed = TestResults.FindAll((result) => { return result != null && result.TestPassed; }).Count;
            var TotalFailed = TestResults.FindAll((result) => { return result != null && result.TestFailing; }).Count + TestResults.FindAll((result) => { return result != null && result.TestInvalid; }).Count;

            var TRXFile = new XmlDocument();
            TRXFile.AppendChild(TRXFile.CreateXmlDeclaration("1.0", "utf-8", null));
            var testRunElement = TRXFile.CreateElement("TestRun");
            testRunElement.SetAttribute("id", Guid.NewGuid().ToString());
            testRunElement.SetAttribute("name", "Warewolf Service Tests");
            testRunElement.SetAttribute("runUser", WarewolfServerUsername);
            testRunElement.SetAttribute("xmlns", "http://microsoft.com/schemas/VisualStudio/TeamTest/2010");
            var timesElement = TRXFile.CreateElement("Times");
            timesElement.SetAttribute("creation", TestStartDateTime);
            timesElement.SetAttribute("queuing", TestStartDateTime);
            timesElement.SetAttribute("start", TestStartDateTime);
            timesElement.SetAttribute("finish", TestEndDateTime);
            testRunElement.AppendChild(timesElement);
            var resultsSummaryElement = TRXFile.CreateElement("ResultSummary");
            resultsSummaryElement.SetAttribute("outcome", "Completed");
            var countersElement = TRXFile.CreateElement("Counters");
            countersElement.SetAttribute("total", TotalTests.ToString());
            countersElement.SetAttribute("executed", TotalTests.ToString());
            countersElement.SetAttribute("passed", TotalPassed.ToString());
            countersElement.SetAttribute("error", "0");
            countersElement.SetAttribute("failed", TotalFailed.ToString());
            countersElement.SetAttribute("timeout", "0");
            countersElement.SetAttribute("aborted", "0");
            countersElement.SetAttribute("inconclusive", "0");
            countersElement.SetAttribute("passedButRunAborted", "0");
            countersElement.SetAttribute("notRunnable", "0");
            countersElement.SetAttribute("notExecuted", "0");
            countersElement.SetAttribute("disconnected", "0");
            countersElement.SetAttribute("warning", "0");
            countersElement.SetAttribute("completed", "0");
            countersElement.SetAttribute("inProgress", "0");
            countersElement.SetAttribute("pending", "0");
            resultsSummaryElement.AppendChild(countersElement);
            testRunElement.AppendChild(resultsSummaryElement);
            var testDefinitionsElement = TRXFile.CreateElement("TestDefinitions");
            var testIDIndex = 0;
            foreach (var TestResult in TestResults.FindAll((result) => { return result != null; }))
            {
                var unitTestDefinitionElement = TRXFile.CreateElement("UnitTest");
                unitTestDefinitionElement.SetAttribute("name", TestResult.TestName.Replace(" ", "_"));
                unitTestDefinitionElement.SetAttribute("storage", ServiceName.Replace(" ", "_"));
                unitTestDefinitionElement.SetAttribute("id", testIDs[testIDIndex].ToString());
                var executionElement = TRXFile.CreateElement("Execution");
                executionElement.SetAttribute("id", executionIDs[testIDIndex].ToString());
                unitTestDefinitionElement.AppendChild(executionElement);
                var testMethodElement = TRXFile.CreateElement("TestMethod");
                testMethodElement.SetAttribute("codeBase", ServiceName.Replace(" ", "_") + ".dll");
                testMethodElement.SetAttribute("adapterTypeName", "Microsoft.VisualStudio.TestTools.TestTypes.Unit.UnitTestAdapter, Microsoft.VisualStudio.QualityTools.Tips.UnitTest.Adapter, Version=14.0.0.0, Culture = neutral, PublicKeyToken=b03f5f7f11d50a3a");
                testMethodElement.SetAttribute("className", ServiceName.Replace(" ", "_") + ", Version = " + WarewolfServerVersion + ", Culture = neutral, PublicKeyToken = null");
                testMethodElement.SetAttribute("name", TestResult.TestName.Replace(" ", "_"));
                unitTestDefinitionElement.AppendChild(testMethodElement);
                testDefinitionsElement.AppendChild(unitTestDefinitionElement);
                testIDIndex++;
            }
            testRunElement.AppendChild(testDefinitionsElement);
            var testListsElement = TRXFile.CreateElement("TestLists");
            var testListNoListElement = TRXFile.CreateElement("TestList");
            testListNoListElement.SetAttribute("name", "Results Not in a List");
            testListNoListElement.SetAttribute("id", TestListID);
            testListsElement.AppendChild(testListNoListElement);
            var testListDefaultListElement = TRXFile.CreateElement("TestList");
            testListDefaultListElement.SetAttribute("name", "All Loaded Results");
            testListDefaultListElement.SetAttribute("id", "19431567-8539-422a-85d7-44ee4e166bda");
            testListsElement.AppendChild(testListDefaultListElement);
            testRunElement.AppendChild(testListsElement);
            var testEntriesElement = TRXFile.CreateElement("TestEntries");
            testIDIndex = 0;
            foreach (var TestResult in TestResults.FindAll((result) => { return result != null; }))
            {
                var testEntryElement = TRXFile.CreateElement("TestEntry");
                testEntryElement.SetAttribute("testId", testIDs[testIDIndex].ToString());
                testEntryElement.SetAttribute("executionId", executionIDs[testIDIndex].ToString());
                testEntryElement.SetAttribute("testListId", TestListID);
                testEntriesElement.AppendChild(testEntryElement);
                testIDIndex++;
            }
            testRunElement.AppendChild(testEntriesElement);
            var resultsElement = TRXFile.CreateElement("Results");
            testIDIndex = 0;
            foreach (var TestResult in TestResults.FindAll((result) => { return result != null; }))
            {
                var unitTestResultElement = TRXFile.CreateElement("UnitTestResult");
                unitTestResultElement.SetAttribute("executionId", executionIDs[testIDIndex].ToString());
                unitTestResultElement.SetAttribute("testId", testIDs[testIDIndex].ToString());
                unitTestResultElement.SetAttribute("testName", TestResult.TestName.Replace(" ", "_"));
                unitTestResultElement.SetAttribute("computerName", WarewolfServerHostname);
                unitTestResultElement.SetAttribute("duration", TestDuration);
                unitTestResultElement.SetAttribute("startTime", TestStartDateTime);
                unitTestResultElement.SetAttribute("endTime", DateTime.Now.ToString("o", CultureInfo.InvariantCulture));
                unitTestResultElement.SetAttribute("testType", "13cdc9d9-ddb5-4fa4-a97d-d965ccfc6d4b");
                unitTestResultElement.SetAttribute("outcome", TestResult.Result.RunTestResult.ToString().Replace("Test", ""));
                unitTestResultElement.SetAttribute("testListId", TestListID);
                unitTestResultElement.SetAttribute("relativeResultsDirectory", "ca6d373f-8816-4969-8999-3dac700d7626");
                if (TestResult.TestFailing)
                {
                    var outputElement = TRXFile.CreateElement("Output");
                    var errorInfoElement = TRXFile.CreateElement("ErrorInfo");
                    var messageElement = TRXFile.CreateElement("Message");
                    messageElement.InnerText = System.Web.HttpUtility.HtmlEncode(TestResult.FailureMessage);
                    errorInfoElement.AppendChild(messageElement);
                    var stackTraceElement = TRXFile.CreateElement("StackTrace");
                    errorInfoElement.AppendChild(stackTraceElement);
                    outputElement.AppendChild(errorInfoElement);
                    unitTestResultElement.AppendChild(outputElement);
                }
                resultsElement.AppendChild(unitTestResultElement);
                testIDIndex++;
            }
            testRunElement.AppendChild(resultsElement);
            TRXFile.AppendChild(testRunElement);            
            return TRXFile.InnerXml;
        }
    }
}
