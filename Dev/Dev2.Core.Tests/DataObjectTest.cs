using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.DynamicServices;
using Dev2.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

// ReSharper disable InconsistentNaming
namespace Dev2.Tests
{
    [TestClass]
    [ExcludeFromCodeCoverage]
    public class DataObjectTest
    {

        [TestMethod]
        [Owner("Travis Frisinger")]
        [TestCategory("DsfDataObject_Clone")]
        public void DsfDataObject_Clone_NormalClone_FullDuplicationForProperties()
        {
            //------------Setup for test--------------------------
            IDSFDataObject dataObject = new DsfDataObject(string.Empty, Guid.NewGuid(), "<x>1</x>");

            dataObject.BookmarkExecutionCallbackID = Guid.NewGuid();
            dataObject.CurrentBookmarkName = "def";
            dataObject.DataList = "<x/>";
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
            dataObject.Errors = new ErrorResultTO();
            dataObject.Errors.AddError("my error");
            dataObject.ExecutionCallbackID = Guid.NewGuid();
            dataObject.ExecutionOrigin = ExecutionOrigin.Debug;
            dataObject.ExecutionOriginDescription = "xxx";
            dataObject.ForceDeleteAtNextNativeActivityCleanup = true;
            dataObject.IsDataListScoped = false;
            dataObject.IsDebug = true;
            dataObject.IsFromWebServer = true;
            dataObject.IsOnDemandSimulation = true;
            dataObject.IsWebpage = false;
            dataObject.NumberOfSteps = 2;
            dataObject.OriginalInstanceID = Guid.NewGuid();
            dataObject.ParentInstanceID = "1211";
            dataObject.ParentServiceName = "xxx";
            dataObject.ParentThreadID = 2;
            dataObject.ParentWorkflowInstanceId = "1233";
            dataObject.RawPayload = "<raw>a</raw>";
            dataObject.RemoteDebugItems = new List<DebugState>();
            dataObject.RemoteInvoke = false;
            dataObject.RemoteInvokeResultShape = "<x/>";
            dataObject.RemoteInvokerID = "999";
            dataObject.RemoteServiceType = "NA";
            dataObject.ResourceID = Guid.NewGuid();
            dataObject.ReturnType = EmitionTypes.XML;
            dataObject.ServerID = Guid.NewGuid();
            dataObject.ServiceName = "xxx";
            dataObject.WorkflowInstanceId = "333";
            dataObject.WorkflowResumeable = false;
            dataObject.ParentID = Guid.NewGuid();
            dataObject.WorkspaceID = Guid.NewGuid();
            dataObject.ClientID = Guid.NewGuid();
            dataObject.RunWorkflowAsync = true;
            dataObject.IsDebugNested = true;
            var threadsToDispose = new Dictionary<int, List<Guid>>();
            List<Guid> guidList = new List<Guid> { Guid.NewGuid() };
            threadsToDispose.Add(3, guidList);
            dataObject.ThreadsToDispose = threadsToDispose;

            //------------Execute Test---------------------------
            IDSFDataObject clonedObject = dataObject.Clone();

            //------------Assert Results-------------------------

            // check counts, then check values
            var properties = typeof(IDSFDataObject).GetProperties();
            Assert.AreEqual(49, properties.Length);

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
            Assert.AreEqual(dataObject.Errors, clonedObject.Errors);
            Assert.AreEqual(dataObject.ExecutingUser, clonedObject.ExecutingUser);
            Assert.AreEqual(dataObject.ExecutionCallbackID, clonedObject.ExecutionCallbackID);
            Assert.AreEqual(dataObject.ExecutionOrigin, clonedObject.ExecutionOrigin);
            Assert.AreEqual(dataObject.ExecutionOriginDescription, clonedObject.ExecutionOriginDescription);
            Assert.AreEqual(dataObject.ForceDeleteAtNextNativeActivityCleanup, clonedObject.ForceDeleteAtNextNativeActivityCleanup);
            Assert.AreEqual(dataObject.IsDataListScoped, clonedObject.IsDataListScoped);
            Assert.AreEqual(dataObject.IsDebug, clonedObject.IsDebug);
            Assert.AreEqual(dataObject.IsFromWebServer, clonedObject.IsFromWebServer);
            Assert.AreEqual(dataObject.IsOnDemandSimulation, clonedObject.IsOnDemandSimulation);
            Assert.AreEqual(dataObject.IsRemoteWorkflow, clonedObject.IsRemoteWorkflow);
            Assert.AreEqual(dataObject.IsWebpage, clonedObject.IsWebpage);
            Assert.AreEqual(dataObject.NumberOfSteps, clonedObject.NumberOfSteps);
            Assert.AreEqual(dataObject.OriginalInstanceID, clonedObject.OriginalInstanceID);
            Assert.AreEqual(dataObject.ParentInstanceID, clonedObject.ParentInstanceID);
            Assert.AreEqual(dataObject.ParentServiceName, clonedObject.ParentServiceName);
            Assert.AreEqual(dataObject.ParentThreadID, clonedObject.ParentThreadID);
            Assert.AreEqual(dataObject.ParentWorkflowInstanceId, clonedObject.ParentWorkflowInstanceId);
            Assert.AreEqual(dataObject.RawPayload, clonedObject.RawPayload);
            Assert.AreEqual(dataObject.RemoteDebugItems, clonedObject.RemoteDebugItems);
            Assert.AreEqual(dataObject.RemoteInvoke, clonedObject.RemoteInvoke);
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
        }

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
    }
}
