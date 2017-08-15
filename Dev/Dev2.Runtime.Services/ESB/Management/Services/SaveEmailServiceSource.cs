using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Dev2.Common;
using Dev2.Common.Interfaces;
using Dev2.Common.Interfaces.Enums;
using Dev2.Common.Interfaces.Infrastructure;
using Dev2.Communication;
using Dev2.DynamicServices;
using Dev2.DynamicServices.Objects;
using Dev2.Runtime.Hosting;
using Dev2.Runtime.Interfaces;
using Dev2.Runtime.ServiceModel.Data;
using Dev2.Workspaces;


namespace Dev2.Runtime.ESB.Management.Services
{

    public class SaveEmailServiceSource : IEsbManagementEndpoint
    {
        IExplorerServerResourceRepository _serverExplorerRepository;
        IResourceCatalog _resourceCatalogue;

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
                Dev2Logger.Info("Save Email Service Source");
                StringBuilder resourceDefinition;

                values.TryGetValue("EmailServiceSource", out resourceDefinition);

                IEmailServiceSource src = serializer.Deserialize<IEmailServiceSource>(resourceDefinition);
                EmailSource con = new EmailSource
                {
                    Host = src.HostName,
                    UserName = src.UserName,
                    Password = src.Password,
                    Port = src.Port,
                    EnableSsl = src.EnableSsl,
                    Timeout = src.Timeout,
                    ResourceName = src.ResourceName,
                    ResourceID = src.Id
                };
                ResourceCatalog.Instance.SaveResource(GlobalConstants.ServerWorkspaceID, con, src.Path);
                ServerExplorerRepo.UpdateItem(con);

                msg.HasError = false;
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
            DynamicService newDs = new DynamicService { Name = HandlesType(), DataListSpecification = new StringBuilder("<DataList><Roles ColumnIODirection=\"Input\"/><EmailServiceSource ColumnIODirection=\"Input\"/><WorkspaceID ColumnIODirection=\"Input\"/><Dev2System.ManagmentServicePayload ColumnIODirection=\"Both\"></Dev2System.ManagmentServicePayload></DataList>") };
            ServiceAction sa = new ServiceAction { Name = HandlesType(), ActionType = Common.Interfaces.Core.DynamicServices.enActionType.InvokeManagementDynamicService, SourceMethod = HandlesType() };
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
                return _resourceCatalogue?? ResourceCatalog.Instance;
            }
            set
            {
                _resourceCatalogue = value;
            }
        }

        public string HandlesType()
        {
            return "SaveEmailServiceSource";
        }
    }
}