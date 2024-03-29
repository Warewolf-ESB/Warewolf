#pragma warning disable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Warewolf.Core;
using Task = System.Threading.Tasks.Task;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchComPluginActions : DefaultEsbManagementEndpoint
    {
        public override StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var pluginSource = serializer.Deserialize<ComPluginSourceDefinition>(values["source"]);
                var ns = serializer.Deserialize<INamespaceItem>(values["namespace"]);

                var services = new ComPluginServices();
                var src = ResourceCatalog.Instance.GetResource<ComPluginSource>(GlobalConstants.ServerWorkspaceID, pluginSource.Id);
                var svc = new ComPluginService();
                if (ns != null)
                {
                    svc.Namespace = ns.FullName;
                    svc.Source = src;
                }
                else
                {
                    svc.Source = src;
                }


                var serviceMethodList = new ServiceMethodList();
                var task = Task.Run(() =>
                {
                    return serviceMethodList = services.Methods(svc, Guid.Empty, Guid.Empty);
                });
                try
                {
                    var timeoutAfter = task.TimeoutAfter(TimeSpan.FromSeconds(3));
                    serviceMethodList = timeoutAfter.Result;
                }
                catch (Exception e)
                {
                    Dev2Logger.Error(e, GlobalConstants.WarewolfError);
                }


                var methods = serviceMethodList.Select(a => new PluginAction
                {
                    FullName = ns?.FullName,
                    Inputs = a.Parameters.Select(x =>
                    new ServiceInput(x.Name, x.DefaultValue ?? "")
                    {
                        Name = BuildServiceInputName(x.Name, x.TypeName)
                        ,
                        EmptyIsNull = x.EmptyToNull
                        ,
                        RequiredField = x.IsRequired
                        ,
                        TypeName = x.TypeName
                    } as IServiceInput).ToList(),
                    Method = a.Name,
                    Variables = a.Parameters.Select(x => new NameValue { Name = x.Name + " (" + x.TypeName + ")", Value = "" } as INameValue).ToList(),
                } as IPluginAction).ToList();
                return serializer.SerializeToBuilder(new ExecuteMessage
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

        string BuildServiceInputName(string name, string typeName)
        {
            try
            {
                var cleanTypeName = Type.GetType(typeName);

                return $"{name} ({cleanTypeName.Name})";
            }
            catch (Exception)
            {
                try
                {
                    var cleanTypeName = typeName.Contains("&") ? typeName.Split('&').First() : typeName.Split(',').First();
                    var newName = $"{name} ({cleanTypeName})";
                    return newName;
                }
                catch (Exception)
                {
                    return name;
                }
            }
        }

        public IResourceCatalog Resources => ResourceCatalog.Instance;

        public override DynamicService CreateServiceEntry() => EsbManagementServiceEntry.CreateESBManagementServiceEntry(HandlesType(), "<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>");

        public override string HandlesType() => "FetchComPluginActions";
    }
}