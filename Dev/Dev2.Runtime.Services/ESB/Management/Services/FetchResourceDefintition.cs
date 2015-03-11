
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
using System.Text;
using Dev2.Common;
using Dev2.Common.Common;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Data;
using Dev2.Communication;
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
        const string PayloadStart = "<XamlDefinition>";
        const string PayloadEnd = "</XamlDefinition>";
        const string AltPayloadStart = "<Actions>";
        const string AltPayloadEnd = "</Actions>";

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {


                var res = new ExecuteMessage { HasError = false };

                string serviceId = null;
                StringBuilder tmp;
                values.TryGetValue("ResourceID", out tmp);

                if (tmp != null)
                {
                    serviceId = tmp.ToString();
                }

                Guid resourceId;
                Guid.TryParse(serviceId, out resourceId);
                Dev2Logger.Log.Info("Fetch Resource definition. ResourceId:" + resourceId);
                try
                {
                    var result = ResourceCatalog.Instance.GetResourceContents(theWorkspace.ID, resourceId);
                    var resource = ResourceCatalog.Instance.GetResource(theWorkspace.ID, resourceId);

                    if (resource != null && resource.ResourceType == ResourceType.DbSource)
                    {
                        res.Message.Append(result);
                    }
                    else
                    {
                        var startIdx = result.IndexOf(PayloadStart, 0, false);

                        if (startIdx >= 0)
                        {
                            // remove beginning junk
                            startIdx += PayloadStart.Length;
                            result = result.Remove(0, startIdx);

                            startIdx = result.IndexOf(PayloadEnd, 0, false);

                            if (startIdx > 0)
                            {
                                var len = result.Length - startIdx;
                                result = result.Remove(startIdx, len);

                                res.Message.Append(result.Unescape());
                            }
                        }
                        else
                        {
                            // handle services ;)
                            startIdx = result.IndexOf(AltPayloadStart, 0, false);
                            if (startIdx >= 0)
                            {
                                // remove begging junk
                                startIdx += AltPayloadStart.Length;
                                result = result.Remove(0, startIdx);

                                startIdx = result.IndexOf(AltPayloadEnd, 0, false);

                                if (startIdx > 0)
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
                }
                catch (Exception e)
                {
                    Dev2Logger.Log.Error(string.Format("Error getting resource definition for: {0}", resourceId), e);
                }


                // Finally, clean the definition as per execution hydration rules ;)
                Dev2XamlCleaner dev2XamlCleaner = new Dev2XamlCleaner();
                res.Message = dev2XamlCleaner.StripNaughtyNamespaces(res.Message);

                Dev2JsonSerializer serializer = new Dev2JsonSerializer();
                return serializer.SerializeToBuilder(res);
            }
            catch (Exception err)
            {
                Dev2Logger.Log.Error(err);
                throw;
            }
        }

        public DynamicService CreateServiceEntry()
        {
            var serviceAction = new ServiceAction { Name = HandlesType(), SourceMethod = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService };

            var serviceEntry = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            serviceEntry.Actions.Add(serviceAction);

            return serviceEntry;
        }

        public string HandlesType()
        {
            return "FetchResourceDefinitionService";
        }

    }
}
