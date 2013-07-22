namespace Dev2.Activities
{
    public class MockDsfDatabaseActivity : DsfDatabaseActivity
    {
        public void MockCleanDataList(Runtime.Helpers.RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, System.Guid workspaceID, DataList.Contract.ErrorResultTO errorResultTo, DataList.Contract.IDataListCompiler compiler)
        {
            CleanDataList(runtimeHelpers, dataObject, workspaceID, errorResultTo, compiler);
        }

        public Services.Execution.DatabaseServiceExecution MockGetNewDatabaseServiceExecution(Runtime.ServiceModel.Data.DbService resource, System.Guid dlid)
        {
            return GetNewDatabaseServiceExecution(null);
        }

        public System.Guid MockExecuteDatabaseService(Services.Execution.DatabaseServiceExecution container, out DataList.Contract.ErrorResultTO errorsTo)
        {
            return ExecuteDatabaseService(container, out errorsTo);
        }
    }
}