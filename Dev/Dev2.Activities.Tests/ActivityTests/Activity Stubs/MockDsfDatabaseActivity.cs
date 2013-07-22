namespace Dev2.Activities
{
    public class MockDsfDatabaseActivity : DsfDatabaseActivity
    {
        public void MockCleanDataList(Runtime.Helpers.RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, System.Guid workspaceID, DataList.Contract.IDataListCompiler compiler)
        {
            CleanDataList(runtimeHelpers, dataObject, workspaceID, compiler);
        }

        public Services.Execution.DatabaseServiceExecution MockGetNewDatabaseServiceExecution(IDSFDataObject context)
        {
            return GetNewDatabaseServiceExecution(context);
        }

        public System.Guid MockExecuteDatabaseService(Services.Execution.DatabaseServiceExecution container)
        {
            return ExecuteDatabaseService(container);
        }

        public System.Guid MockExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, out DataList.Contract.ErrorResultTO tmpErrors)
        {
            return base.ExecutionImpl(esbChannel, dataObject, out tmpErrors);
        }
    }
}