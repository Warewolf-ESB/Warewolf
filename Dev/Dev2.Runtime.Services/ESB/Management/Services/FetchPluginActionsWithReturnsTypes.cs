using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.DB;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Services.Security;
using Dev2.Workspaces;
using Warewolf.Core;

namespace Dev2.Runtime.ESB.Management.Services
{
    [SuppressMessage("ReSharper", "UnusedMember.Global")]
    public class FetchPluginActionsWithReturnsTypes : IEsbManagementEndpoint
    {
        private readonly IResourceCatalog _catalog;

        public FetchPluginActionsWithReturnsTypes(IResourceCatalog catalog)
        {
            _catalog = catalog;
        }

        public FetchPluginActionsWithReturnsTypes()
        {

        }
        public Guid GetResourceID(Dictionary<string, StringBuilder> requestArgs)
        {
            return Guid.Empty;
        }

        public AuthorizationContext GetAuthorizationContextForService()
        {
            return AuthorizationContext.Any;
        }

        public string HandlesType()
        {
            return "FetchPluginActionsWithReturnsTypes";
        }

        public StringBuilder Execute(Dictionary<string, StringBuilder> values, IWorkspace theWorkspace)
        {
            var serializer = new Dev2JsonSerializer();
            try
            {
                var pluginSource = serializer.Deserialize<PluginSourceDefinition>(values["source"]);
                var ns = serializer.Deserialize<INamespaceItem>(values["namespace"]);
                // ReSharper disable MaximumChainedReferences
                PluginServices services = new PluginServices();
                var src = Resources.GetResource<PluginSource>(GlobalConstants.ServerWorkspaceID, pluginSource.Id);
                //src.AssemblyName = ns.FullName;
                if (ns != null)
                {
                    PluginService svc = new PluginService { Namespace = ns.FullName, Source = src };

                    var serviceMethodList = services.MethodsWithReturns(svc, Guid.Empty, Guid.Empty);
                    List<IPluginAction> methods = serviceMethodList.Select(a => new PluginAction
                    {
                        FullName = ns.FullName,
                        Inputs = a.Parameters.Select(x => new ServiceInput(x.Name, x.DefaultValue ?? "")
                        {
                            Name = x.Name
                            ,
                            EmptyIsNull = x.EmptyToNull
                            ,
                            RequiredField = x.IsRequired
                            ,
                            TypeName = x.TypeName
                            ,
                            ShortTypeName = x.ShortTypeName
                            ,
                            IsObject = x.IsObject
                            ,
                            Dev2ReturnType = x.Dev2ReturnType,
                            
                            IntellisenseFilter = x.IsObject ? enIntellisensePartType.JsonObject : enIntellisensePartType.All

                        } as IServiceInput).ToList(),
                        Method = a.Name,
                        Variables = a.Parameters.Select(x => new NameValue { Name = x.Name + " (" + x.TypeName + ")", Value = "" } as INameValue).ToList(),
                        Dev2ReturnType = a.Dev2ReturnType,
                        IsObject = a.IsObject,                        
                        IsVoid = a.IsVoid,
                        IsProperty = a.IsProperty
                    } as IPluginAction).ToList();
                    return serializer.SerializeToBuilder(new ExecuteMessage
                    {
                        HasError = false,
                        Message = serializer.SerializeToBuilder(methods)
                    });
                }
                // ReSharper disable once RedundantIfElseBlock
                else
                {
                    return serializer.SerializeToBuilder(new ExecuteMessage
                    {
                        HasError = false,
                        Message = serializer.SerializeToBuilder(new List<IPluginAction>())
                    });
                }

                // ReSharper restore MaximumChainedReferences
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



        public DynamicService CreateServiceEntry()
        {
            var findServices = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };

            var fetchItemsAction = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };

            findServices.Actions.Add(fetchItemsAction);

            return findServices;
        }

        public IResourceCatalog Resources => _catalog ?? ResourceCatalog.Instance;
    }
}