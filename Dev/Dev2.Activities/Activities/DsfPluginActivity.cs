
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2014 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using Dev2.DataList.Contract;
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

            ErrorResultTO invokeErrors;
            esbChannel.CorrectDataList(dataObject, dataObject.WorkspaceID, out invokeErrors, compiler);

            dataObject.DataListID = compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Input, inputs, out invokeErrors);
            _errorsTo.MergeErrors(invokeErrors);

            _errorsTo.MergeErrors(invokeErrors);
            var pluginServiceExecution = GetNewPluginServiceExecution(dataObject);
            tmpErrors = new ErrorResultTO();
            tmpErrors.MergeErrors(_errorsTo);
            var result = ExecutePluginService(pluginServiceExecution);
            tmpErrors.MergeErrors(_errorsTo);
            return result;
        }

        #endregion

        #region Protected Helper Functions

        protected virtual Guid ExecutePluginService(PluginServiceExecution container)
        {
            return container.Execute(out _errorsTo);
        }

        protected virtual PluginServiceExecution GetNewPluginServiceExecution(IDSFDataObject context)
        {
            return new PluginServiceExecution(context, true);
        }

        #endregion
    }
}
