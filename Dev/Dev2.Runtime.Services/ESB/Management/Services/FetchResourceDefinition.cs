/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2017 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Workspaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces;
using Dev2.Runtime.Interfaces;
using System.Diagnostics.CodeAnalysis;

namespace Dev2.Runtime.ESB.Management.Services
{
    /// <summary>
    /// Fetch a service body definition
    /// </summary>
    public class FetchResourceDefinition : IEsbManagementEndpoint
    {
        private IResourceDefinationCleaner resourceDefinationCleaner;
        private IResourceCatalog resourceCatalog;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            requestArgs.TryGetValue("ResourceID", out StringBuilder tmp);
            if (tmp != null)
            {
                if (Guid.TryParse(tmp.ToString(), out Guid resourceId))
                {
                    return resourceId;
                }
            }

            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.View;
        }

        [ExcludeFromCodeCoverage]
        public FetchResourceDefinition()
        {
        }
        
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            try
            {
                string serviceId = null;
                bool prepairForDeployment = false;
                values.TryGetValue(@"ResourceID", out StringBuilder tmp);

                if (tmp != null)
                {
                    serviceId = tmp.ToString();
                }

                values.TryGetValue(@"PrepairForDeployment", out tmp);

                if (tmp != null)
                {
                    prepairForDeployment = bool.Parse(tmp.ToString());
                }

                Guid.TryParse(serviceId, out Guid resourceId);

                Dev2Logger.Info($"Fetch Resource definition. ResourceId: {resourceId}", GlobalConstants.WarewolfInfo);
                if (resourceCatalog == null)
                {
                    resourceCatalog = ResourceCatalog.Instance;
                }
                if (resourceDefinationCleaner == null)
                {
                    resourceDefinationCleaner = new ResourceDefinationCleaner();
                }
                var result = resourceCatalog.GetResourceContents(theWorkspace.ID, resourceId);
                var resourceDefinition = resourceDefinationCleaner.GetResourceDefinition(prepairForDeployment, resourceId, result);

                return resourceDefinition;
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }



        public StringBuilder DecryptAllPasswords(StringBuilder stringBuilder)
        {
            return resourceDefinationCleaner.DecryptAllPasswords(stringBuilder);
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
            return @"FetchResourceDefinitionService";
        }

    }
}
