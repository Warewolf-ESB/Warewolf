/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2018 by Warewolf Ltd <alpha@warewolf.io>
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

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchResourceDefinition : IEsbManagementEndpoint
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
        public FetchResourceDefinition()
        {
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                string serviceId = null;
                var prepairForDeployment = false;
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
                var result = ResourceCat.GetResourceContents(theWorkspace.ID, resourceId);
                var resourceDefinition = Cleaner.GetResourceDefinition(prepairForDeployment, resourceId, result);

                return resourceDefinition;
            }
            catch (Exception err)
            {
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
                throw;
            }
        }

        public StringBuilder DecryptAllPasswords(StringBuilder stringBuilder) => Cleaner.DecryptAllPasswords(stringBuilder);
        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><ResourceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public string HandlesType() => @"FetchResourceDefinitionService";

    }
}
