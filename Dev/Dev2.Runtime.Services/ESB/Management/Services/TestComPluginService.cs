using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Enums;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    
    public class TestComPluginService : IEsbManagementEndpoint
    {


        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Contribute;
        }
        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            ExecuteMessage msg = new ExecuteMessage();
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                Dev2Logger.Info("Test ComPlugin Service", "Warewolf Info");
                StringBuilder resourceDefinition;

                values.TryGetValue("ComPluginService", out resourceDefinition);
                IComPluginService src = serializer.Deserialize<IComPluginService>(resourceDefinition);

                
                var parameters = src.Inputs?.Select(a => new MethodParameter { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();
                
                var pluginsrc = ResourceCatalog.Instance.GetResource<ComPluginSource>(GlobalConstants.ServerWorkspaceID, src.Source.Id);
                var res = new ComPluginService
                {
                    Method = new ServiceMethod(src.Action.Method, src.Name, parameters, new OutputDescription(), new List<MethodOutput>(), "test"),
                    Namespace = src.Action.FullName,
                    ResourceName = src.Name,
                    ResourceID = src.Id,
                    Source = pluginsrc
                };

                string serializedResult;
                var result = _pluginServices.Value.Test(serializer.SerializeToBuilder(res).ToString(), out serializedResult);

                if (serializedResult.StartsWith("Exception: "))
                {
                    msg.HasError = true;
                    msg.Message = new StringBuilder(serializedResult);
                    Dev2Logger.Error(serializedResult, "Warewolf Error");
                }
                else
                {
                    msg.HasError = false;
                    msg.Message = serializer.SerializeToBuilder(new RecordsetListWrapper { Description = result.Description, RecordsetList = result, SerializedResult = serializedResult });
                }
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, "Warewolf Error");
            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><ComPluginService ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        private readonly Lazy<IComPluginServices> _pluginServices =new Lazy<IComPluginServices>(()=> new ComPluginServices());

        public string HandlesType()
        {
            return "TestComPluginService";
        }
    }
}