
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2016 by Warewolf Ltd <alpha@warewolf.io>
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

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors, int update)
        {
            _errorsTo = new ErrorResultTO();
            var pluginServiceExecution = GetNewPluginServiceExecution(dataObject);
            pluginServiceExecution.InstanceInputDefinitions = inputs;
            pluginServiceExecution.InstanceOutputDefintions = outputs;
            tmpErrors = new ErrorResultTO();
            tmpErrors.MergeErrors(_errorsTo);
            ExecutePluginService(pluginServiceExecution, update);
            tmpErrors.MergeErrors(_errorsTo);
        }

        #endregion

        #region Protected Helper Functions

        protected virtual Guid ExecutePluginService(PluginServiceExecution container, int update)
        {
            return container.Execute(out _errorsTo, update);
        }

        protected virtual PluginServiceExecution GetNewPluginServiceExecution(IDSFDataObject context)
        {
            return new PluginServiceExecution(context, true);
        }

        #endregion
    }
}
