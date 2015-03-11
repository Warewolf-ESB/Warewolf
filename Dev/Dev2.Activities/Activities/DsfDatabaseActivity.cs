
/*
*  Warewolf - The Easy Service Bus
*  Copyright 2015 by Warewolf Ltd <alpha@warewolf.io>
*  Licensed under GNU Affero General Public License 3.0 or later. 
*  Some rights reserved.
*  Visit our website for more information <http://warewolf.io/>
*  AUTHORS <http://warewolf.io/authors.php> , CONTRIBUTORS <http://warewolf.io/contributors.php>
*  @license GNU Affero General Public License <http://www.gnu.org/licenses/agpl-3.0.html>
*/

using System;
using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Common.Interfaces.Data;
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

            IList<KeyValuePair<enDev2ArgumentType, IList<IDev2Definition>>> remainingMappings = esbChannel.ShapeForSubRequest(dataObject, inputs, outputs, out errors);
            errors.MergeErrors(execErrors);

            var databaseServiceExecution = ServiceExecution as DatabaseServiceExecution;
            if(databaseServiceExecution != null)
            {
                databaseServiceExecution.InstanceOutputDefintions = outputs; // set the output mapping for the instance ;)
            }

            var result = ServiceExecution.Execute(out execErrors);
            errors.MergeErrors(execErrors);

            // Adjust the remaining output mappings ;)
            compiler.SetParentID(dataObject.DataListID, oldID);

            if(remainingMappings != null)
            {
                var outputMappings = remainingMappings.FirstOrDefault(c => c.Key == enDev2ArgumentType.Output);
                compiler.Shape(dataObject.DataListID, enDev2ArgumentType.Output, outputMappings.Value, out execErrors);
                errors.MergeErrors(execErrors);
            }
            else
            {
                compiler.Shape(dataObject.DataListID, enDev2ArgumentType.DB_ForEach, outputs, out execErrors);
                errors.MergeErrors(execErrors);
            }

            compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out execErrors);
            errors.MergeErrors(execErrors);
            compiler.ConvertFrom(oldID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out execErrors);
            errors.MergeErrors(execErrors);

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
