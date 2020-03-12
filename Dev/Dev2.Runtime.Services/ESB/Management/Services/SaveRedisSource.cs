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
    public class SaveRedisSource : EsbManagementEndpointBase
    {
        IResourceCatalog _resourceCatalog;

        public SaveRedisSource()
        {

        }

        public SaveRedisSource(IResourceCatalog resourceCatalog)
        {
            _resourceCatalog = resourceCatalog;
        }

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Roles ColumnIODirection=\"Input\"/><ServerSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var msg = new ExecuteMessage();
            var serializer = new Dev2JsonSerializer();

            try
            {
                Dev2Logger.Info("Save Redis Resource Service", GlobalConstants.WarewolfInfo);

                values.TryGetValue(Warewolf.Service.SaveRedisSource.RedisSource, out StringBuilder resourceDefinition);

                IRedisServiceSource redisSourceDef = serializer.Deserialize<RedisSourceDefinition>(resourceDefinition);

                if (redisSourceDef.Path == null)
                {
                    redisSourceDef.Path = string.Empty;
                }

                if (redisSourceDef.Path.EndsWith("\\"))
                {
                    redisSourceDef.Path = redisSourceDef.Path.Substring(0, redisSourceDef.Path.LastIndexOf("\\", StringComparison.Ordinal));
                }

                var redisSource = new RedisSource
                {
                    ResourceID = redisSourceDef.Id,
                    HostName = redisSourceDef.HostName,
                    Port = redisSourceDef.Port,
                    AuthenticationType = redisSourceDef.AuthenticationType,
                    Password = redisSourceDef.Password, 
                    ResourceName = redisSourceDef.Name
                };

                ResourceCat.SaveResource(GlobalConstants.ServerWorkspaceID, redisSource, redisSourceDef.Path);
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

        public  IResourceCatalog ResourceCat
        {
            get => _resourceCatalog ?? ResourceCatalog.Instance;
            set => _resourceCatalog = value;
        }

        public override AuthorizationContext GetAuthorizationContextForService() => AuthorizationContext.Contribute;

        public override Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs) => Guid.Empty;

        public override string HandlesType() => nameof(Warewolf.Service.SaveRedisSource);
    }
}
