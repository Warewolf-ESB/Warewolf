using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Runtime.Interfaces;
using Dev2.Services.Security;
using Unlimited.Framework.Converters.Graph.Ouput;

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class TestWcfService : IEsbManagementEndpoint
    {
        private IResourceCatalog _rescat;



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
                Dev2Logger.Info("Test Wcf Service");
                StringBuilder resourceDefinition;

                values.TryGetValue("wcfService", out resourceDefinition);
                IWcfService service = serializer.Deserialize<IWcfService>(resourceDefinition);

                var source = ResourceCatalog.Instance.GetResource<WcfSource>(GlobalConstants.ServerWorkspaceID, service.Source.Id);
                var parameters = service.Inputs?.Select(a => new MethodParameter() { EmptyToNull = a.EmptyIsNull, IsRequired = a.RequiredField, Name = a.Name, Value = a.Value, TypeName = a.TypeName }).ToList() ?? new List<MethodParameter>();

                var res = new WcfService
                {
                    Method = new ServiceMethod(service.Action.Method, service.Name, parameters, new OutputDescription(), new List<MethodOutput>(), service.Action.Method),
                    ResourceName = service.Name,
                    ResourceID = service.Id,
                    Source = source
                };

                var services = new ServiceModel.Services();

                var output = services.WcfTest(res, GlobalConstants.ServerWorkspaceID, Guid.Empty);

                msg.HasError = false;
                msg.Message = serializer.SerializeToBuilder(new RecordsetListWrapper() { Description = output.Description, RecordsetList = output });
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err);
            }

            return serializer.SerializeToBuilder(msg);
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

        public string HandlesType()
        {
            return "TestWcfService";
        }
    }
}