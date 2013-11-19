using System;
using System.Collections.Generic;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Fetch a service body definition
    /// </summary>
    public class FetchResourceDefintition : IEsbManagementEndpoint
    {
        private static string payloadStart = "<XamlDefinition>";
        private static string payloadEnd = "</XamlDefinition>";
        private static string altPayloadStart = "<Actions>";
        private static string altPayloadEnd = "</Actions>";

        public string Execute(IDictionary<string, string> values, IWorkspace theWorkspace)
        {
            string serviceID;
            values.TryGetValue("ResourceID", out serviceID);
            Guid resourceID;
            Guid.TryParse(serviceID, out resourceID);

            var result = ResourceCatalog.Instance.GetResourceContents(theWorkspace.ID, resourceID);


            var startIdx = result.IndexOf(payloadStart, StringComparison.Ordinal);

            if (startIdx >= 0)
            {
                var endIdx = result.IndexOf(payloadEnd, StringComparison.Ordinal);

                if (endIdx > startIdx)
                {
                    startIdx += payloadStart.Length;
                    var len = endIdx - startIdx;
                    return result.Substring(startIdx, len);
                }
            }
            else
            {
                // handle services ;)
                startIdx = result.IndexOf(altPayloadStart, StringComparison.Ordinal);

                var endIdx = result.IndexOf(altPayloadEnd, StringComparison.Ordinal);

                if(endIdx > startIdx)
                {
                    startIdx += altPayloadStart.Length;
                    var len = endIdx - startIdx;
                    return result.Substring(startIdx, len);
                }
            }

            return string.Empty;
        }

        public DynamicService CreateServiceEntry()
        {
            var serviceAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var serviceEntry = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><ResourceID/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            serviceEntry.Actions.Add(serviceAction);

            return serviceEntry;
        }

        public string HandlesType()
        {
            return "FetchResourceDefinitionService";
        }

    }
}
