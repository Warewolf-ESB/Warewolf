using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Common.Interfaces.Studio.ViewModels.Dialogues;
using Dev2.Common.Interfaces.WebServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;
using Warewolf.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    public class TestPluginService : IEsbManagementEndpoint
        // ReSharper restore UnusedMember.Global
    {
        IResourceCatalog _rescat;
        IPluginServices _pluginServices;

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Log.Info("Test Plugin Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("PluginService", out resourceDefinition);
                IPluginService src = serializer.Deserialize<IPluginService>(resourceDefinition);
           
                var methods = GetMethods(serializer,src.Source.Id,src.Action.FullName).First(a=>a.Method==src.Action.Method);
                
                // ReSharper disable MaximumChainedReferences
                var parameters = src.Inputs==null?new List<MethodParameter>(): src.Inputs.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value,Type=a.TypeName}).ToList();
                // ReSharper restore MaximumChainedReferences
                var pluginsrc = ResourceCatalog.Instance.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                var res = new PluginService
                {
                    Method = new ServiceMethod(src.Action.Method, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(),"test")
                    ,
                    Namespace = src.Action.FullName,
                    ResourceName = src.Name,
                    ResourcePath = src.Path,
                    ResourceID = src.Id,
                    Source = pluginsrc
                };

                var result =PluginServices.Test(serializer.SerializeToBuilder(res).ToString(), Guid.Empty, Guid.Empty);
                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(result);
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Log.Error(err);

            }

            return serializer.SerializeToBuilder(msg);
        }

        static List<IPluginAction> GetMethods(Dev2JsonSerializer serializer,Guid srcId,string ns)
        {
            ServiceModel.PluginServices services = new ServiceModel.PluginServices();
            var src = ResourceCatalog.Instance.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, srcId);
            //src.AssemblyName = ns.FullName;
            PluginService svc = new PluginService { Namespace = ns, Source = src };

            var methods = services.Methods(serializer.Serialize(svc), Guid.Empty, Guid.Empty).Select(a => new PluginAction()
            {
                FullName = a.Name,
                Inputs = a.Parameters.Select(x => new ServiceInput(x.Name, x.DefaultValue ?? "") { Name = x.Name, DefaultValue = x.DefaultValue, EmptyIsNull = x.EmptyToNull, RequiredField = x.IsRequired, TypeName = x.Type } as IServiceInput).ToList(),
                Method = a.Name,
                Variables = a.Parameters.Select(x => new NameValue() { Name = x.Name + " (" + x.TypeName + ")", Value = "" } as INameValue).ToList(),
            } as IPluginAction
                ).ToList();
            return methods;
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><PluginService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public IResourceCatalog ResourceCatalogue
        {
            get
            {
                return _rescat ?? ResourceCatalog.Instance;
            }
            set
            {
                _rescat = value;
            }
        }
        public IPluginServices PluginServices
        {
            get
            {
                return _pluginServices ?? new PluginServices();
            }
            set
            {
                _pluginServices = value;
            }
        }

        public string HandlesType()
        {
            return "TestPluginService";
        }
    }
}