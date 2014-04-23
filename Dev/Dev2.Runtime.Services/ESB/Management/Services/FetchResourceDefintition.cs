using System;
using System.Collections.Generic;
using System.Text;
using Dev2.Common.Common;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Util;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Fetch a service body definition
    /// </summary>
    public class FetchResourceDefintition : IEsbManagementEndpoint
    {
        const string payloadStart = "<XamlDefinition>";
        const string payloadEnd = "</XamlDefinition>";
        const string altPayloadStart = "<Actions>";
        const string altPayloadEnd = "</Actions>";

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {

            var res = new ExecuteMessage { HasError = false };

            string serviceID = null;
            StringBuilder tmp;
            values.TryGetValue("ResourceID", out tmp);

            if(tmp != null)
            {
                serviceID = tmp.ToString();
            }

            Guid resourceID;
            Guid.TryParse(serviceID, out resourceID);

            var result = ResourceCatalog.Instance.GetResourceContents(theWorkspace.ID, resourceID);
            var resource = ResourceCatalog.Instance.GetResource(theWorkspace.ID, resourceID);

            if(resource != null && resource.ResourceType == ResourceType.DbSource)
            {
                res.Message.Append(result);
            }
            else
            {
                var startIdx = result.IndexOf(payloadStart, 0, false);

                if(startIdx >= 0)
                {
                    // remove beginning junk
                    startIdx += payloadStart.Length;
                    result = result.Remove(0, startIdx);

                    startIdx = result.IndexOf(payloadEnd, 0, false);

                    if(startIdx > 0)
                    {
                        var len = result.Length - startIdx;
                        result = result.Remove(startIdx, len);

                        res.Message.Append(result.Unescape());
                    }
                }
                else
                {
                    // handle services ;)
                    startIdx = result.IndexOf(altPayloadStart, 0, false);
                    if(startIdx >= 0)
                    {
                        // remove begging junk
                        startIdx += altPayloadStart.Length;
                        result = result.Remove(0, startIdx);

                        startIdx = result.IndexOf(altPayloadEnd, 0, false);

                        if(startIdx > 0)
                        {
                            var len = result.Length - startIdx;
                            result = result.Remove(startIdx, len);

                            res.Message.Append(result.Unescape());
                        }
                    }
                    else
                    {
                        // send the entire thing ;)
                        res.Message.Append(result);
                    }
                }
            }

            // Finally, clean the definition as per execution hydration rules ;)
            Dev2XamlCleaner dev2XamlCleaner = new Dev2XamlCleaner();
            res.Message = dev2XamlCleaner.StripNaughtyNamespaces(res.Message);

            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            return serializer.SerializeToBuilder(res);
        }

        public DynamicService CreateServiceEntry()
        {
            var serviceAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var serviceEntry = new DynamicService { Name = HandlesType(), DataListSpecification = "<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>" };
            serviceEntry.Actions.Add(serviceAction);

            return serviceEntry;
        }

        public string HandlesType()
        {
            return "FetchResourceDefinitionService";
        }

    }
}
