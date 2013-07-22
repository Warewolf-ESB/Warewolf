namespace Dev2.Activities
{
    public class MockDsfPluginActivity : DsfPluginActivity
    {
        public void MockCleanDataList(Runtime.Helpers.RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, System.Guid workspaceID, DataList.Contract.ErrorResultTO errorResultTo, DataList.Contract.IDataListCompiler compiler)
        {
            CleanDataList(runtimeHelpers, dataObject, workspaceID, errorResultTo, compiler);
        }

        public Services.Execution.PluginServiceExecution MockGetNewPluginServiceExecution(Runtime.ServiceModel.Data.PluginService resource, System.Guid dlid)
        {
            return GetNewPluginServiceExecution(resource, dlid);
        }

        public System.Guid MockExecutePluginService(Services.Execution.PluginServiceExecution container, out DataList.Contract.ErrorResultTO errorsTo)
        {
            return ExecutePluginService(container, out errorsTo);
        }
    }
}