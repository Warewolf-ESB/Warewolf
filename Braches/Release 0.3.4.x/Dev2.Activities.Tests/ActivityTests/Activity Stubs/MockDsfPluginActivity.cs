namespace Dev2.Activities
{
    public class MockDsfPluginActivity : DsfPluginActivity
    {
        public void MockCleanDataList(Runtime.Helpers.RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, System.Guid workspaceID, DataList.Contract.ErrorResultTO errorResultTo, DataList.Contract.IDataListCompiler compiler)
        {
            CleanDataList(runtimeHelpers, dataObject, workspaceID, compiler);
        }

        public Services.Execution.PluginServiceExecution MockGetNewPluginServiceExecution(IDSFDataObject context)
        {
            return GetNewPluginServiceExecution(context);
        }

        public System.Guid MockExecutePluginService(Services.Execution.PluginServiceExecution container)
        {
            return ExecutePluginService(container);
        }

        public System.Guid MockExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, out DataList.Contract.ErrorResultTO tmpErrors)
        {
            return base.ExecutionImpl(esbChannel, dataObject, out tmpErrors);
        }
    }
}