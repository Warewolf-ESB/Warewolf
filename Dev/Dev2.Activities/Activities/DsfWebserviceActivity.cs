/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using Dev2.Data.TO;
using Dev2.Interfaces;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfWebserviceActivity : DsfActivity
    {
        #region Overrides of DsfActivity

        protected override void ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors,int update)
        {
            tmpErrors = new ErrorResultTO();
            var webserviceExecution = GetNewWebserviceExecution(dataObject);


            if(webserviceExecution != null && !tmpErrors.HasErrors())
            {
                webserviceExecution.InstanceOutputDefintions = outputs; // set the output mapping for the instance ;)
                webserviceExecution.InstanceInputDefinitions = inputs;
                ErrorResultTO invokeErrors;
                webserviceExecution.Execute(out invokeErrors, update);
                string err = invokeErrors.MakeDataListReady();
                if(!string.IsNullOrEmpty(err))
                dataObject.Environment.AddError(err);
            }
        }

        #endregion

        #region Protected Helper Functions

        protected virtual WebserviceExecution GetNewWebserviceExecution(IDSFDataObject context)
        {
            return new WebserviceExecution(context, false);
        }

        #endregion
    }
}
