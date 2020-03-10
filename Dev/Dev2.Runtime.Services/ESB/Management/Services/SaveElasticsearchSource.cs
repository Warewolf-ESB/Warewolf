/*
*  Warewolf - Once bitten, there's no going back
*  Copyright 2020 by Warewolf Ltd <alpha@warewolf.io>
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
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.Data.ServiceModel;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class SaveElasticsearchSource : IEsbManagementEndpoint
    {
        IResourceCatalog _resourceCatalog;

        public SaveElasticsearchSource()
        {

        }

        public SaveElasticsearchSource(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Elasticsearch Resource Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue(Warewolf.Service.SaveElasticsearchSource.ElasticsearchSource, out StringBuilder resourceDefinition);

                IElasticsearchSourceDefinition elasticsearchSourceDef = serializer.Deserialize<ElasticsearchSourceDefinition>(resourceDefinition);

                if (elasticsearchSourceDef.Path == null)
                {
                    elasticsearchSourceDef.Path = string.Empty;
                }

                if (elasticsearchSourceDef.Path.EndsWith("\\"))
                {
                    elasticsearchSourceDef.Path = elasticsearchSourceDef.Path.Substring(0, elasticsearchSourceDef.Path.LastIndexOf("\\", StringComparison.Ordinal));
                }

                var elasticsearchSource = new ElasticsearchSource
                {
                    ResourceID = elasticsearchSourceDef.Id,
                    HostName = elasticsearchSourceDef.HostName,
                    Port = elasticsearchSourceDef.Port,
                    AuthenticationType = elasticsearchSourceDef.AuthenticationType,
                    Password = elasticsearchSourceDef.Password, 
                    Username = elasticsearchSourceDef.Username, 
                    ResourceName = elasticsearchSourceDef.Name
                };

                ResourceCat.SaveResource(GlobalConstants.ServerWorkspaceID, elasticsearchSource, elasticsearchSourceDef.Path);
                msg.HasError = false;
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public IResourceCatalog ResourceCat
        {
            get => _resourceCatalog ?? ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public string HandlesType() => nameof(Warewolf.Service.SaveElasticsearchSource);
    }
}
