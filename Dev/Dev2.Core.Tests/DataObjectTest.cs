/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Dev2.Common.Interfaces.Diagnostics.Debug;
using Dev2.Data;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Interfaces;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Warewolf.Storage;

// ReSharper disable FunctionComplexityOverflow

// ReSharper disable InconsistentNaming
// ReSharper disable MethodTooLong
namespace Dev2.Tests
{
    [TestClass]
    public class DataObjectTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataObject_IsRemoteWorkflow")]
        public void DsfDataOBject_IsRemoteWorkflow_WhenOverrideNotSet_ExpectTrue()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.EnvironmentID = Guid.NewGuid();

            //------------Execute Test---------------------------
            var result = dataObject.IsRemoteWorkflow();

            //------------Assert Results-------------------------
            Assert.IsTrue(result);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PopEnvironment_GivenHasNoEnvironments_ShouldNotSetEnvironment()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IExecutionEnvironment>();
            mock.SetupAllProperties();
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.Environment = mock.Object;
            PrivateObject privateObject = new PrivateObject(dataObject);
            var field = privateObject.GetField("_environments", BindingFlags.Instance | BindingFlags.NonPublic) as ConcurrentStack<IExecutionEnvironment>;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dataObject.Environment);
            Assert.IsNotNull(field);
            Assert.AreEqual(0,field.Count);
            //---------------Execute Test ----------------------
            dataObject.PopEnvironment();
            //---------------Test Result -----------------------
            Assert.AreEqual(dataObject.Environment, mock.Object);
        }

        [TestMethod]
        [Owner("Nkosinathi Sangweni")]
        public void PopEnvironment_GivenNoEnvironments_ShouldSetEnvironment()
        {
            //---------------Set up test pack-------------------
            var mock = new Mock<IExecutionEnvironment>();
            mock.SetupAllProperties();
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.Environment = mock.Object;
            PrivateObject privateObject = new PrivateObject(dataObject);
            var field = privateObject.GetField("_environments", BindingFlags.Instance | BindingFlags.NonPublic) as ConcurrentStack<IExecutionEnvironment>;
            //---------------Assert Precondition----------------
            Assert.IsNotNull(dataObject.Environment);
            Assert.IsNotNull(field);
            Assert.AreEqual(0,field.Count);
            //---------------Execute Test ----------------------
            dataObject.PushEnvironment(new ExecutionEnvironment());
            dataObject.PopEnvironment();
            //---------------Test Result -----------------------
            Assert.AreEqual(dataObject.Environment, mock.Object);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataObject_IsRemoteWorkflow")]
        public void DsfDataOBject_IsRemoteWorkflow_WhenOverrideSet_ExpectFalse()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.EnvironmentID = Guid.NewGuid();
            dataObject.IsRemoteInvokeOverridden = true;

            //------------Execute Test---------------------------
            var result = dataObject.IsRemoteWorkflow();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataObject_IsRemoteWorkflow")]
        public void DsfDataOBject_IsRemoteWorkflow_WhenOverrideSetAndEmptyGuid_ExpectFalse()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.EnvironmentID = Guid.Empty;
            dataObject.IsRemoteInvokeOverridden = true;

            //------------Execute Test---------------------------
            var result = dataObject.IsRemoteWorkflow();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataObject_IsRemoteWorkflow")]
        public void DsfDataOBject_IsRemoteWorkflow_WhenOverrideNotSetAndEmptyGuid_ExpectFalse()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.EnvironmentID = Guid.Empty;

            //------------Execute Test---------------------------
            var result = dataObject.IsRemoteWorkflow();

            //------------Assert Results-------------------------
            Assert.IsFalse(result);
        }


        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataObject_RawPayload")]
        public void DsfDataObject_RawPayload_WhenNull_ExpectEmptyString()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());

            //------------Execute Test---------------------------
            var result = dataObject.RawPayload;

            //------------Assert Results-------------------------
            Assert.AreEqual(string.Empty, result.ToString(), "RawPayload did not return and empty string");
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DsfDataObject_NestingLevel")]
        public void DsfDataObject_NestingLevel_Get_Set_ExpectCorrectGetSet()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid());
            dataObject.ForEachNestingLevel = 3;
            Assert.AreEqual(dataObject.ForEachNestingLevel, 3);

        }
        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataObjectz_RawPayload")]
        public void DsfDataObjectz_RawPayload_WhenNotNull_ExpectRawPayload()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "foo");

            //------------Execute Test---------------------------
            var result = dataObject.RawPayload.ToString();

            //------------Assert Results-------------------------
            StringAssert.Contains(result, "foo");
        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataObject_Clone")]
        public void DsfDataObject_Clone_NormalClone_FullDuplicationForProperties()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "<x>1</x>");
            Guid wfInstanceID = Guid.NewGuid();

            dataObject.BookmarkExecutionCallbackID = Guid.NewGuid();
            dataObject.CurrentBookmarkName = "def";
            dataObject.DataList = new StringBuilder("<x/>");
            dataObject.DataListID = Guid.NewGuid();
            dataObject.DatalistInMergeDepth = enTranslationDepth.Data;
            dataObject.DatalistInMergeID = Guid.NewGuid();
            dataObject.DatalistInMergeType = enDataListMergeTypes.Union;
            dataObject.DatalistOutMergeDepth = enTranslationDepth.Data;
            dataObject.DatalistOutMergeFrequency = DataListMergeFrequency.Always;
            dataObject.DatalistOutMergeID = Guid.NewGuid();
            dataObject.DatalistOutMergeType = enDataListMergeTypes.Union;
            dataObject.DebugSessionID = Guid.NewGuid();
            dataObject.EnvironmentID = Guid.NewGuid();
            dataObject.ExecutionCallbackID = Guid.NewGuid();
            dataObject.ExecutionOrigin = ExecutionOrigin.Debug;
            dataObject.ExecutionOriginDescription = "xxx";
            dataObject.ForceDeleteAtNextNativeActivityCleanup = true;
            dataObject.IsDataListScoped = false;
            dataObject.IsDebug = true;
            dataObject.IsFromWebServer = true;
            dataObject.IsOnDemandSimulation = true;
            dataObject.NumberOfSteps = 2;
            dataObject.OriginalInstanceID = Guid.NewGuid();
            dataObject.ParentInstanceID = "1211";
            dataObject.ParentServiceName = "xxx";
            dataObject.ParentThreadID = 2;
            dataObject.ParentWorkflowInstanceId = "1233";
            dataObject.RawPayload = new StringBuilder("<raw>a</raw>");
            dataObject.RemoteDebugItems = new List<IDebugState>();
            dataObject.RemoteInvoke = false;
            dataObject.RemoteInvokeResultShape = new StringBuilder("<x/>");
            dataObject.RemoteInvokerID = "999";
            dataObject.RemoteServiceType = "NA";
            dataObject.ResourceID = Guid.NewGuid();
            dataObject.ReturnType = EmitionTypes.XML;
            dataObject.ServerID = Guid.NewGuid();
            dataObject.ServiceName = "xxx";
            dataObject.WorkflowInstanceId = wfInstanceID;
            dataObject.WorkflowResumeable = false;
            dataObject.ParentID = Guid.NewGuid();
            dataObject.WorkspaceID = Guid.NewGuid();
            dataObject.ClientID = Guid.NewGuid();
            dataObject.RunWorkflowAsync = true;
            dataObject.IsDebugNested = true;
            dataObject.ForEachNestingLevel = 3;
            dataObject.StopExecution = false;
            dataObject.IsServiceTestExecution = true;
            dataObject.TestName = "Test 1";
            dataObject.SourceResourceID = Guid.NewGuid();
            dataObject.ServiceTest = new ServiceTestModelTO {TestName = "Test Mock"};
            var threadsToDispose = new Dictionary<int, List<Guid>>();
            List<Guid> guidList = new List<Guid> { Guid.NewGuid() };
            threadsToDispose.Add(3, guidList);
            dataObject.ThreadsToDispose = threadsToDispose;

            //------------Execute Test---------------------------
            IDSFDataObject clonedObject = dataObject.Clone();

            //------------Assert Results-------------------------

            // check counts, then check values
            var properties = typeof(IDSFDataObject).GetProperties();
            Assert.AreEqual(62, properties.Length);

            // now check each value to ensure it transfered
            Assert.AreEqual(dataObject.BookmarkExecutionCallbackID, clonedObject.BookmarkExecutionCallbackID);
            Assert.AreEqual(dataObject.CurrentBookmarkName, clonedObject.CurrentBookmarkName);
            Assert.AreEqual(dataObject.DataList, clonedObject.DataList);
            Assert.AreEqual(dataObject.DataListID, clonedObject.DataListID);
            Assert.AreEqual(dataObject.DatalistInMergeDepth, clonedObject.DatalistInMergeDepth);
            Assert.AreEqual(dataObject.DatalistInMergeID, clonedObject.DatalistInMergeID);
            Assert.AreEqual(dataObject.DatalistInMergeType, clonedObject.DatalistInMergeType);
            Assert.AreEqual(dataObject.DatalistOutMergeDepth, clonedObject.DatalistOutMergeDepth);
            Assert.AreEqual(dataObject.DatalistOutMergeFrequency, clonedObject.DatalistOutMergeFrequency);
            Assert.AreEqual(dataObject.DatalistOutMergeID, clonedObject.DatalistOutMergeID);
            Assert.AreEqual(dataObject.DatalistOutMergeType, clonedObject.DatalistOutMergeType);
            Assert.AreEqual(dataObject.DebugSessionID, clonedObject.DebugSessionID);
            Assert.AreEqual(dataObject.EnvironmentID, clonedObject.EnvironmentID);
            Assert.AreEqual(dataObject.ExecutingUser, clonedObject.ExecutingUser);
            Assert.AreEqual(dataObject.ExecutionCallbackID, clonedObject.ExecutionCallbackID);
            Assert.AreEqual(dataObject.ExecutionOrigin, clonedObject.ExecutionOrigin);
            Assert.AreEqual(dataObject.ExecutionOriginDescription, clonedObject.ExecutionOriginDescription);
            Assert.AreEqual(dataObject.ForceDeleteAtNextNativeActivityCleanup, clonedObject.ForceDeleteAtNextNativeActivityCleanup);
            Assert.AreEqual(dataObject.IsDataListScoped, clonedObject.IsDataListScoped);
            Assert.AreEqual(dataObject.IsDebug, clonedObject.IsDebug);
            Assert.AreEqual(dataObject.IsFromWebServer, clonedObject.IsFromWebServer);
            Assert.AreEqual(dataObject.IsOnDemandSimulation, clonedObject.IsOnDemandSimulation);
            Assert.AreEqual(dataObject.IsRemoteInvoke, clonedObject.IsRemoteInvoke);
            Assert.AreEqual(dataObject.IsRemoteInvokeOverridden, clonedObject.IsRemoteInvokeOverridden);
            Assert.AreEqual(dataObject.NumberOfSteps, clonedObject.NumberOfSteps);
            Assert.AreEqual(dataObject.OriginalInstanceID, clonedObject.OriginalInstanceID);
            Assert.AreEqual(dataObject.ParentInstanceID, clonedObject.ParentInstanceID);
            Assert.AreEqual(dataObject.ParentServiceName, clonedObject.ParentServiceName);
            Assert.AreEqual(dataObject.ParentThreadID, clonedObject.ParentThreadID);
            Assert.AreEqual(dataObject.ParentWorkflowInstanceId, clonedObject.ParentWorkflowInstanceId);
            Assert.AreEqual(dataObject.RawPayload, clonedObject.RawPayload);
            Assert.AreEqual(dataObject.RemoteDebugItems, clonedObject.RemoteDebugItems);
            Assert.AreEqual(dataObject.RemoteInvoke, clonedObject.RemoteInvoke);
            Assert.AreEqual(dataObject.RemoteNonDebugInvoke, clonedObject.RemoteNonDebugInvoke);
            Assert.AreEqual(dataObject.RemoteInvokeResultShape, clonedObject.RemoteInvokeResultShape);
            Assert.AreEqual(dataObject.RemoteInvokerID, clonedObject.RemoteInvokerID);
            Assert.AreEqual(dataObject.RemoteServiceType, clonedObject.RemoteServiceType);
            Assert.AreEqual(dataObject.ResourceID, clonedObject.ResourceID);
            Assert.AreEqual(dataObject.ReturnType, clonedObject.ReturnType);
            Assert.AreEqual(dataObject.ServerID, clonedObject.ServerID);
            Assert.AreEqual(dataObject.ClientID, clonedObject.ClientID);
            Assert.AreEqual(dataObject.ServiceName, clonedObject.ServiceName);
            Assert.AreEqual(dataObject.WorkflowInstanceId, clonedObject.WorkflowInstanceId);
            Assert.AreEqual(dataObject.WorkflowResumeable, clonedObject.WorkflowResumeable);
            Assert.AreEqual(dataObject.WorkspaceID, clonedObject.WorkspaceID);
            Assert.AreEqual(dataObject.ThreadsToDispose, clonedObject.ThreadsToDispose);
            Assert.AreEqual(dataObject.ParentID, clonedObject.ParentID);
            Assert.AreEqual(dataObject.RunWorkflowAsync, clonedObject.RunWorkflowAsync);
            Assert.AreEqual(dataObject.IsDebugNested, clonedObject.IsDebugNested);
            Assert.AreEqual(dataObject.ForEachNestingLevel, clonedObject.ForEachNestingLevel);
            Assert.AreEqual(dataObject.StopExecution, clonedObject.StopExecution);
            Assert.AreEqual(dataObject.SourceResourceID, clonedObject.SourceResourceID);
            Assert.AreEqual(dataObject.TestName, clonedObject.TestName);
            Assert.AreEqual(dataObject.IsServiceTestExecution, clonedObject.IsServiceTestExecution);
            Assert.AreNotEqual(dataObject.ServiceTest, clonedObject.ServiceTest);
            Assert.AreEqual(dataObject.ServiceTest.TestName, clonedObject.ServiceTest.TestName);
        }

        #region Debug Mode Test

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataObject_IsDebugMode")]
        public void DataObject_IsDebugMode_IsDebugIsTrueAndRunWorkflowAsyncIsTrue_IsDebugModeIsFalse()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "<x>1</x>");
            dataObject.RunWorkflowAsync = true;
            dataObject.IsDebug = true;
            //------------Execute Test---------------------------
            var isDebug = dataObject.IsDebugMode();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDebug);
        }

        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataObject_IsDebugMode")]
        public void DataObject_IsDebugMode_IsDebugIsTrueAndRunWorkflowAsyncIsTrue_RemoteInvokeNotDebug()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "<x>1</x>");
            dataObject.RunWorkflowAsync = true;
            dataObject.IsDebug = true;
            dataObject.RemoteNonDebugInvoke = true;
            //------------Execute Test---------------------------
            var isDebug = dataObject.IsDebugMode();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDebug);
        }
        [TestMethod]
        [Owner("Leon Rajindrapersadh")]
        [TestCategory("DataObject_IsDebugMode")]
        public void DataObject_IsDebugMode_IsDebugIsTrueAndRunWorkflowAsyncIsTrue_RemoteInvokeNotDebugRemote()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "<x>1</x>");
            dataObject.RunWorkflowAsync = false;
            dataObject.IsDebug = false;
            dataObject.RemoteNonDebugInvoke = true;
            dataObject.RemoteInvoke = true;
            //------------Execute Test---------------------------
            var isDebug = dataObject.IsDebugMode();
            //------------Assert Results-------------------------
            Assert.IsTrue(isDebug);
        }


        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataObject_IsDebugMode")]
        public void DataObject_IsDebugMode_IsDebugIsTrueAndRunWorkflowAsyncIsFalse_IsDebugModeIsTrue()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "<x>1</x>");
            dataObject.RunWorkflowAsync = false;
            dataObject.IsDebug = true;
            //------------Execute Test---------------------------
            var isDebug = dataObject.IsDebugMode();
            //------------Assert Results-------------------------
            Assert.IsTrue(isDebug);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataObject_IsDebugMode")]
        public void DataObject_IsDebugMode_RemoteInvokeIsTrueAndRunWorkflowAsyncIsTrue_IsDebugModeIsFalse()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "<x>1</x>");
            dataObject.RunWorkflowAsync = true;
            dataObject.RemoteInvoke = true;
            //------------Execute Test---------------------------
            var isDebug = dataObject.IsDebugMode();
            //------------Assert Results-------------------------
            Assert.IsFalse(isDebug);
        }

        [TestMethod]
        [Owner("Tshepo Ntlhokoa")]
        [TestCategory("DataObject_IsDebugMode")]
        public void DataObject_IsDebugMode_RemoteInvokeIsTrueAndRunWorkflowAsyncIsFalse_IsDebugModeIsTrue()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "<x>1</x>");
            dataObject.RunWorkflowAsync = false;
            dataObject.RemoteInvoke = true;
            //------------Execute Test---------------------------
            var isDebug = dataObject.IsDebugMode();
            //------------Assert Results-------------------------
            Assert.IsTrue(isDebug);
        }

        #endregion

        #region Constructor Test

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataObject_Ctor")]
        public void DataObject_Ctor_WhenXmlStringContainsAllButDataMergePropertiesSet_ExpectParseAndSet()
        {
            //------------Setup for test--------------------------
            var debugID = Guid.NewGuid();
            var envID = Guid.NewGuid();
            var exeID = Guid.NewGuid();
            var bookmarkID = Guid.NewGuid();
            var parentID = Guid.NewGuid();
            var instID = Guid.NewGuid();

            var xmlStr = "<Payload>" +
                         "<IsDebug>true</IsDebug>" +
                         "<DebugSessionID>" + debugID + "</DebugSessionID>" +
                         "<EnvironmentID>" + envID + "</EnvironmentID>" +
                         "<IsOnDemandSimulation>true</IsOnDemandSimulation>" +
                         "<ParentServiceName>TestParentService</ParentServiceName>" +
                         "<ExecutionCallbackID>" + exeID + "</ExecutionCallbackID>" +
                         "<BookmarkExecutionCallbackID>" + bookmarkID + "</BookmarkExecutionCallbackID>" +
                         "<ParentInstanceID>" + parentID + "</ParentInstanceID>" +
                         "<NumberOfSteps>5</NumberOfSteps>" +
                         "<CurrentBookmarkName>MyBookmark</CurrentBookmarkName>" +
                         "<WorkflowInstanceId>" + instID + "</WorkflowInstanceId>" +
                         "<IsDataListScoped>true</IsDataListScoped>" +
                         "<Service>MyTestService</Service>" +
                         "</Payload>";


            //------------Execute Test---------------------------
            var dataObjct = new DsfDataObject(xmlStr, Guid.NewGuid(), "<x>1</x>");

            //------------Assert Results-------------------------
            Assert.IsTrue(dataObjct.IsDebug);
            StringAssert.Contains(dataObjct.DebugSessionID.ToString(), debugID.ToString());
            StringAssert.Contains(dataObjct.EnvironmentID.ToString(), envID.ToString());
            Assert.IsTrue(dataObjct.IsOnDemandSimulation);
            StringAssert.Contains(dataObjct.ParentServiceName, "TestParentService");
            StringAssert.Contains(dataObjct.ExecutionCallbackID.ToString(), exeID.ToString());
            StringAssert.Contains(dataObjct.BookmarkExecutionCallbackID.ToString(), bookmarkID.ToString());
            StringAssert.Contains(dataObjct.ParentInstanceID, parentID.ToString());
            Assert.AreEqual(5, dataObjct.NumberOfSteps, "Wrong number of steps");
            StringAssert.Contains(dataObjct.CurrentBookmarkName, "MyBookmark");
            StringAssert.Contains(dataObjct.WorkflowInstanceId.ToString(), instID.ToString());
            Assert.IsTrue(dataObjct.IsDataListScoped);
            StringAssert.Contains(dataObjct.ServiceName, "MyTestService");
            StringAssert.Contains(dataObjct.RawPayload.ToString(), xmlStr);

            // Default Data Merge Checks
            StringAssert.Contains(dataObjct.DatalistOutMergeID.ToString(), Guid.Empty.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeType.ToString(), enDataListMergeTypes.Intersection.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeDepth.ToString(), enTranslationDepth.Data_With_Blank_OverWrite.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeFrequency.ToString(), DataListMergeFrequency.OnCompletion.ToString());

            StringAssert.Contains(dataObjct.DatalistInMergeID.ToString(), Guid.Empty.ToString());
            StringAssert.Contains(dataObjct.DatalistInMergeType.ToString(), enDataListMergeTypes.Intersection.ToString());
            StringAssert.Contains(dataObjct.DatalistInMergeDepth.ToString(), enTranslationDepth.Data_With_Blank_OverWrite.ToString());

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataObject_Ctor")]
        public void DataObject_Ctor_WhenXmlStringgDoesNotHaveIsDebugModeSetButHasBDSDebugModeSet_ExpectParseAndSetAndIsDebugStillTrue()
        {
            //------------Setup for test--------------------------
            var debugID = Guid.NewGuid();
            var envID = Guid.NewGuid();
            var exeID = Guid.NewGuid();
            var bookmarkID = Guid.NewGuid();
            var parentID = Guid.NewGuid();
            var instID = Guid.NewGuid();

            var xmlStr = "<Payload>" +
                         "<BDSDebugMode>true</BDSDebugMode>" +
                         "<DebugSessionID>" + debugID + "</DebugSessionID>" +
                         "<EnvironmentID>" + envID + "</EnvironmentID>" +
                         "<IsOnDemandSimulation>true</IsOnDemandSimulation>" +
                         "<ParentServiceName>TestParentService</ParentServiceName>" +
                         "<ExecutionCallbackID>" + exeID + "</ExecutionCallbackID>" +
                         "<BookmarkExecutionCallbackID>" + bookmarkID + "</BookmarkExecutionCallbackID>" +
                         "<ParentInstanceID>" + parentID + "</ParentInstanceID>" +
                         "<NumberOfSteps>5</NumberOfSteps>" +
                         "<CurrentBookmarkName>MyBookmark</CurrentBookmarkName>" +
                         "<WorkflowInstanceId>" + instID + "</WorkflowInstanceId>" +
                         "<IsDataListScoped>true</IsDataListScoped>" +
                         "<Service>MyTestService</Service>" +
                         "</Payload>";


            //------------Execute Test---------------------------
            var dataObjct = new DsfDataObject(xmlStr, Guid.NewGuid(), "<x>1</x>");

            //------------Assert Results-------------------------
            Assert.IsTrue(dataObjct.IsDebug);
            StringAssert.Contains(dataObjct.DebugSessionID.ToString(), debugID.ToString());
            StringAssert.Contains(dataObjct.EnvironmentID.ToString(), envID.ToString());
            Assert.IsTrue(dataObjct.IsOnDemandSimulation);
            StringAssert.Contains(dataObjct.ParentServiceName, "TestParentService");
            StringAssert.Contains(dataObjct.ExecutionCallbackID.ToString(), exeID.ToString());
            StringAssert.Contains(dataObjct.BookmarkExecutionCallbackID.ToString(), bookmarkID.ToString());
            StringAssert.Contains(dataObjct.ParentInstanceID, parentID.ToString());
            Assert.AreEqual(5, dataObjct.NumberOfSteps, "Wrong number of steps");
            StringAssert.Contains(dataObjct.CurrentBookmarkName, "MyBookmark");
            StringAssert.Contains(dataObjct.WorkflowInstanceId.ToString(), instID.ToString());
            Assert.IsTrue(dataObjct.IsDataListScoped);
            StringAssert.Contains(dataObjct.ServiceName, "MyTestService");
            StringAssert.Contains(dataObjct.RawPayload.ToString(), xmlStr);

            // Default Data Merge Checks
            StringAssert.Contains(dataObjct.DatalistOutMergeID.ToString(), Guid.Empty.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeType.ToString(), enDataListMergeTypes.Intersection.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeDepth.ToString(), enTranslationDepth.Data_With_Blank_OverWrite.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeFrequency.ToString(), DataListMergeFrequency.OnCompletion.ToString());

            StringAssert.Contains(dataObjct.DatalistInMergeID.ToString(), Guid.Empty.ToString());
            StringAssert.Contains(dataObjct.DatalistInMergeType.ToString(), enDataListMergeTypes.Intersection.ToString());
            StringAssert.Contains(dataObjct.DatalistInMergeDepth.ToString(), enTranslationDepth.Data_With_Blank_OverWrite.ToString());

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataObject_Ctor")]
        public void DataObject_Ctor_WhenXmlStringDoesNotHaveBookmarkExecutionCallbackIDSet_ExpectParseAndSet()
        {
            //------------Setup for test--------------------------
            var debugID = Guid.NewGuid();
            var envID = Guid.NewGuid();
            var exeID = Guid.NewGuid();
            var parentID = Guid.NewGuid();
            var instID = Guid.NewGuid();

            var xmlStr = "<Payload>" +
                         "<IsDebug>true</IsDebug>" +
                         "<DebugSessionID>" + debugID + "</DebugSessionID>" +
                         "<EnvironmentID>" + envID + "</EnvironmentID>" +
                         "<IsOnDemandSimulation>true</IsOnDemandSimulation>" +
                         "<ParentServiceName>TestParentService</ParentServiceName>" +
                         "<ExecutionCallbackID>" + exeID + "</ExecutionCallbackID>" +
                         "<ParentInstanceID>" + parentID + "</ParentInstanceID>" +
                         "<NumberOfSteps>5</NumberOfSteps>" +
                         "<CurrentBookmarkName>MyBookmark</CurrentBookmarkName>" +
                         "<WorkflowInstanceId>" + instID + "</WorkflowInstanceId>" +
                         "<IsDataListScoped>true</IsDataListScoped>" +
                         "<Service>MyTestService</Service>" +
                         "</Payload>";


            //------------Execute Test---------------------------
            var dataObjct = new DsfDataObject(xmlStr, Guid.NewGuid(), "<x>1</x>");

            //------------Assert Results-------------------------
            Assert.IsTrue(dataObjct.IsDebug);
            StringAssert.Contains(dataObjct.DebugSessionID.ToString(), debugID.ToString());
            StringAssert.Contains(dataObjct.EnvironmentID.ToString(), envID.ToString());
            Assert.IsTrue(dataObjct.IsOnDemandSimulation);
            StringAssert.Contains(dataObjct.ParentServiceName, "TestParentService");
            StringAssert.Contains(dataObjct.ExecutionCallbackID.ToString(), exeID.ToString());
            StringAssert.Contains(dataObjct.BookmarkExecutionCallbackID.ToString(), exeID.ToString());
            StringAssert.Contains(dataObjct.ParentInstanceID, parentID.ToString());
            Assert.AreEqual(5, dataObjct.NumberOfSteps, "Wrong number of steps");
            StringAssert.Contains(dataObjct.CurrentBookmarkName, "MyBookmark");
            StringAssert.Contains(dataObjct.WorkflowInstanceId.ToString(), instID.ToString());
            Assert.IsTrue(dataObjct.IsDataListScoped);
            StringAssert.Contains(dataObjct.ServiceName, "MyTestService");
            StringAssert.Contains(dataObjct.RawPayload.ToString(), xmlStr);

            // Default Data Merge Checks
            StringAssert.Contains(dataObjct.DatalistOutMergeID.ToString(), Guid.Empty.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeType.ToString(), enDataListMergeTypes.Intersection.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeDepth.ToString(), enTranslationDepth.Data_With_Blank_OverWrite.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeFrequency.ToString(), DataListMergeFrequency.OnCompletion.ToString());

            StringAssert.Contains(dataObjct.DatalistInMergeID.ToString(), Guid.Empty.ToString());
            StringAssert.Contains(dataObjct.DatalistInMergeType.ToString(), enDataListMergeTypes.Intersection.ToString());
            StringAssert.Contains(dataObjct.DatalistInMergeDepth.ToString(), enTranslationDepth.Data_With_Blank_OverWrite.ToString());

        }

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DataObject_Ctor")]
        public void DataObject_Ctor_WhenXmlStringContainsAllIncludingDataMergePropertiesSet_ExpectParseAndSet()
        {
            //------------Setup for test--------------------------
            var debugID = Guid.NewGuid();
            var envID = Guid.NewGuid();
            var exeID = Guid.NewGuid();
            var bookmarkID = Guid.NewGuid();
            var parentID = Guid.NewGuid();
            var instID = Guid.NewGuid();
            var mergeIDOut = Guid.NewGuid();
            var mergeIDIn = Guid.NewGuid();

            var xmlStr = "<Payload>" +
                         "<IsDebug>true</IsDebug>" +
                         "<DebugSessionID>" + debugID + "</DebugSessionID>" +
                         "<EnvironmentID>" + envID + "</EnvironmentID>" +
                         "<IsOnDemandSimulation>true</IsOnDemandSimulation>" +
                         "<ParentServiceName>TestParentService</ParentServiceName>" +
                         "<ExecutionCallbackID>" + exeID + "</ExecutionCallbackID>" +
                         "<BookmarkExecutionCallbackID>" + bookmarkID + "</BookmarkExecutionCallbackID>" +
                         "<ParentInstanceID>" + parentID + "</ParentInstanceID>" +
                         "<NumberOfSteps>5</NumberOfSteps>" +
                         "<CurrentBookmarkName>MyBookmark</CurrentBookmarkName>" +
                         "<WorkflowInstanceId>" + instID + "</WorkflowInstanceId>" +
                         "<IsDataListScoped>true</IsDataListScoped>" +
                         "<Service>MyTestService</Service>" +
                         "<DatalistOutMergeID>" + mergeIDOut + "</DatalistOutMergeID>" +
                         "<DatalistOutMergeType>Union</DatalistOutMergeType>" +
                         "<DatalistOutMergeDepth>Data</DatalistOutMergeDepth>" +
                         "<DatalistOutMergeFrequency>Never</DatalistOutMergeFrequency>" +
                         "<DatalistInMergeID>" + mergeIDIn + "</DatalistInMergeID>" +
                         "<DatalistInMergeType>Union</DatalistInMergeType>" +
                         "<DatalistInMergeDepth>Data</DatalistInMergeDepth>" +
                         "</Payload>";


            //------------Execute Test---------------------------
            var dataObjct = new DsfDataObject(xmlStr, Guid.NewGuid(), "<x>1</x>");

            //------------Assert Results-------------------------
            Assert.IsTrue(dataObjct.IsDebug);
            StringAssert.Contains(dataObjct.DebugSessionID.ToString(), debugID.ToString());
            StringAssert.Contains(dataObjct.EnvironmentID.ToString(), envID.ToString());
            Assert.IsTrue(dataObjct.IsOnDemandSimulation);
            StringAssert.Contains(dataObjct.ParentServiceName, "TestParentService");
            StringAssert.Contains(dataObjct.ExecutionCallbackID.ToString(), exeID.ToString());
            StringAssert.Contains(dataObjct.BookmarkExecutionCallbackID.ToString(), bookmarkID.ToString());
            StringAssert.Contains(dataObjct.ParentInstanceID, parentID.ToString());
            Assert.AreEqual(5, dataObjct.NumberOfSteps, "Wrong number of steps");
            StringAssert.Contains(dataObjct.CurrentBookmarkName, "MyBookmark");
            StringAssert.Contains(dataObjct.WorkflowInstanceId.ToString(), instID.ToString());
            Assert.IsTrue(dataObjct.IsDataListScoped);
            StringAssert.Contains(dataObjct.ServiceName, "MyTestService");
            StringAssert.Contains(dataObjct.RawPayload.ToString(), xmlStr);

            // Data Merge Checks
            StringAssert.Contains(dataObjct.DatalistOutMergeID.ToString(), mergeIDOut.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeType.ToString(), enDataListMergeTypes.Union.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeDepth.ToString(), enTranslationDepth.Data.ToString());
            StringAssert.Contains(dataObjct.DatalistOutMergeFrequency.ToString(), DataListMergeFrequency.Never.ToString());

            StringAssert.Contains(dataObjct.DatalistInMergeID.ToString(), mergeIDIn.ToString());
            StringAssert.Contains(dataObjct.DatalistInMergeType.ToString(), enDataListMergeTypes.Union.ToString());
            StringAssert.Contains(dataObjct.DatalistInMergeDepth.ToString(), enTranslationDepth.Data.ToString());

        }

        #endregion
    }
}
