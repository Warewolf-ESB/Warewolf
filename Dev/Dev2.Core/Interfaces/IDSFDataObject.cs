using System;
using Dev2.DataList.Contract;

namespace Dev2
{
    public interface IDSFDataObject
    {
        //string XmlData { get; set; } // 2012.11.05 : Travis.Frisinger - Removed for Binary DataList
        string CurrentBookmarkName { get; set; }
        string WorkflowInstanceId { get; set; }
        string ServiceName { get; set; }
        string ParentWorkflowInstanceId { get; set; }
        string ParentServiceName { get; set; }
        string DataList { get; set; }
        //string ParentWorkflowXmlData { get; set; } // 2012.11.05 : Travis.Frisinger - Removed for Binary DataList
        bool WorkflowResumeable { get; set; }
        bool IsDebug { get; set; }
        Guid WorkspaceID { get; set; }
        bool IsOnDemandSimulation { get; set; }
        Guid ServerID { get; set; }
        ErrorResultTO Errors { get; set; }

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

        // 2012.11.05 : Travis.Frisinger - Added for Binary DataList
        Guid DataListID { get; set; }
        bool IsDataListScoped { get; set; }
        bool ForceDeleteAtNextNativeActivityCleanup { get; set; }
        string RawPayload { get; set; }

        IDSFDataObject Clone();
    }
}
