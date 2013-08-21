using System;
using Dev2.DataList.Contract;
using Dev2.Runtime.Helpers;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfDatabaseActivity : DsfActivity
    {
        public IServiceExecution ServiceExecution { get; protected set; }

        #region Overrides of DsfActivity

        protected override Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, out ErrorResultTO tmpErrors)
        {
            tmpErrors = new ErrorResultTO();

            var execErrors = new ErrorResultTO();
            var compiler = DataListFactory.CreateDataListCompiler();
            CleanDataList(new RuntimeHelpers(), dataObject, dataObject.WorkspaceID, compiler, execErrors);
            tmpErrors.MergeErrors(execErrors);

            var result = ServiceExecution.Execute(out execErrors);
            tmpErrors.MergeErrors(execErrors);
            return result;
        }

        #region Overrides of DsfActivity

        protected override void BeforeExecutionStart(IDSFDataObject dataObject, ErrorResultTO tmpErrors)
        {
            base.BeforeExecutionStart(dataObject, tmpErrors);
            ServiceExecution = new DatabaseServiceExecution(dataObject);
            ServiceExecution.BeforeExecution(tmpErrors);
        }

        protected override void AfterExecutionCompleted(ErrorResultTO tmpErrors)
        {
            base.AfterExecutionCompleted(tmpErrors);
            ServiceExecution.AfterExecution(tmpErrors);
        }

        #endregion

        #endregion

        #region Protected Helper Functions

        protected virtual void CleanDataList(RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, Guid workspaceID, IDataListCompiler compiler, ErrorResultTO tmpErrors)
        {
            runtimeHelpers.GetCorrectDataList(dataObject, workspaceID, tmpErrors, compiler);
        }

        #endregion
    }
}