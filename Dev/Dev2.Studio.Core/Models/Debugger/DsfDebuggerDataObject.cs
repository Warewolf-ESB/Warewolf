using System;
using System.Collections.Generic;
using Dev2.Data.Enums;
using System.Security.Principal;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;

namespace Dev2.Studio.Core.Models
{
    public class DsfDebuggerDataObject : IDSFDataObject
    {
        public Guid DebugSessionID { get; set; }
        public Guid ParentID { get; set; }
        public Guid EnvironmentID { get; set; }
        public bool IsRemoteWorkflow { get { return EnvironmentID != Guid.Empty; } }
        public int ParentThreadID { get; set; }

        public Dictionary<int, List<Guid>> ThreadsToDispose { get; set; }
        public string CurrentBookmarkName { get; set; }
        public string ParentServiceName { get; set; }
        public string ParentWorkflowInstanceId { get; set; }
        public string ServiceName { get; set; }
        public string WorkflowInstanceId { get; set; }
        public string XmlData { get; set; }
        public string DataList { get; set; }
        public string ParentWorkflowXmlData { get; set; }
        public bool WorkflowResumeable { get; set; }
        public bool IsDebug { get; set; }
        public Guid WorkspaceID { get; set; }
        public Guid OriginalInstanceID { get; set; }
        public bool IsOnDemandSimulation { get; set; }
        public Guid ServerID { get; set; }
        public Guid ClientID { get; set; }
        public Guid ResourceID { get; set; }
        public ErrorResultTO Errors { get; set; }
        public int NumberOfSteps { get; set; }
        public IPrincipal ExecutingUser { get; set; }

        public enTranslationDepth DatalistOutMergeDepth { get; set; }
        public DataListMergeFrequency DatalistOutMergeFrequency { get; set; }
        public Guid DatalistOutMergeID { get; set; }
        public enDataListMergeTypes DatalistOutMergeType { get; set; }

        public enTranslationDepth DatalistInMergeDepth { get; set; }
        public Guid DatalistInMergeID { get; set; }
        public enDataListMergeTypes DatalistInMergeType { get; set; }

        public Guid ExecutionCallbackID { get; set; }
        public Guid BookmarkExecutionCallbackID { get; set; }
        public string ParentInstanceID { get; set; }

        public Guid DataListID { get; set; }
        public string RawPayload { get; set; }
        public string RemoteInvokeUri { get; set; }
        public string RemoteInvokeResultShape { get; set; }
        public bool RemoteInvoke { get; set; }
        public string RemoteInvokerID { get; set; }
        public IList<DebugState> RemoteDebugItems { get; set; }
        public string RemoteServiceType { get; set; }
        public bool IsWebpage { get; set; }

        public IDSFDataObject Clone()
        {
            // brendon.page, 2012.11.18, This should never be used, if it is there are issues.
            throw new NotImplementedException();
        }

        public bool IsInDebugMode()
        {
            throw new NotImplementedException();
        }

        public ExecutionOrigin ExecutionOrigin { get; set; }
        public string ExecutionOriginDescription { get; set; }
        public bool IsFromWebServer { get; set; }

        public bool IsDebugMode()
        {
            throw new NotImplementedException();
        }

        public bool IsDataListScoped { get; set; }


        public bool ForceDeleteAtNextNativeActivityCleanup { get; set; }


        public Web.EmitionTypes ReturnType
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }
    }
}
