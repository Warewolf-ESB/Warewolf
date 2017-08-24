using Dev2.Common;
using Dev2.Common.Interfaces.Core;
using Dev2.Common.Interfaces.Core.DynamicServices;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common.Interfaces.Enums;
using Dev2.Runtime.Interfaces;



namespace Dev2.Runtime.ESB.Management.Services
{

    public class SaveWcfServiceSource : IEsbManagementEndpoint
    {
        private IExplorerServerResourceRepository _serverExplorerRepository;
        private IResourceCatalog _resourceCatalogue;


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
                Dev2Logger.Info("Save Wcf Service Source", GlobalConstants.WarewolfInfo);
                StringBuilder resourceDefinition;

                values.TryGetValue("WcfSource", out resourceDefinition);

                var src = serializer.Deserialize<WcfServiceSourceDefinition>(resourceDefinition);
                var con = new WcfSource
                {
                    EndpointUrl = src.EndpointUrl,
                    ResourceName = src.Name,
                    Name = src.Name,
                    ResourceID = src.Id,
                    Type = enSourceType.WcfSource,
                    ResourceType = "WcfSource"
                };
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, con, src.Path);
                ServerExplorerRepo.UpdateItem(con);

                msg.HasError = false;
            }
            catch (Exception err)
            {
                msg.HasError = true;
                msg.Message = new StringBuilder(err.Message);
                Dev2Logger.Error(err, GlobalConstants.WarewolfError);
            }

            return serializer.SerializeToBuilder(msg);
        }

        public DynamicService CreateServiceEntry()
        {
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
            newDs.Actions.Add(sa);

            return newDs;
        }

        public IExplorerServerResourceRepository ServerExplorerRepo
        {
            get { return _serverExplorerRepository ?? ServerExplorerRepository.Instance; }
            set { _serverExplorerRepository = value; }
        }

        public IResourceCatalog ResourceCatalogue
        {
            get
            {
                return _resourceCatalogue ?? ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalogue = value;
            }
        }

        public string HandlesType()
        {
            return "SaveWcfServiceSource";
        }
    }
}