
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Activities.Statements;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Security.Principal;
using System.Threading;
using ActivityUnitTests;
using Dev2.Activities;
using Dev2.Common.Interfaces;
using Dev2.Data.Enums;
using Dev2.Interfaces;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace Dev2.Tests.Activities.ActivityTests
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    [ExcludeFromCodeCoverage]
    // ReSharper disable InconsistentNaming
    public class GatherSystemInformationTests : BaseActivityUnitTest
    {
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext { get; set; }


        [TestMethod]
        public void GatherSystemInformationWhereGetSystemInformationHelperNullExpectConcreateImplementation()
        {
            //------------Setup for test--------------------------
            var activity = GetGatherSystemInformationActivity();
            //------------Execute Test---------------------------
            var getSystemInformation = activity.GetSystemInformation;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(getSystemInformation, typeof(GetSystemInformationHelper));
        }

        [TestMethod]
        public void GatherSystemInformationWhereConstructedExpectIsICollectionActivity()
        {
            //------------Setup for test--------------------------

            //------------Execute Test---------------------------
            var activity = GetGatherSystemInformationActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(activity, typeof(ICollectionActivity));
        }

        [TestMethod]
        public void GatherSystemInformationWhereGivenAnIGetSystemInformationExpectGetGivenValue()
        {
            //------------Setup for test--------------------------
            var activity = GetGatherSystemInformationActivity();
            var getSystemInformation = new Mock<IGetSystemInformation>().Object;
            activity.GetSystemInformation = getSystemInformation;
            //------------Execute Test---------------------------
            var systemInfo = activity.GetSystemInformation;
            //------------Assert Results-------------------------
            Assert.AreEqual(getSystemInformation, systemInfo);
            Assert.IsNotInstanceOfType(systemInfo, typeof(GetSystemInformationHelper));
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetOperatingSystemInformationExpectOSDetails()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "my awesome OS";
            mock.Setup(information => information.GetOperatingSystemInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var operatingSystemInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.OperatingSystem);

            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue, operatingSystemInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetServicePackInformationExpectOSDetails()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Service Pack greatness";
            mock.Setup(information => information.GetServicePackInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var operatingSystemInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.ServicePack);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue, operatingSystemInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetOSBitValueInformationExpectOSDetails()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "128";
            mock.Setup(information => information.GetOSBitValueInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var operatingSystemInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.OSBitValue);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, operatingSystemInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetFullDateTimeInformationExpectDateTimeInformation()
        {
            //------------Setup for test--------------------------
            var activity = DsfGatherSystemInformationActivity(null);
            //------------Execute Test---------------------------
            var dateTimeInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.FullDateTime);
            //------------Assert Results-------------------------
            DateTime result = DateTime.Parse(dateTimeInformation);
            if(result.Millisecond == 0)
            {
                Thread.Sleep(10);
                dateTimeInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.FullDateTime);
                result = DateTime.Parse(dateTimeInformation);
                Assert.IsTrue(result.Millisecond > 0);
            }
            Assert.IsTrue(result.Millisecond > 0);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetDateTimeFormatInformationExpectDateTimeInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "date and time format";
            mock.Setup(information => information.GetDateTimeFormatInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var dateTimeInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.DateTimeFormat);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, dateTimeInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetDiskSpaceAvailableInformationExpectDiskInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "C: Drive 10 GB";
            mock.Setup(information => information.GetDiskSpaceAvailableInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var diskInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.DiskAvailable);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, diskInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetDiskSpaceTotalInformationInformationExpectDiskInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "C: Drive 100 GB";
            mock.Setup(information => information.GetDiskSpaceTotalInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var diskInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.DiskTotal);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, diskInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetMemoryAvailableInformationExpectMemoryInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Ram: 2GB";
            mock.Setup(information => information.GetPhysicalMemoryAvailableInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var memoryInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.PhysicalMemoryAvailable);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, memoryInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetMemoryTotalInformationExpectMemoryInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "8GB";
            mock.Setup(information => information.GetPhysicalMemoryTotalInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var memoryInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.PhysicalMemoryTotal);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, memoryInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetCPUAvailableInformationExpectProcessorInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var processorInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.CPUAvailable);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, processorInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetCPUTotalInformationExpectProcessorInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "8*1500";
            mock.Setup(information => information.GetCPUTotalInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var processorInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.CPUTotal);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, processorInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetLanguageInformationExpectLanguageInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "English";
            mock.Setup(information => information.GetLanguageInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var languageInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.Language);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, languageInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetRegionInformationExpectRegionInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "South Africa";
            mock.Setup(information => information.GetRegionInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var languageInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.Region);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, languageInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetUserRolesInformationExpectUserRolesInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Admin,Dev";
            mock.Setup(information => information.GetUserRolesInformation(It.IsAny<IIdentity>())).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var userRolesInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.UserRoles);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, userRolesInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetUserNameInformationExpectUserNameInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "IAMUSER";
            mock.Setup(information => information.GetUserNameInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var userNameInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.UserName);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, userNameInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetDomainInformationExpectDomainInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "DEV2";
            mock.Setup(information => information.GetDomainInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var userNameInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.Domain);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, userNameInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetNumberOfWareWolfAgentsInformationExpectNumberOfWareWolfAgentsInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "2";
            mock.Setup(information => information.GetNumberOfWareWolfAgentsInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var numWareWolfAgents = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.NumberOfWarewolfAgents);
            //------------Assert Results-------------------------
            Assert.AreEqual(expectedValue, numWareWolfAgents);
        }

        [TestMethod]
        public void GetFindMissingTypeExpectDataGridActivityType()
        {
            //------------Setup for test--------------------------
            var activity = new DsfGatherSystemInformationActivity();
            //------------Execute Test---------------------------
            var findMissingType = activity.GetFindMissingType();
            //------------Assert Results-------------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity, findMissingType);
        }

        [TestMethod]
        public void GatherSystemInformationWhereExecuteExpectCorrectResultsWithScalar()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.OperatingSystem, "[[testVar]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "my awesome OS";
            mock.Setup(information => information.GetOperatingSystemInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            TestData = "<root><testVar /></root>";
            string actual;
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();

            //------------Assert Results-------------------------
            Assert.IsFalse(Compiler.HasErrors(result.DataListID));
            GetScalarValueFromDataList(result.DataListID, "testVar", out actual, out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);

            Assert.AreEqual(ExpectedValue, actual);
        }

        [TestMethod]
        public void GatherSystemInformationWithBlankNotationWhereExecuteExpectCorrectResultsWithRecordsetAppend()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.UserName, "[[recset1().field1]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "IAMUSER";
            var expected = new List<string> { "Some Other Value", expectedValue };
            mock.Setup(information => information.GetUserNameInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root><recset1><field1>Some Other Value</field1></recset1></root>";
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("sfGatherSystemInformation_ExecuteProcess")]
        public void DsfGatherSystemInformation_ExecuteProcess_ACoupleOfTimes_InitiailizesDebugProperties()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.UserName, "[[a]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            mock.Setup(information => information.GetUserNameInformation()).Returns("IAMUSER");
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            CurrentDl = "<ADL><a/></ADL>";
            TestData = "<root><a>Some Other Value</a></root>";
            //------------Execute Test---------------------------
            var result = ExecuteProcess(isDebug: true);
            ExecuteProcess(isDebug: true);
            ExecuteProcess(isDebug: true);
            //------------Assert Results-------------------------
            // remove test datalist ;)
            DataListRemoval(result.DataListID);
            var debugOutputs = activity.GetDebugOutputs(null);
            var debugInputs = activity.GetDebugInputs(null);

            Assert.AreEqual(1, debugInputs.Count);
            Assert.AreEqual(1, debugOutputs.Count);
        }

        [TestMethod]
        public void GatherSystemInformationWithStarNotationWhereExecuteExpectCorrectResultsWithRecordsetOverwrite()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.DiskAvailable, "[[recset1(*).field1]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "C: Drive";
            mock.Setup(information => information.GetDiskSpaceAvailableInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root><recset1><field1>Some Other Value</field1></recset1></root>";
            var expected = new List<string> { expectedValue };
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);
            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void GatherSystemInformationWithSpecificIndexNotationWhereExecuteExpectCorrectResultsWithInsertIntoRecordset()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[recset1(2).field1]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(expectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root><recset1><field1>Some Other Value</field1></recset1><recset1><field1>Some Other Value 2</field1></recset1><recset1><field1>Some Other Value 2</field1></recset1></root>";
            var expected = new List<string> { "Some Other Value", expectedValue, "Some Other Value 2" };
            string error;
            //------------Execute Test---------------------------
            var result = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(result.DataListID, "recset1", "field1", out error);

            // remove test datalist ;)
            DataListRemoval(result.DataListID);


            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new ActivityUnitTests.Utils.StringComparer());
        }

        [TestMethod]
        public void GetCollectionCountWhereSystemInformationCollectionHasTwoItemsExpectTwo()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>
            {
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUTotal, "[[testVar]]", 1),
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.Language, "[[testLanguage]]", 2)
            };
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUTotalInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            //------------Execute Test---------------------------
            var collectionCount = activity.GetCollectionCount();
            //------------Assert Results-------------------------
            Assert.AreEqual(2, collectionCount);
        }

        [TestMethod]
        public void AddListToCollectionWhereNotOverwriteExpectInsertToCollection()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>
            {
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUTotal, "[[testVar]]", 1),
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.Language, "[[testLanguage]]", 2)
            };
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            var modelItem = TestModelItemUtil.CreateModelItem(activity);
            //------------Execute Test---------------------------
            activity.AddListToCollection(new[] { "[[Var1]]" }, false, modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(4, activity.SystemInformationCollection.Count);
        }

        [TestMethod]
        public void AddListToCollectionWhereNotOverwriteEmptyListExpectAddToCollection()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>();
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            var modelItem = TestModelItemUtil.CreateModelItem(activity);
            //------------Execute Test---------------------------
            activity.AddListToCollection(new[] { "[[Var1]]" }, false, modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, activity.SystemInformationCollection.Count);
        }

        [TestMethod]
        public void AddListToCollectionWhereOverwriteExpectAddToCollection()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>
            {
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUTotal, "[[testVar]]", 1),
                new GatherSystemInformationTO(enTypeOfSystemInformationToGather.Language, "[[testLanguage]]", 2)
            };
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            var modelItem = TestModelItemUtil.CreateModelItem(activity);
            //------------Execute Test---------------------------
            activity.AddListToCollection(new[] { "[[Var1]]" }, true, modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2, activity.SystemInformationCollection.Count);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfGatherSystemInformationActivity_UpdateForEachInputs")]
        public void DsfGatherSystemInformationActivity_UpdateForEachInputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[testVar]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(expectedValue);
            var act = DsfGatherSystemInformationActivity(mock);
            act.SystemInformationCollection = systemInformationCollection;

            //------------Execute Test---------------------------
            act.UpdateForEachInputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[testVar]]", act.SystemInformationCollection[0].Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfGatherSystemInformationActivity_UpdateForEachInputs")]
        public void DsfGatherSystemInformationActivity_UpdateForEachInputs_MoreThan1Updates_Collection()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[testVar]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(expectedValue);
            var act = DsfGatherSystemInformationActivity(mock);
            act.SystemInformationCollection = systemInformationCollection;

            var tuple1 = new Tuple<string, string>("[[testVar]]", "Test");
            var tuple2 = new Tuple<string, string>("[[Customers(*).DOB]]", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachInputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.SystemInformationCollection[0].Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfGatherSystemInformationActivity_UpdateForEachInputs")]
        public void DsfGatherSystemInformationActivity_UpdateForEachOutputs_MoreThan1Updates_Collection()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[testVar]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(expectedValue);
            var act = DsfGatherSystemInformationActivity(mock);
            act.SystemInformationCollection = systemInformationCollection;

            var tuple1 = new Tuple<string, string>("[[testVar]]", "Test");
            var tuple2 = new Tuple<string, string>("[[Customers(*).DOB]]", "Test2");
            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(new List<Tuple<string, string>> { tuple1, tuple2 }, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("Test", act.SystemInformationCollection[0].Result);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfGatherSystemInformationActivity_UpdateForEachOutputs")]
        public void DsfGatherSystemInformationActivity_UpdateForEachOutputs_NullUpdates_DoesNothing()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[testVar]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(expectedValue);
            var act = DsfGatherSystemInformationActivity(mock);
            act.SystemInformationCollection = systemInformationCollection;

            //------------Execute Test---------------------------
            act.UpdateForEachOutputs(null, null);
            //------------Assert Results-------------------------
            Assert.AreEqual("[[testVar]]", act.SystemInformationCollection[0].Result);
        }


        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfGatherSystemInformationActivity_GetForEachInputs")]
        public void DsfGatherSystemInformationActivity_GetForEachInputs_WhenHasExpression_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[testVar]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(expectedValue);
            var act = DsfGatherSystemInformationActivity(mock);
            act.SystemInformationCollection = systemInformationCollection;

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachInputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[testVar]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[testVar]]", dsfForEachItems[0].Value);
        }

        [TestMethod]
        [Owner("Hagashen Naidu")]
        [TestCategory("DsfGatherSystemInformationActivity_GetForEachOutputs")]
        public void DsfGatherSystemInformationActivity_GetForEachOutputs_WhenHasResult_ReturnsInputList()
        {
            //------------Setup for test--------------------------
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO> { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[testVar]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string expectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(expectedValue);
            var act = DsfGatherSystemInformationActivity(mock);
            act.SystemInformationCollection = systemInformationCollection;

            //------------Execute Test---------------------------
            var dsfForEachItems = act.GetForEachOutputs();
            //------------Assert Results-------------------------
            Assert.AreEqual(1, dsfForEachItems.Count);
            Assert.AreEqual("[[testVar]]", dsfForEachItems[0].Name);
            Assert.AreEqual("[[testVar]]", dsfForEachItems[0].Value);
        }

        static DsfGatherSystemInformationActivity DsfGatherSystemInformationActivity(Mock<IGetSystemInformation> mock)
        {
            DsfGatherSystemInformationActivity activity;

            if(mock != null)
            {
                var getSystemInformation = mock.Object;
                activity = GetGatherSystemInformationActivity();
                activity.GetSystemInformation = getSystemInformation;
            }
            else
            {
                activity = GetGatherSystemInformationActivity();
            }

            return activity;
        }

        static DsfGatherSystemInformationActivity GetGatherSystemInformationActivity()
        {
            var activity = new DsfGatherSystemInformationActivity();
            return activity;
        }

    }
}
