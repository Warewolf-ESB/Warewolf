using System.Activities.Statements;
using System.Collections.Generic;
using System.Linq;
using Dev2;
using Dev2.Activities;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Data.Enums;
using Dev2.Diagnostics;
using Dev2.Enums;
using Dev2.Interfaces;
using Dev2.Tests;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;

namespace ActivityUnitTests.ActivityTest
{
    /// <summary>
    /// Summary description for CountRecordsTest
    /// </summary>
    [TestClass]
    //[Ignore] //Does not work on server
    public class GatherSystemInformationTests : BaseActivityUnitTest
    {
        private TestContext _testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return _testContextInstance;
            }
            set
            {
                _testContextInstance = value;
            }
        }

      
        [TestMethod]
        public void GatherSystemInformationWhereGetSystemInformationHelperNullExpectConcreateImplementation()
        {
            //------------Setup for test--------------------------
            var activity = GetGatherSystemInformationActivity();
            //------------Execute Test---------------------------
            var getSystemInformation = activity.GetSystemInformation;
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(getSystemInformation,typeof(GetSystemInformationHelper));
        }

        [TestMethod]
        public void GatherSystemInformationWhereConstructedExpectIsICollectionActivity()
        {
            //------------Setup for test--------------------------
            
            //------------Execute Test---------------------------
            var activity = GetGatherSystemInformationActivity();
            //------------Assert Results-------------------------
            Assert.IsInstanceOfType(activity,typeof(ICollectionActivity));
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
            Assert.AreEqual(getSystemInformation,systemInfo);
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
            Assert.AreEqual(ExpectedValue,operatingSystemInformation);
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
            Assert.AreEqual(ExpectedValue,operatingSystemInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetOSBitValueInformationExpectOSDetails()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "128";
            mock.Setup(information => information.GetOSBitValueInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var operatingSystemInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.OSBitValue);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,operatingSystemInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetFullDateTimeInformationExpectDateTimeInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "the date and time";
            mock.Setup(information => information.GetFullDateTimeInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var dateTimeInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.FullDateTime);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,dateTimeInformation);
        } 
        
        [TestMethod]
        public void GatherSystemInformationWhereGetDateTimeFormatInformationExpectDateTimeInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "date and time format";
            mock.Setup(information => information.GetDateTimeFormatInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var dateTimeInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.DateTimeFormat);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,dateTimeInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetDiskSpaceAvailableInformationExpectDiskInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "C: Drive 10 GB";
            mock.Setup(information => information.GetDiskSpaceAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var diskInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.DiskAvailable);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,diskInformation);
        } 
        
        [TestMethod]
        public void GatherSystemInformationWhereGetDiskSpaceTotalInformationInformationExpectDiskInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "C: Drive 100 GB";
            mock.Setup(information => information.GetDiskSpaceTotalInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var diskInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.DiskTotal);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,diskInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetMemoryAvailableInformationExpectMemoryInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Ram: 2GB";
            mock.Setup(information => information.GetPhysicalMemoryAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var memoryInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.PhysicalMemoryAvailable);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,memoryInformation);
        }  
        
        [TestMethod]
        public void GatherSystemInformationWhereGetMemoryTotalInformationExpectMemoryInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "8GB";
            mock.Setup(information => information.GetPhysicalMemoryTotalInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var memoryInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.PhysicalMemoryTotal);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,memoryInformation);
        } 
        
        [TestMethod]
        public void GatherSystemInformationWhereGetCPUAvailableInformationExpectProcessorInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var processorInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.CPUAvailable);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,processorInformation);
        } 
        
        [TestMethod]
        public void GatherSystemInformationWhereGetCPUTotalInformationExpectProcessorInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "8*1500";
            mock.Setup(information => information.GetCPUTotalInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var processorInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.CPUTotal);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,processorInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetLanguageInformationExpectLanguageInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "English";
            mock.Setup(information => information.GetLanguageInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var languageInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.Language);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,languageInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetRegionInformationExpectRegionInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "South Africa";
            mock.Setup(information => information.GetRegionInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var languageInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.Region);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,languageInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetUserRolesInformationExpectUserRolesInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Admin,Dev";
            mock.Setup(information => information.GetUserRolesInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var userRolesInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.UserRoles);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,userRolesInformation);
        }

        [TestMethod]
        public void GatherSystemInformationWhereGetUserNameInformationExpectUserNameInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "IAMUSER";
            mock.Setup(information => information.GetUserNameInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var userNameInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.UserName);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,userNameInformation);
        } 
        
        [TestMethod]
        public void GatherSystemInformationWhereGetDomainInformationExpectDomainInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "DEV2";
            mock.Setup(information => information.GetDomainInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var userNameInformation = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.Domain);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,userNameInformation);
        } 
        
        [TestMethod]
        public void GatherSystemInformationWhereGetNumberOfWareWolfAgentsInformationExpectNumberOfWareWolfAgentsInformation()
        {
            //------------Setup for test--------------------------
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "2";
            mock.Setup(information => information.GetNumberOfWareWolfAgentsInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            //------------Execute Test---------------------------
            var numWareWolfAgents = activity.GetCorrectSystemInformation(enTypeOfSystemInformationToGather.NumberOfWarewolfAgents);
            //------------Assert Results-------------------------
            Assert.AreEqual(ExpectedValue,numWareWolfAgents);
        }       
        
        [TestMethod]
        public void GetFindMissingTypeExpectDataGridActivityType()
        {
            //------------Setup for test--------------------------
            var activity = new DsfGatherSystemInformationActivity();
            //------------Execute Test---------------------------
            var findMissingType = activity.GetFindMissingType();
            //------------Assert Results-------------------------
            Assert.AreEqual(enFindMissingType.DataGridActivity,findMissingType);
        }

        [TestMethod]
        public void GatherSystemInformationWhereExecuteExpectCorrectResultsWithScalar()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>() { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.OperatingSystem, "[[testVar]]", 1) };
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
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            Assert.IsFalse(Compiler.HasErrors(executeProcess.DataListID));
            GetScalarValueFromDataList(executeProcess.DataListID, "testVar", out actual, out error);
            Assert.AreEqual(ExpectedValue,actual);
        }

        [TestMethod]
        public void GatherSystemInformationWithBlankNotationWhereExecuteExpectCorrectResultsWithRecordsetAppend()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>() { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.UserName, "[[recset1().field1]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "IAMUSER";
            var expected = new List<string> { "Some Other Value",ExpectedValue };
            mock.Setup(information => information.GetUserNameInformation()).Returns(ExpectedValue);
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
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(executeProcess.DataListID, "recset1", "field1", out error);
            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
        }
        
        [TestMethod]
        public void GatherSystemInformationWithStarNotationWhereExecuteExpectCorrectResultsWithRecordsetOverwrite()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>() { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.DiskAvailable, "[[recset1(*).field1]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "C: Drive";
            mock.Setup(information => information.GetDiskSpaceAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root><recset1><field1>Some Other Value</field1></recset1></root>";
            var expected = new List<string> { ExpectedValue };
            string error;
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(executeProcess.DataListID, "recset1", "field1", out error);
            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
        }

        [TestMethod]
        public void GatherSystemInformationWithSpecificIndexNotationWhereExecuteExpectCorrectResultsWithInsertIntoRecordset()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>() { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[recset1(2).field1]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;
            TestStartNode = new FlowStep
            {
                Action = activity
            };
            CurrentDl = "<ADL><recset1><field1/></recset1></ADL>";
            TestData = "<root><recset1><field1>Some Other Value</field1></recset1><recset1><field1>Some Other Value 2</field1></recset1><recset1><field1>Some Other Value 2</field1></recset1></root>";
            var expected = new List<string> { "Some Other Value", ExpectedValue, "Some Other Value 2" };
            string error;
            //------------Execute Test---------------------------
            var executeProcess = ExecuteProcess();
            //------------Assert Results-------------------------
            List<string> actual = RetrieveAllRecordSetFieldValues(executeProcess.DataListID, "recset1", "field1", out error);
            var actualArray = actual.ToArray();
            actual.Clear();
            actual.AddRange(actualArray.Select(s => s.Trim()));
            CollectionAssert.AreEqual(expected, actual, new Utils.StringComparer());
        }



        [TestMethod]
        public void GatherSystemInformationGetDebugInputOutputExpectedCorrectResults()
        {
            IList<GatherSystemInformationTO> systemInformationCollection = new List<GatherSystemInformationTO>() { new GatherSystemInformationTO(enTypeOfSystemInformationToGather.CPUAvailable, "[[testVar]]", 1) };
            var mock = new Mock<IGetSystemInformation>();
            const string ExpectedValue = "Intel i7";
            mock.Setup(information => information.GetCPUAvailableInformation()).Returns(ExpectedValue);
            var activity = DsfGatherSystemInformationActivity(mock);
            activity.SystemInformationCollection = systemInformationCollection;

            List<DebugItem> inRes;
            List<DebugItem> outRes;

            CheckActivityDebugInputOutput(activity, "<root><testVar /></root>",
                                                                "<root><testVar /></root>", out inRes, out outRes);

            Assert.AreEqual(1, outRes.Count);
            var fetchResultsList = outRes[0].FetchResultsList();
            Assert.AreEqual(6, fetchResultsList.Count);

            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[0].Type);
            Assert.AreEqual("1", fetchResultsList[0].Value);
            
            Assert.AreEqual(DebugItemResultType.Variable, fetchResultsList[1].Type);
            Assert.AreEqual("[[testVar]]", fetchResultsList[1].Value);

            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[2].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, fetchResultsList[2].Value);

            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[3].Type);
            Assert.AreEqual(enTypeOfSystemInformationToGather.CPUAvailable.GetDescription(), fetchResultsList[3].Value);

            Assert.AreEqual(DebugItemResultType.Label, fetchResultsList[4].Type);
            Assert.AreEqual(GlobalConstants.EqualsExpression, fetchResultsList[4].Value);

            Assert.AreEqual(DebugItemResultType.Value, fetchResultsList[5].Type);
            Assert.AreEqual(ExpectedValue, fetchResultsList[5].Value);
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
            Assert.AreEqual(2,collectionCount);
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
            activity.AddListToCollection(new[]{"[[Var1]]"}, false,modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(4,activity.SystemInformationCollection.Count);
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
            activity.AddListToCollection(new[]{"[[Var1]]"}, false,modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2,activity.SystemInformationCollection.Count);
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
            activity.AddListToCollection(new[]{"[[Var1]]"}, true,modelItem);
            //------------Assert Results-------------------------
            Assert.AreEqual(2,activity.SystemInformationCollection.Count);
        }

        static DsfGatherSystemInformationActivity DsfGatherSystemInformationActivity(Mock<IGetSystemInformation> mock)
        {
            var getSystemInformation = mock.Object;
            var activity = GetGatherSystemInformationActivity();
            activity.GetSystemInformation = getSystemInformation;
            return activity;
        }

        static DsfGatherSystemInformationActivity GetGatherSystemInformationActivity()
        {
            var activity = new DsfGatherSystemInformationActivity();
            return activity;
        }

    }
}