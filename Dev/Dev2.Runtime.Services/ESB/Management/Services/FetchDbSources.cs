﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchDbSources : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            
            var list = Resources.GetResourceList<DbSource>(GlobalConstants.ServerWorkspaceID).Select(a =>
            {
                if (a is DbSource res)
                {
                    return new DbSourceDefinition
                    {
                        AuthenticationType = res.AuthenticationType,
                        DbName = res.DatabaseName,
                        Id = res.ResourceID,
                        Name = res.ResourceName,
                        Path = res.GetSavePath(),
                        Password = res.Password,
                        ServerName = res.Server,
                        Type = res.ServerType,
                        UserName = res.UserID
                    };
                }
                return null;
            }).ToList();

            return serializer.SerializeToBuilder(new ExecuteMessage { HasError = false, Message = serializer.SerializeToBuilder(list) });
            
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchDbSources";
    }
}
