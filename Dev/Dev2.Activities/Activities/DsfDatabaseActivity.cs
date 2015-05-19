
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
    public class DsfDatabaseActivity : DsfActivity
    {
        public IServiceExecution ServiceExecution { get; protected set; }

        #region Overrides of DsfActivity

        protected override Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO errors)
        {
            var execErrors = new ErrorResultTO();
            var compiler = DataListFactory.CreateDataListCompiler();
            var oldID = dataObject.DataListID;

            errors = new ErrorResultTO();
            errors.MergeErrors(execErrors);

            var databaseServiceExecution = ServiceExecution as DatabaseServiceExecution;
            if(databaseServiceExecution != null)
            {
                databaseServiceExecution.InstanceInputDefinitions = inputs; // set the output mapping for the instance ;)
                databaseServiceExecution.InstanceOutputDefintions = outputs; // set the output mapping for the instance ;)
            }
            //ServiceExecution.DataObj = dataObject;
            var result = ServiceExecution.Execute(out execErrors);
            var fetchErrors = execErrors.FetchErrors();
            foreach(var error in fetchErrors)
            {
                dataObject.Environment.Errors.Add(error);
            }
            
            errors.MergeErrors(execErrors);

            // Adjust the remaining output mappings ;)
            compiler.SetParentID(dataObject.DataListID, oldID);
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
    }
}
