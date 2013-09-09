/*
 *  List of system tags, please update as required
 */
namespace Dev2.DataList.Contract
{

    /*
     * 17.07.2012 - Travis.Frisinger 
     * 
     * Please note any additional tags added here, but be added to the 
     * DataListUtil class's isSystemTag method as this avoids any nasty
     * issues where a tag below is seen as a recordset with nested data
     * causing all sorts of havoc with the Intellisense parsing
     * 
     */
    public enum enSystemTag {
        FormView,
        InstanceId,
        Bookmark,
        ParentWorkflowInstanceId,
        ParentServiceName,
        ParentInstanceID,
        BDSDebugMode,
        WebServerUrl,
        Service,
        WebPage,
        Dev2ServiceInput,
        Dev2UIServiceOutput,
        Resumption,
        InternalTransferDataList,
        WebXMLConfiguration,
        Dev2TransientGhostServiceInvoke,
        Dev2ResumeData,
        ActivityInput,
        CarriedSystemRegions,
        Dev2DesignTimeBinding,
        Fragment,
        DEV2WebsiteEditingMode,
        Dev2WebServer,
        DEV2DefaultWebpage,
        DEV2MetaTags,
        Dev2WebPage,
        Dev2Error,
        ServiceName,
        WorkflowInstanceId,
        WebpageInstance,
        Async,
        ManagmentServicePayload,
        EvaluateIteration,
        SubstituteTokens,
        SystemModel,
        wid,
        PostData
    }
}
