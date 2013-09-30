using System;
using System.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel.Helper;
using Dev2.DataList.Contract;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;

namespace Dev2.Runtime.Helpers
{
    /// <summary>
    /// This class MUST GO. Fetch the EsbChannel from the context to keep logic where it belongs!
    /// </summary>
    public class RuntimeHelpers
    {

        /// <summary>
        /// Gets the correct data list.
        /// </summary>
        /// <param name="dataObject">The data object.</param>
        /// <param name="workspaceID">The workspace unique identifier.</param>
        /// <param name="errors">The errors.</param>
        /// <param name="compiler">The compiler.</param>
        /// <returns></returns>
        /// WARNING : WHY THE FLIP DOES THIS LOGIC DUPLICATE THE ESB CHANNEL? 
        public virtual Guid GetCorrectDataList(IDSFDataObject dataObject, Guid workspaceID, ErrorResultTO errors, IDataListCompiler compiler)
        {
            string theShape;
            ErrorResultTO invokeErrors;

            // If no DLID, we need to make it based upon the request ;)
            if(dataObject.DataListID == GlobalConstants.NullDataListID)
            {
                theShape = FindServiceShape(workspaceID, dataObject.ServiceName, true);
                dataObject.DataListID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                    dataObject.RawPayload, theShape, out invokeErrors);
                errors.MergeErrors(invokeErrors);
                dataObject.RawPayload = string.Empty;
            }

            // force all items to exist in the DL ;)
            theShape = FindServiceShape(workspaceID, dataObject.ServiceName, true);
            var innerDatalistID = compiler.ConvertTo(DataListFormat.CreateFormat(GlobalConstants._XML),
                string.Empty, theShape, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            // Add left to right
            var left = compiler.FetchBinaryDataList(dataObject.DataListID, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            var right = compiler.FetchBinaryDataList(innerDatalistID, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            DataListUtil.MergeDataList(left, right, out invokeErrors);
            errors.MergeErrors(invokeErrors);
            compiler.PushBinaryDataList(left.UID, left, out invokeErrors);
            errors.MergeErrors(invokeErrors);

            return innerDatalistID;
        }

        /// <summary>
        /// Finds the service shape.
        /// </summary>
        /// <param name="workspaceID">The workspace ID.</param>
        /// <param name="serviceName">Name of the service.</param>
        /// <returns></returns>
        /// WARNING : WHY THE FLIP IS THIS LOGIC IN TWO PLACES? REMOVE ONE!
        public string FindServiceShape(Guid workspaceID, string serviceName, bool serviceInputs)
        {
            var services = ResourceCatalog.Instance.GetDynamicObjects<DynamicService>(workspaceID, serviceName);

            var tmp = services.FirstOrDefault();
            const string baseResult = "<ADL></ADL>";
            var result = "<DataList></DataList>";

            if (tmp != null)
            {
                result = tmp.DataListSpecification;

                // Handle services ;)
                if (result == baseResult && tmp.OutputSpecification == null)
                {
                    var serviceDef = tmp.ResourceDefinition;

                    ErrorResultTO errors;
                    if (!serviceInputs)
                    {
                        var outputMappings = ServiceUtils.ExtractOutputMapping(serviceDef);
                        result = DataListUtil.ShapeDefinitionsToDataList(outputMappings, enDev2ArgumentType.Output,
                                                                         out errors);
                    }
                    else
                    {
                        var inputMappings = ServiceUtils.ExtractInputMapping(serviceDef);
                        result = DataListUtil.ShapeDefinitionsToDataList(inputMappings, enDev2ArgumentType.Input,
                                                                         out errors);
                    }
                }
            }

            if (string.IsNullOrEmpty(result))
            {
                result = "<DataList></DataList>";
            }

            return result;
        }
    }
}
