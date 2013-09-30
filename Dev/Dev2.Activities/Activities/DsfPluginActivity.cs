using System;
using Dev2.DataList.Contract;
using Dev2.Runtime.Helpers;
using Dev2.Runtime.Hosting;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfPluginActivity : DsfActivity
    {
        ErrorResultTO _errorsTo;

        #region Overrides of DsfActivity

        protected override Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors)
        {
            _errorsTo = new ErrorResultTO();
            var compiler = DataListFactory.CreateDataListCompiler();
            CleanDataList(new RuntimeHelpers(), dataObject, dataObject.WorkspaceID, compiler);
            var pluginServiceExecution = GetNewPluginServiceExecution(dataObject);
            tmpErrors = new ErrorResultTO();
            tmpErrors.MergeErrors(_errorsTo);
            return ExecutePluginService(pluginServiceExecution);
        }

        #endregion

        #region Protected Helper Functions

        protected virtual Guid ExecutePluginService(PluginServiceExecution container)
        {
            return container.Execute(out _errorsTo);
        }

        protected virtual PluginServiceExecution GetNewPluginServiceExecution(IDSFDataObject context)
        {
            return new PluginServiceExecution(context, false);
        }

        protected virtual void CleanDataList(RuntimeHelpers runtimeHelpers, IDSFDataObject dataObject, Guid workspaceID, IDataListCompiler compiler)
        {
            runtimeHelpers.GetCorrectDataList(dataObject, workspaceID, _errorsTo, compiler);
        }

        #endregion
    }
}