#pragma warning disable
/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2019 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Interfaces;
using System.Diagnostics.CodeAnalysis;
using Dev2.Communication;
using System.Activities;
using System.Xaml;
using System.Activities.XamlIntegration;
using Newtonsoft.Json;
using Dev2.Common.Interfaces.Infrastructure.Communication;
using Dev2.Common.Common;
using Dev2.Common.X6;
using System.ComponentModel;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchJSONResourceDefinition : IEsbManagementEndpoint
    {
        private IResourceDefinationCleaner _resourceDefinationCleaner;
        private IResourceCatalog _resourceCatalog;
        public IResourceCatalog ResourceCat
        {
            private get
            {
                return _resourceCatalog ?? ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalog = value;
            }
        }

        public IResourceDefinationCleaner Cleaner
        {
            private get
            {
                return _resourceDefinationCleaner ?? new ResourceDefinationCleaner();
            }
            set
            {
                _resourceDefinationCleaner = value;
            }
        }

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("ResourceID", out StringBuilder tmp);
            if (tmp != null && Guid.TryParse(tmp.ToString(), out Guid resourceId))
            {
                return resourceId;
            }


            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.View;

        [ExcludeFromCodeCoverage]
        public FetchJSONResourceDefinition()
        {
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            var finalresult = new ExecuteMessage();

            try
            {
                string serviceId = null;
                var prepairForDeployment = false;
                values.TryGetValue(@"ResourceID", out StringBuilder tmp);

                if (tmp != null)
                    serviceId = tmp.ToString();

                values.TryGetValue(@"PrepairForDeployment", out tmp);

                if (tmp != null)
                    prepairForDeployment = bool.Parse(tmp.ToString());

                Guid.TryParse(serviceId, out Guid resourceId);

                Dev2Logger.Info($"Fetch JSON Resource definition. ResourceId: {resourceId}", GlobalConstants.WarewolfInfo);
                var resource = ResourceCat.GetResource(theWorkspace.ID, resourceId);
                var result = ResourceCat.GetResourceContents(theWorkspace.ID, resourceId);
                var serviceXaml = new StringBuilder(result.ToString());
                finalresult = (ExecuteMessage)Cleaner.GetRawResourceDefinition(prepairForDeployment, resourceId, result);

                if (finalresult != null && !finalresult.HasError)
                {

                    if (resource.IsServer || resource.IsSource)
                    {
                        var info = new X6RequestInfo() { ResourceName = resource.ResourceName, ActivityXaml = "", WorkflowXML = "" };
                        finalresult.Message = new StringBuilder(JsonConvert.SerializeObject(info));
                    }
                    else
                    {
                        var workflowXaml = new Dev2.Runtime.ServiceModel.Data.Workflow(serviceXaml.ToXElement(), true);
                        var info = new X6RequestInfo() { ResourceName = workflowXaml.ResourceName, ActivityXaml = finalresult.Message.ToString(), WorkflowXML = workflowXaml.ToServiceDefinition().ToString() };
                        finalresult.Message = new StringBuilder(JsonConvert.SerializeObject(info));
                    }
                }
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                finalresult.HasError = true;
                finalresult.Message = err.Message.ToStringBuilder();
            }
            return serializer.SerializeToBuilder(finalresult);

        }

        public StringBuilder DecryptAllPasswords(StringBuilder stringBuilder) => Cleaner.DecryptAllPasswords(stringBuilder);
        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => ServiceName();
        public static string ServiceName() => "FetchJSONResourceDefinitionService";
    }
}
