using System;
using System.Collections.Generic;
using System.Security.Principal;
using Dev2.Data.Enums;
using Dev2.DataList.Contract;
using Dev2.Diagnostics;
using Dev2.Diagnostics.Debug;
using Dev2.Web;

namespace Dev2
{
    public interface IDSFDataObject
    {
        Dictionary<int, List<Guid>> ThreadsToDispose { get; set; }

        string CurrentBookmarkName { get; set; }
        Guid WorkflowInstanceId { get; set; }
        string ServiceName { get; set; }
        string ParentWorkflowInstanceId { get; set; }
        string ParentServiceName { get; set; }
        string DataList { get; set; }
        bool WorkflowResumeable { get; set; }
        bool IsDebug { get; set; }
        Guid WorkspaceID { get; set; }
        Guid ResourceID { get; set; }
        Guid OriginalInstanceID { get; set; }
        Guid ClientID { get; set; }
        bool IsOnDemandSimulation { get; set; }
        Guid ServerID { get; set; }
        ErrorResultTO Errors { get; set; }
        int NumberOfSteps { get; set; }
        IPrincipal ExecutingUser { get; set; }
        Guid DatalistOutMergeID { get; set; }
        enDataListMergeTypes DatalistOutMergeType { get; set; }
        enTranslationDepth DatalistOutMergeDepth { get; set; }
        DataListMergeFrequency DatalistOutMergeFrequency { get; set; }

        Guid DatalistInMergeID { get; set; }
        enDataListMergeTypes DatalistInMergeType { get; set; }
        enTranslationDepth DatalistInMergeDepth { get; set; }

        Guid ExecutionCallbackID { get; set; }
        Guid BookmarkExecutionCallbackID { get; set; }
        string ParentInstanceID { get; set; }

        EmitionTypes ReturnType { get; set; }

        // 2012.11.05 : Travis.Frisinger - Added for Binary DataList
        Guid DataListID { get; set; }
        bool IsDataListScoped { get; set; }
        bool ForceDeleteAtNextNativeActivityCleanup { get; set; }
        string RawPayload { get; set; }
        string RemoteInvokeResultShape { get; set; }
        bool RemoteInvoke { get; set; }
        string RemoteInvokerID { get; set; }
        IList<IDebugState> RemoteDebugItems { get; set; }
        string RemoteServiceType { get; set; }

        // Massimo.Guerrera :15-04-2013 - Added for the detection of webpages in the webserver so that the system tags dont get striped
        bool IsWebpage { get; set; }

        IDSFDataObject Clone();

        ExecutionOrigin ExecutionOrigin { get; set; }
        string ExecutionOriginDescription { get; set; }
        bool IsFromWebServer { get; set; }

        bool IsDebugMode();

        Guid EnvironmentID { get; set; }
        bool IsRemoteWorkflow { get; }

        int ParentThreadID { get; set; }

        Guid DebugSessionID { get; set; }
        Guid ParentID { get; set; }

        bool RunWorkflowAsync { get; set; }
        bool IsDebugNested { get; set; }
    }
}
