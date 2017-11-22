﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchComPluginNameSpaces : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var dbSource = serializer.Deserialize<ComPluginSourceDefinition>(values["source"]);
                
                var services = new ComPluginServices();
                var src = ResourceCatalog.Instance.GetResource<ComPluginSource>(GlobalConstants.ServerWorkspaceID, dbSource.Id);
                var methods = new List<INamespaceItem>();
                var task = Task.Run(() =>
                {
                    return methods = services.Namespaces(src, Guid.Empty, Guid.Empty).Select(a => a as INamespaceItem).ToList();
                });
                try
                {
                    var timeoutAfter = task.TimeoutAfter(TimeSpan.FromSeconds(3));
                    methods = timeoutAfter.Result;
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                }
             
                return serializer.SerializeToBuilder(new ExecuteMessage()
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder(methods)
                });
                
            }
            catch (Exception e)
            {
                return serializer.SerializeToBuilder(new ExecuteMessage()
                {
                    HasError = true,
                    Message = new StringBuilder(e.Message)
                });
            }
        }

        public ResourceCatalog Resources => ResourceCatalog.Instance;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchComPluginNameSpaces";
    }
}