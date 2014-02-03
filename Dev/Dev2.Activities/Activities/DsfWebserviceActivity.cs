using System;
using Dev2.Common;
using Dev2.DataList.Contract;
using Dev2.Services.Execution;
using Unlimited.Applications.BusinessDesignStudio.Activities;

namespace Dev2.Activities
{
    public class DsfWebserviceActivity : DsfActivity
    {
        ErrorResultTO _errorsTo;

        #region Overrides of DsfActivity

        protected override Guid ExecutionImpl(IEsbChannel esbChannel, IDSFDataObject dataObject, string inputs, string outputs, out ErrorResultTO tmpErrors)
        {
            tmpErrors = new ErrorResultTO();
            ErrorResultTO invokeErrors;
            var compiler = DataListFactory.CreateDataListCompiler();
            var oldID = dataObject.DataListID;
            var webserviceExecution = GetNewWebserviceExecution(dataObject);
#pragma warning disable 168
            var remainingMappings = esbChannel.ShapeForSubRequest(dataObject, inputs, outputs, out invokeErrors);
#pragma warning restore 168
            tmpErrors.MergeErrors(invokeErrors);

            if(webserviceExecution != null)
            {
                webserviceExecution.InstanceOutputDefintions = outputs; // set the output mapping for the instance ;)
                var result = webserviceExecution.Execute(out invokeErrors);
                tmpErrors.MergeErrors(invokeErrors);

                // Adjust the remaining output mappings ;)
                compiler.SetParentID(dataObject.DataListID, oldID);

                compiler.ConvertFrom(dataObject.DataListID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out invokeErrors);
                tmpErrors.MergeErrors(invokeErrors);
                compiler.ConvertFrom(oldID, DataListFormat.CreateFormat(GlobalConstants._XML_Without_SystemTags), enTranslationDepth.Data, out invokeErrors);
                tmpErrors.MergeErrors(invokeErrors);
                return result;
            }
            return oldID;
        }

        #endregion

        #region Protected Helper Functions

        protected virtual Guid ExecuteWebservice(WebserviceExecution container)
        {
            return container.Execute(out _errorsTo);
        }

        protected virtual WebserviceExecution GetNewWebserviceExecution(IDSFDataObject context)
        {
            return new WebserviceExecution(context, true);
        }

        #endregion
    }
}
