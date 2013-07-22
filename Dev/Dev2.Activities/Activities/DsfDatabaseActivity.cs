using System;
using Dev2.DataList.Contract;
using Dev2.Runtime.Helpers;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfDatabaseActivity : DsfActivity
    {
        ErrorResultTO _errorsTo;

        #region Overrides of DsfActivity

        protected override Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, out ErrorResultTO tmpErrors)
        {
            tmpErrors = new ErrorResultTO();
            var compiler = DataListFactory.CreateDataListCompiler();
            _errorsTo = new ErrorResultTO();
            CleanDataList(new RuntimeHelpers(), dataObject, dataObject.WorkspaceID, compiler);
            var databaseServiceExecution = GetNewDatabaseServiceExecution(dataObject);

            tmpErrors.MergeErrors(_errorsTo);
            return ExecuteDatabaseService(databaseServiceExecution);
        }

        #endregion

        #region Protected Helper Functions

        protected virtual DatabaseServiceExecution GetNewDatabaseServiceExecution(IDSFDataObject dataObject)
        {
            return new DatabaseServiceExecution(dataObject);
        }

        protected virtual Guid ExecuteDatabaseService(DatabaseServiceExecution container)
        {
            return container.Execute(out _errorsTo);
        }

        protected virtual void CleanDataList(RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, Guid workspaceID, IDataListCompiler compiler)
        {
            runtimeHelpers.GetCorrectDataList(dataObject, workspaceID, _errorsTo, compiler);
        }

        #endregion
    }
}