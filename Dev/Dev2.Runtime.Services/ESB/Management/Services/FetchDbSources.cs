using System;
using System.Collections.Generic;
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
            try
            {
                var list = Resources.GetResourceList<DbSource>(GlobalConstants.ServerWorkspaceID).Select(res =>
                {
                    return new DbSourceDefinition
                    {
                        AuthenticationType = res.AuthenticationType,
                        DbName = res.DatabaseName,
                        Id = res.ResourceID,
                        Name = res.ResourceName,
                        Path = res.GetSavePath(),
                        Password = res.Password,
                        ConnectionTimeout = res.ConnectionTimeout,
                        ServerName = res.Server,
                        Type = res.ServerType,
                        UserName = res.UserID
                    };
                });

                return serializer.SerializeToBuilder(new ExecuteMessage { HasError = false, Message = serializer.SerializeToBuilder(list) });
            }
            catch (Exception e)
            {
                Dev2Logger.Error("Error when trying to retrieve database sources " + e.Message, GlobalConstants.WarewolfError);
                return serializer.SerializeToBuilder(new ExecuteMessage
                {
                    HasError = true,
                    Message = new StringBuilder(e.Message)
                });
            }
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchDbSources";
    }
}
