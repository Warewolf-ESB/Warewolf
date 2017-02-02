using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.ExtMethods;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FetchPluginConstructors : IEsbManagementEndpoint
    {
        private readonly IResourceCatalog _catalog;

        public FetchPluginConstructors(IResourceCatalog catalog)
        {
            _catalog = catalog;
        }

        public FetchPluginConstructors()
        {
        }


        public string HandlesType()
        {
            return "FetchPluginConstructors";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            Dev2JsonSerializer serializer = new Dev2JsonSerializer();
            try
            {

                var pluginSource = values["source"].DeserializeToObject<PluginSourceDefinition>();
                var ns = values["namespace"].DeserializeToObject<INamespaceItem>();
                // ReSharper disable MaximumChainedReferences
                var services = new PluginServices();
                var src = Resources.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, pluginSource.Id);
                var svc = new PluginService();
                if (ns != null)
                {
                    svc.Namespace = ns.FullName;
                    svc.Source = src;
                }
                else
                {
                    svc.Source = src;
                }
                var serviceConstructorList = services.Constructors(svc, Guid.Empty, Guid.Empty);
                List<IPluginConstructor> constructors = serviceConstructorList.Select(a => new PluginConstructor
                {
                    ConstructorName = BuildConstructorName(a.Parameters.Select(parameter => parameter.ShortTypeName)),
                    Inputs = a.Parameters.Cast<IConstructorParameter>().ToList(),
                } as IPluginConstructor).ToList();
                var executeMessage = new ExecuteMessage { HasError = false, Message = constructors.SerializeToJsonStringBuilder() };
                return executeMessage.SerializeToJsonStringBuilder();
            }
            catch (Exception e)
            {

                return serializer.SerializeToBuilder(new ExecuteMessage
                {
                    HasError = true,
                    Message = new StringBuilder(e.Message)
                });
            }
        }

        private string BuildConstructorName(IEnumerable<string> parameters)
        {
            var enumerable = parameters as string[] ?? parameters.ToArray();
            var name = new StringBuilder(".ctor ");
            if (enumerable.Any())
            {
                name.Append("(");
            }

            for (int index = 0; index < enumerable.Length; index++)
            {
                var parameter = enumerable[index];
                name.Append(parameter);
                name.Append(index == enumerable.Length - 1 ? ")" : ",");
            }

            return name.ToString();
        }

        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public IResourceCatalog Resources => _catalog ?? ResourceCatalog.Instance;

        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }
    }
}