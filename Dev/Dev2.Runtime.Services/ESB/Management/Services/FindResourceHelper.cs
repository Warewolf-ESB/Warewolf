using System.Collections.Generic;
using System.Linq;
using Dev2.Common;
using Dev2.Data.ServiceModel;
using Dev2.Providers.Errors;
using Dev2.Runtime.ServiceModel.Data;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FindResourceHelper
    {
        /// <summary>
        /// Strips for ship.
        /// </summary>
        /// <param name="resource">The resource.</param>
        /// <returns></returns>
        public SerializableResource SerializeResourceForStudio(IResource resource)
        {

            // convert the fliping errors due to json issues in c# ;(
            List<ErrorInfo> errors = new List<ErrorInfo>();
            var parseErrors = resource.Errors;
            if(parseErrors != null)
            {
                errors.AddRange(parseErrors.Select(error => (error as ErrorInfo)));
            }

            var datalist = "<DataList></DataList>";

            if(resource.DataList != null)
            {
                datalist = resource.DataList.Replace("\"", GlobalConstants.SerializableResourceQuote).Replace("'", GlobalConstants.SerializableResourceSingleQuote);
            }

            return new SerializableResource
            {
                ResourceCategory = resource.ResourcePath,
                ResourceID = resource.ResourceID,
                ResourceName = resource.ResourceName,
                ResourceType = resource.ResourceType,
                IsValid = resource.IsValid,
                DataList = datalist,
                Errors = errors,
                IsNewResource = resource.IsNewResource
            };
        }
    }
}
