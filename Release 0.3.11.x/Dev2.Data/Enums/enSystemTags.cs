/*
 *  List of system tags, please update as required
 */
namespace Dev2.DataList.Contract
{

    /*
     * 17.07.2012 - Travis.Frisinger 
     * 
     * Please note any additional tags added here, but be added to the 
     * DataListUtil class's IsSystemTag method as this avoids any nasty
     * issues where a tag below is seen as a recordset with nested data
     * causing all sorts of havoc with the Intellisense parsing
     * 
     * 20.09.2013 - Travis.Frisinger
     * 
     * Removed a ton of useless tags ;)
     */
    public enum enSystemTag {
        InstanceId,
        Bookmark,
        ParentWorkflowInstanceId,
        ParentServiceName,
        ParentInstanceID,
        Resumption,
        Dev2ResumeData,
        Dev2Error,
        SystemModel,
        Service,
        WorkflowInstanceId,
        ManagmentServicePayload,
        wid,
        PostData
    }
}
