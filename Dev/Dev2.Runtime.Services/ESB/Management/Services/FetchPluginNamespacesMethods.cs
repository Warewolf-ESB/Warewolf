using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Warewolf.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class FetchPluginNamespacesMethods : IEsbManagementEndpoint
    {


        public string HandlesType()
        {
            return "FetchPluginActions";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var pluginSource = serializer.Deserialize<IPluginSource>(values["source"]);
                var ns = serializer.Deserialize<INamespaceItem>(values["namespace"]);
                // ReSharper disable MaximumChainedReferences
                ServiceModel.PluginServices services = new ServiceModel.PluginServices();
                var src = ResourceCatalog.Instance.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, pluginSource.Id);
                //src.AssemblyName = ns.FullName;
                PluginService svc = new PluginService {Namespace = ns.FullName , Source = src};

                var methods = services.Methods(serializer.Serialize(svc), Guid.Empty, Guid.Empty).Select(a => new PluginAction()
                {FullName  = ns.FullName,
                    Inputs = a.Parameters.Select(x=> new ServiceInput(x.Name,x.DefaultValue??""){Name = x.Name,DefaultValue = x.DefaultValue,EmptyIsNull = x.EmptyToNull,RequiredField = x.IsRequired,TypeName = x.Type} as IServiceInput).ToList(),
                 Method = a.Name,
                  Variables = a.Parameters.Select(x => new NameValue() { Name = x.Name+ " ("+ x.TypeName+")", Value = ""} as INameValue).ToList(),
                } as IPluginAction
                    ).ToList()  ;
                return serializer.SerializeToBuilder(new ExecuteMessage()
                {
                    HasError = false,
                    Message = serializer.SerializeToBuilder(methods)
                });
                // ReSharper restore MaximumChainedReferences
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



        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public ResourceCatalog Resources
        {
            get
            {
                return ResourceCatalog.Instance;
            }

        }
    }
}